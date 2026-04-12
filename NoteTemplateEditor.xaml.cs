/*
* Jotter
* C.Winters / US / Arizona / Thinkpad T15g
* March 2024
*/

using com.nobodynoze.flogger;
using com.nobodynoze.notemanager;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jotter.MainWindow;

namespace Jotter
{
    /// <summary>
    /// NoteTemplateEditor is the child template winodws for notes.
    /// </summary>
    public partial class NoteTemplateEditor : Window
    {
        public ObservableCollection<Note> Notes { get; set; }
        public NoteManager? noteManager;
        public Note SelectedNote { get; set; }

        public event EventHandler<NoteEventArgs> NoteUpdated;

        // Save data after timer triggers. When user enteres
        // data on note, after the delay, data is saved.
        // Delay is in milliseconds - see "Initialize the timer for auto-saving" below
        private DispatcherTimer saveTimer;
        //TODO - Settings feature can be set for save delay
        private const int SaveDelayMilliseconds = 25000; 
        private DateTime lastActivityTime;
        public readonly string UI_TEXTBOX_STR_SEARCH = "Search...";
        // Keep the current note-search results so F3 / Shift+F3 can move between matches.
        private readonly List<TextRange> searchMatches = new List<TextRange>();
        // Track which highlighted match is currently selected while cycling search results.
        private int currentSearchMatchIndex = -1;
        // Track whether current spotlights came from editor text selection instead of explicit search.
        private bool isSelectionSpotlightActive = false;

        // Minimum length of selected text to trigger selection-based spotlighting. This prevents the
        // feature from activating on short selections that are likely not meaningful words,
        // such as single characters or common short words. Setting this to 2 means that the
        // user must select at least 2 characters for the spotlighting to activate, which helps
        // ensure that the feature is used for actual word selections rather than incidental cursor
        // placements or short selections.
        // TODO: Should include a MAXMIMUM_SPOTLIGHT_SELECTION_LENGTH to prevent excessively long
        // selections from triggering the feature, which could cause performance issues.
        // For example, setting a maximum length of around 30 characters could be reasonable to
        // allow for longer phrases while still preventing very long selections from causing problems.
        // Noticed that VSCode does a maximum of 200.
        //This is found in function RchEditNote_SelectionChanged()
        private const int MINIMUM_SPOTLIGHT_SELECTION_LENGTH = 2;

        // Prevent selection-change reentry while code updates the editor selection.
        private bool isHandlingEditorSelectionChange = false;
        // Debounce selection-based highlighting so normal caret movement does not thrash the editor.
        // RichTextBox fires a lot of selection notifications while the mouse is dragging or while the
        // caret is moving with the keyboard, so we wait briefly before searching and painting matches.
        private readonly DispatcherTimer selectionSpotlightTimer;
        // Avoid re-running the same selection highlight repeatedly.
        // If the user keeps the same word selected, there is no reason to rebuild the same highlight set.
        private string lastSelectionSpotlightText = string.Empty;

        /// <summary>
        /// RichTextSearchData is a helper class that encapsulates the data structures needed to 
        /// perform normalized search on a RichTextBox document.
        /// </summary>
        private sealed class RichTextSearchData
        {
            public List<TextPointer> RawTextPointers { get; set; } = new List<TextPointer>();
            public string NormalizedText { get; set; } = string.Empty;
            public List<int> NormalizedToRawIndexMap { get; set; } = new List<int>();
        }


        //Main Note template application child window
        public NoteTemplateEditor(Note note)
        {
            noteManager = new NoteManager();
            // Bind to the Notes collection of NoteManager
            Notes = noteManager.Notes;

            /////////////////////////////////////////
            // Initialize the timer for auto-saving
            // Auto saving wil lhelp retain the data after
            // a short period of time.
            saveTimer = new DispatcherTimer();
            saveTimer.Interval = TimeSpan.FromMilliseconds(SaveDelayMilliseconds);
            saveTimer.Tick += SaveTimer_Tick;

            selectionSpotlightTimer = new DispatcherTimer();
            selectionSpotlightTimer.Interval = TimeSpan.FromMilliseconds(225);
            selectionSpotlightTimer.Tick += SelectionSpotlightTimer_Tick;

            // Initialize last activity time
            lastActivityTime = DateTime.Now;
            /////////////////////////////////////////

            //DataContext = this;
            DataContext = note;

            //SelectedNote = selectedNote;

            InitializeComponent();

            // Subscribe to text changed event of the RichTextBox tracking user activity
            rchEditNote.TextChanged += RchEditNote_TextChanged;
            rchEditNote.SelectionChanged += RchEditNote_SelectionChanged;
            // SelectionChanged is the broad signal that the editor selection moved.
            // MouseUp and KeyUp are the "selection is likely finished" signals that make the
            // feature feel closer to editors like VS Code without constantly repainting highlights.
            // To disable selection-based spotlighting later, comment out the three event hookups below.
            rchEditNote.PreviewMouseLeftButtonUp += RchEditNote_PreviewMouseLeftButtonUp;
            rchEditNote.KeyUp += RchEditNote_KeyUp;

            //Set the selected note
            SelectedNote = note;
            // Initialize SelectedNote with a new Note instance having a unique IdIndexer
            //SelectedNote = new Note { IdIndexer = noteManager.GenerateID(), Title = note.Title, Text = note.Text };
            //NOTE: The selectednote was changed back from above to  = note. This fixed the data exchange
            //Seems to be behaving at the moment, but when clikcing new note, the title should be allowed to change.

            // Load the note content into the editor
            LoadNoteContent(note.Title, note.Text);
            RefreshMediaStrip();
        }


        //Same with the mainwindow - drag window since windowstyle = none
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }


        /// <summary>
        /// Load the note into the window for updating or editing
        /// </summary>
        /// <param name="noteTitle"></param>
        /// <param name="noteContents"></param>
        public void LoadNoteContent(string noteTitle, string noteContents)
        {
            // Set the text of the TextBox (XAML: TextBox Name="StickyNotesTitle")
            // to the note's title
            NotesTitle.Text = noteTitle;
            this.Title = noteTitle;

            //Set the richedit with the note contents
            //XAML RichTextBox x:Name="rchEditNote"
            rchEditNote.Document.Blocks.Clear();
            rchEditNote.Document.Blocks.Add(new Paragraph(new Run(noteContents)));

            // Apply the current theme's editor text color to the loaded document.
            TextRange documentRange = new TextRange(rchEditNote.Document.ContentStart, rchEditNote.Document.ContentEnd);
            documentRange.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("NoteEditForegroundBrush"));

            if (SelectedNote?.Media != null)
            {
                foreach (MediaItem mediaItem in SelectedNote.Media)
                {
                    logger.LogInfo($"[NoteTemplateEditor] Media load original file: {mediaItem.OriginalFileName}");
                    logger.LogInfo($"[NoteTemplateEditor] Media load stored ref: {mediaItem.MediaRef}");
                }
            }

            RefreshMediaStrip();
        }

        //Update the current note with the text from the richedit. We are ready to process it...
        private void UpdateSelectedNoteContent()
        {
            SelectedNote.Text = new TextRange(rchEditNote.Document.ContentStart, rchEditNote.Document.ContentEnd).Text;
        }

        //Kept in because the add/remove buttons were on each note. 
        //This wil be removed soon.
        public void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow myObject = new MainWindow();
            //myObject.DeleteNote_Click(this, e);
        }

        //Exiting the child window and save the note, unsubscribing from auto-save
        private void ExitCurrentNote_Click(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from the events for the auto save. No need if we close the window.
            saveTimer.Tick += SaveTimer_Tick;
            rchEditNote.TextChanged += RchEditNote_TextChanged;

            // Verify if there's a selected note and save its contents
            if (SelectedNote != null)
            {
                //SaveNoteChanges1();
                SaveNoteChanges();
            }
            this.Close();
        }

        // SAve the note, with indexing. See "HELLO to SELF!"
        private void SaveNoteChanges()
        {
            // Check if the note already exists in the Notes collection based on IdIndexer
            //Note existingNote = Notes.FirstOrDefault(n => n.IdIndexer == SelectedNote.IdIndexer);

            // Check if the note already exists in the Notes collection based on IdIndexer
            Note? existingNote = null;

            foreach (Note note in Notes)
            {
                if (note.IdIndexer == SelectedNote.IdIndexer)
                {
                    existingNote = note;
                    break;
                }
            }

            if (existingNote != null)
            {
                // Update the existing note with the content of SelectedNote
                UpdateSelectedNoteContent();
                existingNote.Title = SelectedNote.Title;
                existingNote.Text = SelectedNote.Text;
                // Instead of directly assigning the Media list, we create a deep copy to avoid unintended side effects in the UI.
                // This allows the editor to work with a copy of the media items, and only update the original list when changes are saved.
                existingNote.Media = CloneMediaItems(SelectedNote.Media);
            }
            else
                // Add the SelectedNote to the Notes collection if it doesn't exist
                Notes.Add(SelectedNote);

            // Remove the SelectedNote from the Notes collection if it should be deleted
            if (SelectedNote.IsDeleted)
            {
                NoteMediaStorage.DeleteMediaFiles(existingNote?.Media ?? SelectedNote.Media);

                if (existingNote != null)
                    Notes.Remove(existingNote);
                else
                    Notes.Remove(SelectedNote);
            }

            // Save the notes using the NoteManager
            noteManager?.SaveNotes(jotNotesFilePath);

            // Refresh the Notes collection
            Notes = noteManager?.Notes;

            //ADDED event handler to handle note update. Needed for the search stuff
            //If user searches, finds the note, edits it, closes it, the the UI wasn't updated.
            //The note and listview ui needs to be updated
            // ref: mainwindow.xaml.cs see NoteEditor_NoteUpdated()
            NoteUpdated?.Invoke(this, new NoteEventArgs(SelectedNote));

        }

        /// <summary>
        /// This method creates a deep copy of the media items list to ensure that changes to 
        /// the media items in the editor do not affect the original media items in the note 
        /// until changes are saved. This is important for maintaining data integrity and allowing 
        /// users to cancel changes without affecting the original media references.
        /// </summary>
        /// <param name="sourceMedia">The list of media items to be cloned.</param>
        /// <returns>A deep copy of the media items list.</returns>
        /// Used in SaveNoteChanges() to clone the media items before assigning them to the existing note.
        private List<MediaItem>? CloneMediaItems(List<MediaItem>? sourceMedia)
        {
            if (sourceMedia == null || sourceMedia.Count == 0) return null;

            List<MediaItem> clonedMedia = new List<MediaItem>();

            // Create a new MediaItem for each item in the source list to ensure a deep copy.
            foreach (MediaItem mediaItem in sourceMedia)
            {
                // Since MediaItem only contains simple properties (strings), we can create
                // a new instance with the same values.
                clonedMedia.Add(new MediaItem
                {
                    MediaType = mediaItem.MediaType,
                    MediaRef = mediaItem.MediaRef,
                    OriginalFileName = mediaItem.OriginalFileName
                });
            }

            return clonedMedia;
        }

        /// <summary>
        /// Drag and drop support for adding media files to the note. 
        /// Users can drag supported image files onto the note editor, and they will be copied 
        /// to the media storage and associated with the note. The media strip will refresh 
        /// to show the new images, and the note changes will be saved automatically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteTemplateEditor_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = ContainsSupportedImageFile(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// Drag and Drop support for adding media files to the note. This event fires repeatedly as the user drags files over the editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteTemplateEditor_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = ContainsSupportedImageFile(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// PreviewDrop event handler to process files dropped onto the note editor. It checks for supported 
        /// image files, adds them to the selected note, refreshes the media strip, and saves changes. This 
        /// allows users to easily add images to their notes by dragging and dropping them into the editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteTemplateEditor_PreviewDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            bool mediaAdded = false;

            // Process each dropped file and add supported images to the note.
            foreach (string droppedFile in droppedFiles)
            {
                if (!NoteMediaStorage.IsSupportedImageFile(droppedFile))
                    continue;
                // AddMediaFileToSelectedNote handles copying the file to media storage,
                // creating a media item, and associating it with the note. Returns true
                // if the file was successfully added.
                if (AddMediaFileToSelectedNote(droppedFile))
                    mediaAdded = true;
            }

            if (mediaAdded)
            {
                RefreshMediaStrip();
                SaveNoteChanges();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Sees if the IDataObject from a drag event contains at least one supported image file. 
        /// This is used to determine whether to allow copy effects during drag enter/over and 
        /// whether to process the drop. Supported image files are determined by the 
        /// NoteMediaStorage.IsSupportedImageFile method, which checks file extensions against 
        /// a list of supported types. This method ensures that only valid image files can be 
        /// added to notes via drag and drop, providing a better user experience and preventing 
        /// errors from unsupported file types.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        private bool ContainsSupportedImageFile(IDataObject dataObject)
        {
            if (!dataObject.GetDataPresent(DataFormats.FileDrop))
                return false;

            string[] droppedFiles = (string[])dataObject.GetData(DataFormats.FileDrop);

            // Check if any of the dropped files are supported image files.
            foreach (string droppedFile in droppedFiles)
            {
                if (NoteMediaStorage.IsSupportedImageFile(droppedFile))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Add a media file to the selected note by copying it to the media storage directory, 
        /// creating a MediaItem reference, and associating it with the note.
        /// </summary>
        /// <param name="filePath">The path of the media file to add.</param>
        /// <returns>True if the media file was successfully added; otherwise, false.</returns>
        private bool AddMediaFileToSelectedNote(string filePath)
        {
            logger.LogInfo($"[NoteTemplateEditor] Attempting to add media file: {filePath}");

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            string mediaType = NoteMediaStorage.GetMediaTypeFromExtension(Path.GetExtension(filePath));
            if (string.IsNullOrWhiteSpace(mediaType))
                return false;

            string storedExtension = NoteMediaStorage.GetStoredFileExtension(mediaType);
            if (string.IsNullOrWhiteSpace(storedExtension))
                return false;

            try
            {
                logger.LogInfo($"[NoteTemplateEditor] Calling InitMediaDirectory..");
                NoteMediaStorage.InitMediaDirectoryExists();

                SelectedNote.Media ??= new List<MediaItem>();

                string mediaRef = Guid.NewGuid().ToString("N");
                string destinationFilePath = Path.Combine(NoteMediaStorage.GetMediaDirectoryPath(), $"{mediaRef}{storedExtension}");
                File.Copy(filePath, destinationFilePath, true);

                logger.LogInfo($"[NoteTemplateEditor] Storing media file: {destinationFilePath}");

                SelectedNote.Media.Add(new MediaItem
                {
                    MediaType = mediaType,
                    MediaRef = mediaRef,
                    OriginalFileName = Path.GetFileName(filePath)
                });

                logger.LogInfo($"[NoteTemplateEditor] Added media to note: {Path.GetFileName(filePath)}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"[NoteTemplateEditor] Error adding media to note: {ex.Message}");
                MessageBox.Show($"Unable to add the image.\n\r{ex.Message}", "Media Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Refresh the media strip UI to reflect the current media items associated with the selected note. 
        /// This method clears the existing media tiles and rebuilds them based on the SelectedNote's media 
        /// list. If there are no media items, it hides the media strip. This ensures that the media display 
        /// is always up to date with the note's content, especially after adding or removing media items.
        /// </summary>
        private void RefreshMediaStrip()
        {
            logger.LogInfo($"[Doing Action] [RefreshMediaStrip] Refreshing media strip.");
            if (MediaItemsPanel == null || MediaStripBorder == null)
                return;

            MediaItemsPanel.Children.Clear();

            if (SelectedNote?.Media == null || SelectedNote.Media.Count == 0)
            {
                MediaStripBorder.Visibility = Visibility.Collapsed;
                logger.LogInfo($"[Ending Action] [RefreshMediaStrip] ");
                return;
            }

            // Build media tiles for each media item in the selected note's media list and
            // add them to the media strip panel.
            foreach (MediaPreviewItem previewItem in SelectedNote.PreviewMedia)
            {
                MediaItemsPanel.Children.Add(BuildMediaTile(previewItem));
            }
            // media strip -> visible when there are media items to display.
            MediaStripBorder.Visibility = Visibility.Visible;
            logger.LogInfo($"[Ending Action] [RefreshMediaStrip] Refreshed media strip.");
        }

        /// <summary>
        /// This method builds a media tile UI element for a given MediaPreviewItem. It 
        /// creates a bordered container for the media, and if the item is an overflow 
        /// tile, it displays an overlay with the count of additional media items. If 
        /// it's a regular media item, it loads the image from the file path and displays 
        /// it, along with a remove button. The media tile is designed to be interactive, 
        /// allowing users to click on it to open the media gallery or remove the media 
        /// item from the note. This method encapsulates all the UI logic for representing 
        /// media items in the media strip of the note editor.
        /// </summary>
        /// <param name="previewItem"></param>
        /// <returns></returns>
        private FrameworkElement BuildMediaTile(MediaPreviewItem previewItem)
        {
            // The media tile is a bordered container that holds the media content.
            // It has a fixed size, margin, rounded corners, and a background color.
            // The border color indicates whether it's an overflow tile or a regular media item.
            Border mediaTileBorder = new Border
            {
                Width = 136,
                Height = 104,
                Margin = new Thickness(0, 0, 8, 0),
                CornerRadius = new CornerRadius(6),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = Brushes.Black
            };

            // The media tile grid is the inner container that holds the media content and any overlays or buttons.
            Grid mediaTileGrid = new Grid();
            mediaTileBorder.Child = mediaTileGrid;

            // If the preview item is an overflow tile, we display a dark overlay with the count of additional media items.
            if (previewItem.IsOverflowTile)
            {
                // The overflow tile indicates that there are more media items than can be displayed in the strip. It
                // shows a count of how many additional items there are.
                Border overlayBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0))
                };
                // The overlay contains a text block that displays the overflow count with a "+" prefix, indicating
                // how many more media items are available beyond the ones currently shown in the media strip.
                // The text is styled to be large, bold, and white for visibility against the dark overlay.
                mediaTileGrid.Children.Add(overlayBorder);

                // The overflow button is placed on top of the overlay and displays the count of additional media
                // items. It is styled to be large and bold, with a transparent background and no border, making
                // it look like part of the overlay. The button has a click event handler that opens the media
                // gallery when clicked, allowing users to see all their media items.
                Button overflowButton = new Button
                {
                    Content = previewItem.DisplayText,
                    FontSize = 28,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    ToolTip = previewItem.ToolTipText
                };
                // Event handler; When the overflow button is clicked, it triggers the ShowMediaGallery method with
                // an index of 0, which opens the media gallery starting from the first media item. This allows
                // users to access all their media items even when some are hidden in the overflow.
                overflowButton.Click += (sender, e) => ShowMediaGallery(0);
                // Finally. Add the overflow button to the media tile grid, which will display it on top of the
                // dark overlay, showing the count of additional media items and allowing users to click through
                // to the gallery.
                mediaTileGrid.Children.Add(overflowButton);
            }
            else
            {
                string mediaFilePath = previewItem.PreviewFilePath ?? string.Empty;

                // For regular media items, we attempt to load the image from the provided file path.
                // If the file exists, we create an Image control to display it. The image is set to stretch
                // uniformly to fill the tile, and it has a hand cursor to indicate interactivity. If the image
                // loads successfully, it is added to the media tile grid. If the file does not exist, we
                // display a "Missing image" text block instead, indicating that the media file could not be found.
                if (!string.IsNullOrWhiteSpace(mediaFilePath) && File.Exists(mediaFilePath))
                {
                    Image mediaImage = new Image
                    {
                        Stretch = Stretch.UniformToFill,
                        Cursor = Cursors.Hand,
                        ToolTip = previewItem.ToolTipText
                    };

                    // Load from an in-memory stream so WPF does not keep the media file locked.
                    BitmapImage? bitmapImage = CreateBitmapImageFromFile(mediaFilePath, 240);
                    if (bitmapImage != null)
                        mediaImage.Source = bitmapImage;

                    // When the media image is clicked, we want to open the media gallery starting at the index of
                    // this media item. We calculate the media index by finding the position of the media item
                    // in the SelectedNote's media list. This allows users to click on any media tile and jump
                    // directly to that item in the gallery view.
                    int mediaIndex = SelectedNote.Media?.IndexOf(previewItem.MediaItem) ?? 0;
                    // Event handler; When the media image is clicked, it checks if the click count is 2 or
                    // more (indicating a double-click), and if so, it calls the ShowMediaGallery method with the
                    // index of the media item. This allows users to open the gallery view for that specific media
                    // item by double-clicking on its tile in the media strip.
                    mediaImage.MouseLeftButtonDown += (sender, e) =>
                    {
                        if (e.ClickCount >= 2)
                        {
                            ShowMediaGallery(mediaIndex);
                            e.Handled = true;
                        }
                    };
                    // Add the media image to the media tile grid, which will display the image in the tile. If the image
                    mediaTileGrid.Children.Add(mediaImage);
                }
                else
                {
                    // ELSE; If the media file cannot be found at the specified path, we display a text block with the
                    // message "Missing image". This provides feedback to the user that the media item is supposed to
                    // be there but cannot be loaded, likely due to a missing or deleted file. The text block is
                    // styled with white foreground color and centered alignment to make it clear and noticeable
                    // within the media tile.
                    mediaTileGrid.Children.Add(new TextBlock
                    {
                        Text = "Missing image",
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }

                // Add a button to remove the media item from the note. The button is styled to be small and positioned
                Button removeButton = new Button
                {
                    Content = "X",
                    Width = 24,
                    Height = 24,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 4, 4, 0),
                    Tag = previewItem.MediaItem,
                    ToolTip = "Remove image"
                };
                removeButton.Click += RemoveMediaButton_Click;
                mediaTileGrid.Children.Add(removeButton);
            }
            
            return mediaTileBorder;
        }

        /// <summary>
        /// Remove a media item from the selected note when the remove button on a media tile is clicked. 
        /// This method first checks if the sender is a Button and retrieves the associated MediaItem 
        /// from the button's Tag property. It then releases any resources related to the media strip to 
        /// ensure that there are no file locks on the media files. The media item is removed from the 
        /// SelectedNote's media list, and if the list becomes empty, it is set to null. The media strip 
        /// is refreshed to reflect the changes, and the note changes are saved. Finally, it attempts to 
        /// delete the media file from storage, and if it fails (e.g., due to a file lock), it schedules 
        /// retries to delete the file after short intervals. This ensures that users can remove media 
        /// items from their notes and that the associated files are cleaned up properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Note: The difficulty level was pretty high on this one, because of the need to handle file 
        /// locks and ensure that the UI updates correctly without leaving orphaned media files. The 
        /// use of Dispatcher.InvokeAsync with Background priority is a key technique to allow WPF to 
        /// finish its UI updates and release file locks before we attempt to delete the media file. 
        /// The retry mechanism for deleting media files is also important to handle cases where the 
        /// file might still be locked by the system, providing a better user experience by eventually 
        /// cleaning up the files without requiring manual intervention.
        /// </remarks>
        private async void RemoveMediaButton_Click(object sender, RoutedEventArgs e)
        {
            logger.LogInfo($"[RemoveMediaButton_Click] Attempting to remove media item from note.");
            if (sender is not Button removeButton || removeButton.Tag is not MediaItem mediaItem)
                return;

            ReleaseMediaStripResources();

            SelectedNote.Media?.Remove(mediaItem);

            if (SelectedNote.Media != null && SelectedNote.Media.Count == 0)
                SelectedNote.Media = null;
            //Refresh media strip and save changes before attempting to delete the file to ensure
            //UI is updated and file locks are released.
            RefreshMediaStrip();
            SaveNoteChanges();

            // Let WPF finish updating and releasing the previous thumbnail visuals before deleting the file.
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            NoteMediaStorage.TryDeleteMediaFile(mediaItem);
            // If the file is still locked and cannot be deleted, we schedule retries to attempt deletion after short intervals.
            // This is DISABLED for now but it can be re-enabled if we find that media files are often left undeleted due to
            // locks. The retry mechanism uses a DispatcherTimer to attempt deletion every 300 milliseconds, up to a maximum
            // number of attempts. This allows the system time to release any locks on the file, ensuring that it eventually
            // gets cleaned up without requiring user intervention.
            // The TRADEOFF is that media files might remain in storage for a short time after removal if they are locked,
            // but it prevents the complexity and potential issues of trying to delete files that are still in use. Also, there's
            // quite a delay which seems like a blocked thread in the UI -lasting about 5 seconds after deleting.
            //if (!NoteMediaStorage.TryDeleteMediaFile(mediaItem))
            //    ScheduleMediaDeleteRetry(mediaItem);
            logger.LogInfo($"[RemoveMediaButton_Click] Finished processing media removal for: {mediaItem.OriginalFileName}");
        }

        /// <summary>
        /// Timer method to schedule retries for deleting a media file that is currently locked by the system. It attempts 
        /// to delete the file every 300 milliseconds, up to a maximum number of attempts. If the file is successfully 
        /// deleted or the maximum attempts are reached, the timer stops. This method is a safety net to ensure that 
        /// media files eventually get cleaned up even if they are temporarily locked, without causing issues in the 
        /// UI or requiring manual cleanup by the user.
        /// </summary>
        /// <param name="mediaItem">The media item that is being scheduled for deletion retry.</param>
        /// <remarks>There are tradeoffs to using this approach. While it ensures that media files are eventually deleted 
        /// even if they are temporarily locked, it introduces a delay and potential complexity in the UI. The retry 
        /// mechanism uses a DispatcherTimer to attempt deletion every 300 milliseconds, up to a maximum number of attempts. 
        /// This allows the system time to release any locks on the file, ensuring that it eventually gets cleaned up 
        /// without requiring user intervention.
        /// THS IS USED IN FUNC RemoveMediaButton_Click() when a media file fails to delete due to a lock. REMmed out for now,
        /// but can be re-enabled later.</remarks>
        //private void ScheduleMediaDeleteRetry(MediaItem mediaItem)
        //{
        //    // Safety net only. If you need to isolate delete behavior without delayed retries,
        //    // comment out the ScheduleMediaDeleteRetry(...) calls at the inline remove path
        //    // and the gallery delete path first, then stop here before starting the timer.
        //    DispatcherTimer deleteRetryTimer = new DispatcherTimer();
        //    int attemptsRemaining = 8;
        //    deleteRetryTimer.Interval = TimeSpan.FromMilliseconds(300);
        //    deleteRetryTimer.Tick += (sender, e) =>
        //    {
        //        attemptsRemaining--;

        //        if (NoteMediaStorage.TryDeleteMediaFile(mediaItem) || attemptsRemaining <= 0)
        //            deleteRetryTimer.Stop();
        //    };
        //    deleteRetryTimer.Start();
        //}


        /// <summary>
        /// This method recursively traverses the visual tree starting from the given parent object 
        /// and releases any image sources found in Image controls.
        /// </summary>
        private void ReleaseMediaStripResources()
        {
            if (MediaItemsPanel == null)
                return;

            ReleaseImageSources(MediaItemsPanel);
        }

        /// <summary>
        /// Create a frozen BitmapImage from file bytes so WPF does not keep a lock on the source file.
        /// This is the root-cause fix for image delete "file is still in use" issues.
        /// </summary>
        /// <param name="filePath">The path to the image file.</param>
        /// <param name="decodePixelWidth">Optional width to decode the image to.</param>
        /// <returns>A frozen BitmapImage or null if the file does not exist or an error occurs.</returns>
        private BitmapImage? CreateBitmapImageFromFile(string filePath, int? decodePixelWidth = null)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                byte[] imageData = File.ReadAllBytes(filePath);

                using (MemoryStream stream = new MemoryStream(imageData))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    if (decodePixelWidth.HasValue)
                        bitmap.DecodePixelWidth = decodePixelWidth.Value;

                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[CreateBitmapImageFromFile] Failed for {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Recursively traverses the visual tree starting from the given parent object and releases any image sources found in Image controls.
        /// </summary>
        /// <param name="parentObject">The parent object from which to start the traversal.</param>
        private void ReleaseImageSources(DependencyObject parentObject)
        {
            if (parentObject is Image imageControl)
                imageControl.Source = null;

            int childCount = VisualTreeHelper.GetChildrenCount(parentObject);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(parentObject, i);
                ReleaseImageSources(childObject);
            }
        }

        /// <summary>
        /// Build a media gallery window to display all media items associated with the selected note. 
        /// The gallery includes a toolbar with the note title and a menu button, a main area for 
        /// previewing the selected media item, and a thumbnail strip for navigating between media items. 
        /// Users can click on thumbnails to change the previewed item, and the gallery updates 
        /// dynamically based on the media items in the note. This provides an enhanced experience 
        /// for viewing and managing media within notes, allowing users to easily access and interact 
        /// with their images.
        /// </summary>
        /// <param name="startingIndex">The index of the media item to display first.</param>
        /// Note:There's NO WPF gallery window XAML created and this is all done programmatically here. 
        /// This is because the gallery is a dynamic component that relies heavily on the current 
        /// state of the SelectedNote's media items, and building it in XAML would be less efficient 
        /// and more complex to manage. By constructing the gallery entirely in code, we can easily 
        /// create and update the UI elements based on the media items available, and we can ensure 
        /// that it reflects the current state of the note without needing to bind to a static XAML 
        /// structure. This approach allows for greater flexibility and responsiveness in how the 
        /// gallery displays media content.
        /// A combination of CoPilot, Codex, and Grox helped with this. The layout and structure were 
        /// primarily generated by CoPilot, with some adjustments and refinements made manually. 
        /// The styling and specific UI element configurations were informed by examples and 
        /// documentation, with some assistance from Codex for best practices in WPF UI design. 
        /// Grox was used to help optimize the code structure and ensure that the gallery functionality
        /// was implemented efficiently and effectively --ESPECIALLY with the file lock issue.
        /// </summary>
        /// <param name="startingIndex">The index of the media item to display first.</param>   
        private void ShowMediaGallery(int startingIndex)
        {
            if (SelectedNote?.Media == null || SelectedNote.Media.Count == 0)
                return;

            if (startingIndex < 0 || startingIndex >= SelectedNote.Media.Count)
                startingIndex = 0;

            Window previewWindow = new Window
            {
                Title = "Image gallery",
                Width = 900,
                Height = 760,
                Owner = this,
                Background = (Brush)FindResource("WindowBackgroundBrush")
            };

            Brush windowBackgroundBrush = (Brush)FindResource("WindowBackgroundBrush");
            Brush headerBackgroundBrush = (Brush)FindResource("HeaderBackgroundBrush");
            Brush headerForegroundBrush = (Brush)FindResource("HeaderForegroundBrush");
            Brush noteEditBackgroundBrush = (Brush)FindResource("NoteEditBackgroundBrush");
            Brush noteEditForegroundBrush = (Brush)FindResource("NoteEditForegroundBrush");

            Grid galleryGrid = new Grid
            {
                Background = noteEditBackgroundBrush
            };
            galleryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            galleryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            galleryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            DockPanel toolbarPanel = new DockPanel
            {
                Margin = new Thickness(0),
                Background = headerBackgroundBrush
            };

            TextBlock galleryTitle = new TextBlock
            {
                Foreground = headerForegroundBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 8, 12, 8)
            };

            Button galleryMenuButton = new Button
            {
                Margin = new Thickness(0, 0, 8, 0),
                Width = 40,
                Height = 40,
                Background = (Brush)FindResource("rscHambugerMenu"),
                BorderBrush = Brushes.Gray
            };
            galleryMenuButton.Content = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/build/Icos/hamburger-menu-icon-png-white-11.jpg")),
                Stretch = Stretch.Uniform
            };

            Image previewImage = new Image
            {
                Stretch = Stretch.Uniform
            };

            ScrollViewer previewScroller = new ScrollViewer
            {
                Content = previewImage,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = noteEditBackgroundBrush
            };

            StackPanel thumbnailPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(8)
            };

            ScrollViewer thumbnailScroller = new ScrollViewer
            {
                Content = thumbnailPanel,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = windowBackgroundBrush
            };

            DockPanel.SetDock(galleryMenuButton, Dock.Right);
            toolbarPanel.Children.Add(galleryMenuButton);
            toolbarPanel.Children.Add(galleryTitle);
            Grid.SetRow(toolbarPanel, 0);
            Grid.SetRow(previewScroller, 1);
            Grid.SetRow(thumbnailScroller, 2);

            galleryGrid.Children.Add(toolbarPanel);
            galleryGrid.Children.Add(previewScroller);
            galleryGrid.Children.Add(thumbnailScroller);
            previewWindow.Content = galleryGrid;

            int currentIndex = startingIndex;

            void RefreshGallerySelection()
            {
                if (SelectedNote.Media == null || SelectedNote.Media.Count == 0)
                {
                    previewWindow.Close();
                    return;
                }

                if (currentIndex < 0)
                    currentIndex = 0;

                if (currentIndex >= SelectedNote.Media.Count)
                    currentIndex = SelectedNote.Media.Count - 1;

                MediaItem selectedMediaItem = SelectedNote.Media[currentIndex];
                string selectedMediaPath = NoteMediaStorage.GetMediaFilePath(selectedMediaItem);
                if (string.IsNullOrWhiteSpace(selectedMediaPath) || !File.Exists(selectedMediaPath))
                    return;

                BitmapImage? selectedBitmap = CreateBitmapImageFromFile(selectedMediaPath);
                if (selectedBitmap != null)
                    previewImage.Source = selectedBitmap;

                galleryTitle.Text = selectedMediaItem.OriginalFileName ?? "Image preview";
                thumbnailPanel.Children.Clear();

                for (int i = 0; i < SelectedNote.Media.Count; i++)
                {
                    MediaItem thumbnailMediaItem = SelectedNote.Media[i];
                    string thumbnailPath = NoteMediaStorage.GetMediaFilePath(thumbnailMediaItem);

                    Border thumbnailBorder = new Border
                    {
                        Width = 96,
                        Height = 72,
                        Margin = new Thickness(0, 0, 8, 0),
                        BorderThickness = new Thickness(i == currentIndex ? 2 : 1),
                        BorderBrush = i == currentIndex ? Brushes.Gold : Brushes.Gray,
                        Background = noteEditBackgroundBrush
                    };

                    if (!string.IsNullOrWhiteSpace(thumbnailPath) && File.Exists(thumbnailPath))
                    {
                        Image thumbnailImage = new Image
                        {
                            Stretch = Stretch.UniformToFill,
                            Cursor = Cursors.Hand,
                            ToolTip = thumbnailMediaItem.OriginalFileName
                        };

                        BitmapImage? thumbnailBitmap = CreateBitmapImageFromFile(thumbnailPath, 180);
                        if (thumbnailBitmap != null)
                            thumbnailImage.Source = thumbnailBitmap;

                        int localIndex = i;
                        thumbnailImage.MouseLeftButtonUp += (sender, e) =>
                        {
                            currentIndex = localIndex;
                            RefreshGallerySelection();
                        };

                        thumbnailBorder.Child = thumbnailImage;
                    }
                    else
                    {
                        thumbnailBorder.Child = new TextBlock
                        {
                            Text = "Missing image",
                            Foreground = noteEditForegroundBrush,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }

                    thumbnailPanel.Children.Add(thumbnailBorder);
                }
            }

            ContextMenu galleryMenu = new ContextMenu();
            MenuItem saveToDownloadsMenuItem = new MenuItem { Header = "Save to Downloads" };
            MenuItem openInPhotosMenuItem = new MenuItem { Header = "Open in Photos" };
            MenuItem removeImageMenuItem = new MenuItem { Header = "Delete image" };
            galleryMenu.Items.Add(saveToDownloadsMenuItem);
            galleryMenu.Items.Add(openInPhotosMenuItem);
            galleryMenu.Items.Add(removeImageMenuItem);
            galleryMenuButton.ContextMenu = galleryMenu;
            galleryMenuButton.Click += (sender, e) =>
            {
                galleryMenu.PlacementTarget = galleryMenuButton;
                galleryMenu.IsOpen = true;
            };

            saveToDownloadsMenuItem.Click += (sender, e) =>
            {
                if (SelectedNote.Media == null || SelectedNote.Media.Count == 0)
                    return;

                MediaItem currentMediaItem = SelectedNote.Media[currentIndex];
                string sourceMediaPath = NoteMediaStorage.GetMediaFilePath(currentMediaItem);
                if (string.IsNullOrWhiteSpace(sourceMediaPath) || !File.Exists(sourceMediaPath))
                    return;

                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);

                string originalFileName = currentMediaItem.OriginalFileName;
                if (string.IsNullOrWhiteSpace(originalFileName))
                    originalFileName = Path.GetFileName(sourceMediaPath);

                string destinationFilePath = Path.Combine(downloadsPath, originalFileName);
                string destinationFileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationFilePath);
                string destinationExtension = Path.GetExtension(destinationFilePath);
                int duplicateIndex = 1;

                while (File.Exists(destinationFilePath))
                {
                    destinationFilePath = Path.Combine(downloadsPath, $"{destinationFileNameWithoutExtension} ({duplicateIndex}){destinationExtension}");
                    duplicateIndex++;
                }

                File.Copy(sourceMediaPath, destinationFilePath, false);
            };

            openInPhotosMenuItem.Click += (sender, e) =>
            {
                if (SelectedNote.Media == null || SelectedNote.Media.Count == 0)
                    return;

                MediaItem currentMediaItem = SelectedNote.Media[currentIndex];
                string currentMediaPath = NoteMediaStorage.GetMediaFilePath(currentMediaItem);
                if (string.IsNullOrWhiteSpace(currentMediaPath) || !File.Exists(currentMediaPath))
                    return;

                Process.Start(new ProcessStartInfo(currentMediaPath)
                {
                    UseShellExecute = true
                });
            };

            removeImageMenuItem.Click += async (sender, e) =>
            {
                if (SelectedNote.Media == null || SelectedNote.Media.Count == 0)
                    return;

                MediaItem mediaItemToRemove = SelectedNote.Media[currentIndex];
                previewImage.Source = null;
                ReleaseImageSources(thumbnailPanel);
                thumbnailPanel.Children.Clear();
                SelectedNote.Media.Remove(mediaItemToRemove);

                if (SelectedNote.Media.Count == 0)
                    SelectedNote.Media = null;

                RefreshMediaStrip();
                SaveNoteChanges();
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

                NoteMediaStorage.TryDeleteMediaFile(mediaItemToRemove);
                // if (!NoteMediaStorage.TryDeleteMediaFile(mediaItemToRemove))
                //     ScheduleMediaDeleteRetry(mediaItemToRemove);

                RefreshGallerySelection();
            };

            previewWindow.PreviewKeyDown += (sender, e) =>
            {
                if (SelectedNote.Media == null || SelectedNote.Media.Count == 0)
                    return;

                if (e.Key == Key.Right)
                {
                    currentIndex = (currentIndex + 1) % SelectedNote.Media.Count;
                    RefreshGallerySelection();
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    currentIndex = (currentIndex - 1 + SelectedNote.Media.Count) % SelectedNote.Media.Count;
                    RefreshGallerySelection();
                    e.Handled = true;
                }
            };

            RefreshGallerySelection();
            previewWindow.ShowDialog();
        }


        /// <summary>
        /// RichEdit event handler to handle text change and start auto save timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RchEditNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!HasActiveNoteSearch())
                ClearSelectionSpotlight();

            // Update the last activity time whenever text is changed
            lastActivityTime = DateTime.Now;

            // Start or reset the timer when text is changed
            saveTimer.Stop();
            saveTimer.Start();
        }

        /// <summary>
        /// Highlight all matches of the current text selection in the note editor when no explicit search is active.
        /// </summary>
        private void RchEditNote_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (isHandlingEditorSelectionChange)
                return;

            selectionSpotlightTimer.Stop();

            // Explicit Ctrl+F search owns the editor highlighting state. While search is active,
            // selection-based highlighting stays out of the way so the two features do not fight.
            if (HasActiveNoteSearch())
                return;

            // When the selection changes, we want to eventually update the spotlight to match the new selection.
            string selectedText = NormalizeSearchText(rchEditNote.Selection?.Text);
            if (selectedText.Length < MINIMUM_SPOTLIGHT_SELECTION_LENGTH)
            {
                ClearSelectionSpotlight();
                return;
            }

            selectionSpotlightTimer.Start();
        }

        /// <summary>
        /// Mouse selection changes are finalized on MouseLeftButtonUp, so we trigger the spotlight 
        /// update at that point to ensure we are working with the final selection after mouse interactions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RchEditNote_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // When the mouse button is released, the user is usually done selecting text.
            // Queue the spotlight update here so the highlight pass runs after the final selection settles.
            QueueSelectionSpotlightUpdate();
        }

        /// <summary>
        /// Keyboard selection changes (Shift+Arrow, Ctrl+Shift+Arrow, etc.) will trigger 
        /// SelectionChanged event, but the final selection state is not stable until the key 
        /// is released and the KeyUp event is fired. Queue the spotlight update on KeyUp to 
        /// ensure we are working with the final selection state after keyboard interactions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RchEditNote_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard selection changes (Shift+Arrow, Ctrl+Shift+Arrow, etc.) land here.
            // Reuse the same deferred update path as mouse selection completion.
            QueueSelectionSpotlightUpdate();
        }

        /// <summary>
        /// Spotlight update is queued on selection changes, but we wait until a short period
        /// of inactivity after the change before we run the update.
        /// </summary>
        private void QueueSelectionSpotlightUpdate()
        {
            // This method can be triggered by both SelectionChanged and KeyUp events,
            // but the logic is the same: we want to wait until the user has paused
            // selection changes before we run the spotlight update.
            if (isHandlingEditorSelectionChange)
                return;

            selectionSpotlightTimer.Stop();

            // If the user is in explicit search mode, leave the current search highlights alone.
            if (HasActiveNoteSearch())
                return;

            string selectedText = NormalizeSearchText(rchEditNote.Selection?.Text);

            // If the selection is very small (less than 2 characters), we do not trigger the spotlight.
            if (selectedText.Length < 2)
            {
                // Small or empty selections are ignored on purpose. This keeps one-letter selections
                // from lighting up the whole note and also clears any previous selection-based matches.
                ClearSelectionSpotlight();
                return;
            }

            // The timer gives the RichTextBox a short quiet period before we scan the document.
            selectionSpotlightTimer.Start();
        }

        private void SelectionSpotlightTimer_Tick(object sender, EventArgs e)
        {
            selectionSpotlightTimer.Stop();

            if (isHandlingEditorSelectionChange || HasActiveNoteSearch())
                return;

            string selectedText = NormalizeSearchText(rchEditNote.Selection?.Text);
            if (selectedText.Length < 2)
            {
                ClearSelectionSpotlight();
                return;
            }

            // If we already highlighted this exact selected text and those highlights are still active,
            // do nothing. This avoids rebuilding the same search result set over and over.
            if (selectedText == lastSelectionSpotlightText && isSelectionSpotlightActive)
                return;

            isHandlingEditorSelectionChange = true;

            try
            {
                // Reuse the existing in-note spotlight engine, but do not move the caret/selection.
                // The goal here is "show the other matches" while preserving the user's current selection.
                SpotlightSearch(rchEditNote, selectedText, false);
                isSelectionSpotlightActive = searchMatches.Count > 0;
                lastSelectionSpotlightText = isSelectionSpotlightActive ? selectedText : string.Empty;
            }
            finally
            {
                isHandlingEditorSelectionChange = false;
            }
        }

        private void ClearSelectionSpotlight()
        {
            selectionSpotlightTimer.Stop();
            lastSelectionSpotlightText = string.Empty;

            if (!isSelectionSpotlightActive)
                return;

            isHandlingEditorSelectionChange = true;

            try
            {
                // Only clear highlights that came from the selection spotlight flow.
                // Explicit search will rebuild its own results whenever it is active again.
                ResetSpotlights(rchEditNote);
                isSelectionSpotlightActive = false;
            }
            finally
            {
                isHandlingEditorSelectionChange = false;
            }
        }

        // This will be called after the specified delay of inactivity saving note data.
        // The timer will reset whenever there's user activity, ensuring the note is
        // saved when there's a period of inactivity.
        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            // Get time elapsed since the last activity
            TimeSpan elapsed = DateTime.Now - lastActivityTime;

            // Check if enough time has passed without activity to trigger auto-save
            if (elapsed.TotalMilliseconds >= SaveDelayMilliseconds)
            {
                saveTimer.Stop();
                SaveNoteChanges();
            }
        }

        
        /// <summary>
        /// Event when user presses ENTER to update the title
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotesTitleTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return) UpdateSelectedNoteTitle();
        }

        /// <summary>
        /// With the current note, change the title, then clear focus.
        /// </summary>
        private void UpdateSelectedNoteTitle()
        {
            if (SelectedNote != null && SelectedNote.Title != NotesTitle.Text)
                SelectedNote.Title = NotesTitle.Text;

            this.Title = NotesTitle.Text;

            Keyboard.ClearFocus();
        }

        //Once we click elsewhere, reset the search box watermark, or html "placeholder"
        private void NoteSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoteSearch.Text)
                || NoteSearch.Text == UI_TEXTBOX_STR_SEARCH)
                NoteSearch.Text = UI_TEXTBOX_STR_SEARCH;
        }

        //clear the tesxtbox watermark so we can search.
        private void NoteSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == UI_TEXTBOX_STR_SEARCH)
                textBox.Text = string.Empty;
        }

        private void NoteSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == UI_TEXTBOX_STR_SEARCH)
                textBox.Text = string.Empty;
        }

        private void NoteSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    string searchText = textBox.Text.Trim();

                    if (!string.IsNullOrEmpty(searchText))
                        // Do search and highlight matches
                        SpotlightSearch(rchEditNote, searchText);
                    else
                        ClearNoteSearch();
                }
            }
            else if (string.IsNullOrEmpty(NoteSearch.Text) && (e.Key == Key.Back || e.Key == Key.Delete))
            {
                ClearNoteSearch();
            }
        }

        /// <summary>
        /// Keyboard shortcuts for note search behavior:
        /// Ctrl+F focuses search, F3 moves next, Shift+F3 moves previous, Escape clears.
        /// </summary>
        private void NoteTemplateEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                NoteSearch.Focus();

                if (NoteSearch.Text != UI_TEXTBOX_STR_SEARCH)
                    NoteSearch.SelectAll();

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ClearNoteSearch();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    GoToPreviousSearchMatch();
                else
                    GoToNextSearchMatch();

                e.Handled = true;
            }
        }


        /// <summary>
        /// During a search within a RichEdit, reset the formatting and highlight text we're searching for.
        /// </summary>
        /// <param name="richTextBox">Richedit FlowDocument to search into (</param>
        /// <param name="searchText">string input of text to find</param>
        private void SpotlightSearch(RichTextBox richTextBox, string searchText, bool selectFirstMatch = true)
        {
            // Reset all previous spotlights
            ResetSpotlights(richTextBox);

            // Normalize the search text to enable more flexible matching that ignores extra whitespace.
            // If the normalized search text is empty, do not proceed with searching.
            string normalizedSearchText = NormalizeSearchText(searchText);
            if (string.IsNullOrWhiteSpace(normalizedSearchText))
                return;

            // Build a normalized search view of the RichTextBox document so phrase search can
            RichTextSearchData searchData = BuildRichTextSearchData(richTextBox);
            if (string.IsNullOrWhiteSpace(searchData.NormalizedText))
                return;

            // Search for the normalized search text within the normalized document text, and map those
            // matches back to TextRanges in the RichTextBox for highlighting.
            int searchStartIndex = 0;
            while (searchStartIndex < searchData.NormalizedText.Length)
            {
                // Use ordinal ignore case to find the next match of the normalized search text within the normalized document text.
                int matchIndex = searchData.NormalizedText.IndexOf(normalizedSearchText, searchStartIndex, StringComparison.OrdinalIgnoreCase);
                if (matchIndex < 0)
                    break;

                // Map the normalized match index back to a TextRange in the RichTextBox document. If mapping fails, skip this match and continue searching.
                TextRange foundRange = CreateTextRangeFromNormalizedMatch(richTextBox, searchData, matchIndex, normalizedSearchText.Length);
                if (foundRange == null)
                {
                    searchStartIndex = matchIndex + normalizedSearchText.Length;
                    continue;
                }

                // Paint every matched range with the same background brush.
                // The resulting TextRanges are also cached so F3 / Shift+F3 can move between them.
                // For theme  skinning, use the DynamicResource reference to a highlight brush defined in the app resources 
                // TODO: Finish adding NoteSearchHighlightBrush to the theme skinning set for all themes, then remove the Brushes.Yellow fallback.
                foundRange.ApplyPropertyValue(TextElement.BackgroundProperty, Application.Current.Resources["NoteSearchHighlightBrush"]  ?? Brushes.Yellow);
                searchMatches.Add(foundRange);

                searchStartIndex = matchIndex + normalizedSearchText.Length;
            }

            if (searchMatches.Count > 0)
            {
                currentSearchMatchIndex = 0;

                // Explicit search wants to jump to the first result, but selection-based spotlighting
                // does not. That behavior difference is controlled by the selectFirstMatch flag.
                if (selectFirstMatch)
                    SelectSearchMatch(currentSearchMatchIndex);
            }
        }

        /// <summary>
        /// This helper checks if the search box has active user input that should be considered "owning" the current 
        /// highlights in the editor.
        /// </summary>
        /// <returns>True if the search box has active user input; otherwise, false.</returns>
        private bool HasActiveNoteSearch()
        {
            string searchText = NoteSearch.Text?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(searchText) && searchText != UI_TEXTBOX_STR_SEARCH;
        }

        /// <summary>
        /// Text normalization for search input and RichTextBox content to enable more flexible matching that ignores extra whitespace.
        /// </summary>
        /// <param name="text">The text to normalize.</param>
        /// <returns>The normalized text.</returns>
        private string NormalizeSearchText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return Regex.Replace(text.Trim(), @"\s+", " ");
        }

        /// <summary>
        /// Search for text in a FlowDocument/TextBlock and return the TextRange of the found text.
        /// Build a normalized search view of the RichTextBox document so phrase search can
        /// tolerate whitespace differences while still mapping matches back to TextPointer positions.
        /// </summary>
        /// <param name="richTextBox">RichEdit FlowDocument to search into</param>
        private RichTextSearchData BuildRichTextSearchData(RichTextBox richTextBox)
        {
            // Walk through the RichTextBox document and build a list of TextPointers for each
            // character, along with a normalized text string that collapses consecutive
            // whitespace. This allows us to do flexible phrase searching while still being
            // able to map back to the original TextPointers for highlighting.
            List<TextPointer> rawTextPointers = new List<TextPointer>();
            System.Text.StringBuilder rawTextBuilder = new System.Text.StringBuilder();

            // The TextPointer navigation is a bit complex, but essentially we are iterating
            // through the document character by character.
            // Richedit "documents" are made up of a tree of elements, and TextPointers
            // allow us to navigate through the text content of those elements.
            // Richedit documents: https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/flow-document-overview?view=netdesktop-7.0
            // For the purposes of flow content, there are two important categories:
            //
            // Block - derived classes: Also called "Block content elements" or just "Block Elements".Elements that
            // inherit from Block can be used to group elements under a common parent or to apply common attributes to a group.
            //
            // Inline - derived classes: Also called "Inline content elements" or just "Inline Elements".Elements
            // that inherit from Inline are either contained within a Block Element or another Inline Element.
            // Inline Elements are often used as the direct container of content that is rendered to the screen.
            // For example, a Paragraph(Block Element) can contain a Run(Inline Element) but the Run actually contains
            // the text that is rendered on the screen.
            TextPointer pointer = richTextBox.Document.ContentStart;
            while (pointer != null && pointer.CompareTo(richTextBox.Document.ContentEnd) < 0)
            {
                // When the pointer is at text content, we want to extract that text and keep track
                // of the TextPointers for each character.
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    // Get the run of text in this element and iterate through each character to build our raw text and pointer list.
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    for (int i = 0; i < textRun.Length; i++)
                    {
                        // Get a TextPointer for the current character by offsetting from the start of the run.
                        TextPointer charPointer = pointer.GetPositionAtOffset(i, LogicalDirection.Forward);
                        if (charPointer != null)
                        {
                            rawTextBuilder.Append(textRun[i]);
                            rawTextPointers.Add(charPointer);
                        }
                    }
                }
                // Move the pointer to the next context position to continue iterating through the document.
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }

            // Now we have a raw text string and a parallel list of TextPointers for each character in the RichTextBox document.
            List<int> normalizedToRawIndexMap = new List<int>();
            System.Text.StringBuilder normalizedTextBuilder = new System.Text.StringBuilder();

            // We iterate through the raw text and build a normalized version of it, while also keeping track of the
            // mapping from normalized indices back to raw indices.
            for (int i = 0; i < rawTextBuilder.Length; i++)
            {
                // For normalization, we want to collapse consecutive whitespace characters into a single
                // space, and also trim leading/trailing whitespace.
                char currentChar = rawTextBuilder[i];
                if (char.IsWhiteSpace(currentChar))
                {
                    // If the current character is whitespace, we only want to add a single space to the normalized text,
                    if (normalizedTextBuilder.Length == 0 || normalizedTextBuilder[normalizedTextBuilder.Length - 1] == ' ')
                        continue;

                    normalizedTextBuilder.Append(' ');
                    normalizedToRawIndexMap.Add(i);
                }
                else
                {
                    normalizedTextBuilder.Append(currentChar);
                    normalizedToRawIndexMap.Add(i);
                }
            }

            return new RichTextSearchData
            {
                RawTextPointers = rawTextPointers,
                NormalizedText = normalizedTextBuilder.ToString(),
                NormalizedToRawIndexMap = normalizedToRawIndexMap
            };
        }

        /// <summary>
        /// This helper takes a match index and length from the normalized search text and maps it back to a 
        /// TextRange in the RichTextBox document for highlighting.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox containing the text.</param>
        /// <param name="searchData">The search data containing raw and normalized text information.</param>
        /// <param name="normalizedMatchIndex">The starting index of the match in the normalized text.</param>
        /// <param name="normalizedMatchLength">The length of the match in the normalized text.</param>
        /// <returns>A TextRange representing the matched text in the RichTextBox, or null if the match is invalid.</returns>
        private TextRange? CreateTextRangeFromNormalizedMatch(RichTextBox richTextBox, RichTextSearchData searchData, int normalizedMatchIndex, int normalizedMatchLength)
        {
            // Validate the normalized match indices before attempting to map back to raw indices.
            if (normalizedMatchIndex < 0 ||
                normalizedMatchLength <= 0 ||
                normalizedMatchIndex >= searchData.NormalizedToRawIndexMap.Count)
            {
                return null;
            }

            // Calculate the end index of the match in the normalized text and validate it as well.
            int normalizedEndIndex = normalizedMatchIndex + normalizedMatchLength - 1;
            // The end index is inclusive, so it should be less than the count of the normalized to raw index map.
            if (normalizedEndIndex >= searchData.NormalizedToRawIndexMap.Count)
                return null;

            // Map the normalized match indices back to raw indices using the NormalizedToRawIndexMap.
            int rawStartIndex = searchData.NormalizedToRawIndexMap[normalizedMatchIndex];
            // The end index from the map is inclusive, so we will add 1 to it when creating the TextRange to make it exclusive.
            int rawEndInclusiveIndex = searchData.NormalizedToRawIndexMap[normalizedEndIndex];

            // Validate the raw indices before attempting to create the TextRange.
            if (rawStartIndex < 0 || rawStartIndex >= searchData.RawTextPointers.Count)
                return null;

            // The raw end index is inclusive, so it can be equal to the count of raw text pointers
            // (which means the match goes to the end of the document), but it cannot be greater than that.
            if (rawEndInclusiveIndex < 0 || rawEndInclusiveIndex >= searchData.RawTextPointers.Count)
                return null;

            // Now that we have validated the indices, we can create a TextRange from the raw TextPointers.
            TextPointer start = searchData.RawTextPointers[rawStartIndex];
            // The end TextPointer should be one position after the raw end index to make the range inclusive of the end character.
            TextPointer end = rawEndInclusiveIndex + 1 < searchData.RawTextPointers.Count
                ? searchData.RawTextPointers[rawEndInclusiveIndex + 1]
                : richTextBox.Document.ContentEnd;

            return new TextRange(start, end);
        }

        /// <summary>
        /// This helper walks through the RichTextBox document and removes the background highlight from any 
        /// TextRanges that were highlighted as search matches.
        /// Does support theme skinning by checking for the DynamicResource reference to the highlight brush 
        /// defined in the app resources, with a fallback to Brushes.Yellow for any themes that have not 
        /// yet defined the resource.
        /// </summary>
        /// <param name="richTextBox">RichEdit control to reset</param>
        private void ResetSpotlights(RichTextBox richTextBox)
        {
            searchMatches.Clear();
            currentSearchMatchIndex = -1;

            // Walk through the RichTextBox document and remove only the yellow search/selection
            // background that this window applies. Other formatting is left alone.
            TextPointer pointer = richTextBox.Document.ContentStart;

            // Similar to the BuildRichTextSearchData method, we need to iterate through the document using TextPointers.
            while (pointer != null && pointer.CompareTo(richTextBox.Document.ContentEnd) < 0)
            {
                // When the pointer is at text content, we want to check if it has the search highlight background and clear it if so.
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    // Get the run of text in this element and check its background property.
                    TextPointer start = pointer;
                    TextPointer end = pointer.GetNextContextPosition(LogicalDirection.Forward);

                    if (end != null)
                    {
                        // Check if the background of this text run is the search highlight color that we applied. If so, clear it.
                        TextRange range = new TextRange(start, end);
                        // For theme skinning, we check against the DynamicResource reference to the highlight brush
                        // defined in the app resources, with a fallback to Brushes.Yellow for any themes that have not
                        // yet defined the resource.
                        object background = range.GetPropertyValue(TextElement.BackgroundProperty);

                        if (background != null)
                        {
                            // If the background is a SolidColorBrush, we want to check its color against the highlight color as well,
                            // to account for cases where the brush instances are different but the color is the same (e.g. due to theme changes).
                            if (background is SolidColorBrush backgroundBrush)
                            {
                                object themedHighlight = Application.Current.Resources["NoteSearchHighlightBrush"];
                                if (themedHighlight is SolidColorBrush themedHighlightBrush && backgroundBrush.Color == themedHighlightBrush.Color)
                                    continue;
                                if (backgroundBrush.Color == Colors.Yellow)
                                    continue;
                            }
                        }

                        // Only clear the background if it matches the highlight brush we use. This allows the reset
                        // to coexist with other background formatting that may be present in the document.
                        if (IsOwnedHighlightBrush(background))
                            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        /// <summary>
        /// This helper checks if a given background value matches the highlight brush that this window uses for search highlights.
        /// </summary>
        /// <param name="backgroundValue">The background value to check.</param>
        /// <returns>True if the background value matches the highlight brush; otherwise, false.</returns>
        private bool IsOwnedHighlightBrush(object? backgroundValue)
        {
            if (backgroundValue == null)
                return false;

            if (backgroundValue.Equals(Brushes.Yellow))
                return true;

            // For theme skinning, we check against the DynamicResource reference to the highlight brush defined in the app resources.
            object themedHighlight = Application.Current.Resources["NoteSearchHighlightBrush"];
            if (themedHighlight != null && backgroundValue.Equals(themedHighlight))
                return true;

            // Additionally, if the background is a SolidColorBrush, we check its color against the highlight color as well,
            if (backgroundValue is SolidColorBrush backgroundBrush)
            {
                // This accounts for cases where the brush instances are different but the color is the same (e.g. due to theme changes).
                if (themedHighlight is SolidColorBrush themedHighlightBrush && backgroundBrush.Color == themedHighlightBrush.Color)
                    return true;

                // Fallback check for the default highlight color used in themes that have not defined the resource.
                if (backgroundBrush.Color == Colors.Yellow)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Move selection to the next highlighted search result in the current note.
        /// </summary>
        private void GoToNextSearchMatch()
        {
            if (searchMatches.Count == 0)
            {
                // If there are no current search matches, but the user is trying to navigate
                // search results, it likely means the content has changed since the last search.
                // In this case, we should re-run the search to update the matches before navigating.
                RunSearchFromCurrentText();
                if (searchMatches.Count == 0)
                    return;
            }

            currentSearchMatchIndex++;
            if (currentSearchMatchIndex >= searchMatches.Count)
                currentSearchMatchIndex = 0;

            // Select the new current match and scroll it into view.
            SelectSearchMatch(currentSearchMatchIndex);
        }

        /// <summary>
        /// Move selection to the previous highlighted search result in the current note.
        /// </summary>
        private void GoToPreviousSearchMatch()
        {
            if (searchMatches.Count == 0)
            {
                RunSearchFromCurrentText();
                if (searchMatches.Count == 0)
                    return;
            }

            currentSearchMatchIndex--;
            if (currentSearchMatchIndex < 0)
                currentSearchMatchIndex = searchMatches.Count - 1;

            SelectSearchMatch(currentSearchMatchIndex);
        }

        /// <summary>
        /// Select and focus a specific highlighted match by its index in the search results list.
        /// </summary>
        /// <param name="matchIndex">The index of the match to select.</param>
        private void SelectSearchMatch(int matchIndex)
        {
            if (matchIndex < 0 || matchIndex >= searchMatches.Count)
                return;

            TextRange selectedRange = searchMatches[matchIndex];
            isHandlingEditorSelectionChange = true;
            rchEditNote.Selection.Select(selectedRange.Start, selectedRange.End);
            isHandlingEditorSelectionChange = false;
            rchEditNote.Focus();
        }

        /// <summary>
        /// Re-run note search from the current contents of the search textbox.
        /// Re-runs the SpotlightSearch() func which will clear existing highlights 
        /// and apply new ones based on the current search text. This is useful 
        /// to keep the search results in sync with the search box content, 
        /// especially after edits to the note that may have changed the matches. 
        /// It is called when navigating search results if there are no current 
        /// matches, which likely indicates that the note content has changed 
        /// since the last search. By re-running the search, we can ensure that 
        /// the highlights and navigation are based on the most up-to-date 
        /// content and search query.
        /// </summary>
        private void RunSearchFromCurrentText()
        {
            string searchText = NoteSearch.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(searchText) || searchText == UI_TEXTBOX_STR_SEARCH)
                return;

            // Re-run the search with the current search text to update highlights and matches.
            SpotlightSearch(rchEditNote, searchText);
        }

        /// <summary>
        /// Reset the note search textbox and remove all current in-note highlights.
        /// </summary>
        private void ClearNoteSearch()
        {
            NoteSearch.Text = UI_TEXTBOX_STR_SEARCH;
            selectionSpotlightTimer.Stop();
            lastSelectionSpotlightText = string.Empty;
            
            ResetSpotlights(rchEditNote);
        
            isSelectionSpotlightActive = false;
            isHandlingEditorSelectionChange = true;
            rchEditNote.Selection.Select(rchEditNote.CaretPosition, rchEditNote.CaretPosition);
            isHandlingEditorSelectionChange = false;
        }


        /// <summary>
        /// Handler for double-clicking the note title textbox, which selects all text for easy replacement.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void NotesTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    } //NoteTemplateEditor: Window
} //Namespace Jotter
