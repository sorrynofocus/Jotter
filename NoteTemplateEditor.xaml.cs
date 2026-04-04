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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        // Prevent selection-change reentry while code updates the editor selection.
        private bool isHandlingEditorSelectionChange = false;
        // Debounce selection-based highlighting so normal caret movement does not thrash the editor.
        // RichTextBox fires a lot of selection notifications while the mouse is dragging or while the
        // caret is moving with the keyboard, so we wait briefly before searching and painting matches.
        private readonly DispatcherTimer selectionSpotlightTimer;
        // Avoid re-running the same selection highlight repeatedly.
        // If the user keeps the same word selected, there is no reason to rebuild the same highlight set.
        private string lastSelectionSpotlightText = string.Empty;

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

            //Set the richedit with the note contents
            //XAML RichTextBox x:Name="rchEditNote"
            rchEditNote.Document.Blocks.Clear();
            rchEditNote.Document.Blocks.Add(new Paragraph(new Run(noteContents)));

            // Apply the current theme's editor text color to the loaded document.
            TextRange documentRange = new TextRange(rchEditNote.Document.ContentStart, rchEditNote.Document.ContentEnd);
            documentRange.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("NoteEditForegroundBrush"));
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
            }
            else
                // Add the SelectedNote to the Notes collection if it doesn't exist
                Notes.Add(SelectedNote);

            // Remove the SelectedNote from the Notes collection if it should be deleted
            if (SelectedNote.IsDeleted)
                Notes.Remove(SelectedNote);

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
        /// RichEdit event handler to handle text change and start auto save timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RchEditNote_TextChanged(object sender, TextChangedEventArgs e)
        {
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

            string selectedText = NormalizeSearchText(rchEditNote.Selection?.Text);
            if (selectedText.Length < 2)
            {
                ClearSelectionSpotlight();
                return;
            }

            selectionSpotlightTimer.Start();
        }

        private void RchEditNote_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // When the mouse button is released, the user is usually done selecting text.
            // Queue the spotlight update here so the highlight pass runs after the final selection settles.
            QueueSelectionSpotlightUpdate();
        }

        private void RchEditNote_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard selection changes (Shift+Arrow, Ctrl+Shift+Arrow, etc.) land here.
            // Reuse the same deferred update path as mouse selection completion.
            QueueSelectionSpotlightUpdate();
        }

        private void QueueSelectionSpotlightUpdate()
        {
            if (isHandlingEditorSelectionChange)
                return;

            selectionSpotlightTimer.Stop();

            // If the user is in explicit search mode, leave the current search highlights alone.
            if (HasActiveNoteSearch())
                return;

            string selectedText = NormalizeSearchText(rchEditNote.Selection?.Text);
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

            string normalizedSearchText = NormalizeSearchText(searchText);
            if (string.IsNullOrWhiteSpace(normalizedSearchText))
                return;

            RichTextSearchData searchData = BuildRichTextSearchData(richTextBox);
            if (string.IsNullOrWhiteSpace(searchData.NormalizedText))
                return;

            int searchStartIndex = 0;
            while (searchStartIndex < searchData.NormalizedText.Length)
            {
                int matchIndex = searchData.NormalizedText.IndexOf(normalizedSearchText, searchStartIndex, StringComparison.OrdinalIgnoreCase);
                if (matchIndex < 0)
                    break;

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

        private bool HasActiveNoteSearch()
        {
            string searchText = NoteSearch.Text?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(searchText) && searchText != UI_TEXTBOX_STR_SEARCH;
        }

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
            List<TextPointer> rawTextPointers = new List<TextPointer>();
            System.Text.StringBuilder rawTextBuilder = new System.Text.StringBuilder();

            TextPointer pointer = richTextBox.Document.ContentStart;
            while (pointer != null && pointer.CompareTo(richTextBox.Document.ContentEnd) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    for (int i = 0; i < textRun.Length; i++)
                    {
                        TextPointer charPointer = pointer.GetPositionAtOffset(i, LogicalDirection.Forward);
                        if (charPointer != null)
                        {
                            rawTextBuilder.Append(textRun[i]);
                            rawTextPointers.Add(charPointer);
                        }
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }

            List<int> normalizedToRawIndexMap = new List<int>();
            System.Text.StringBuilder normalizedTextBuilder = new System.Text.StringBuilder();

            for (int i = 0; i < rawTextBuilder.Length; i++)
            {
                char currentChar = rawTextBuilder[i];
                if (char.IsWhiteSpace(currentChar))
                {
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

        private TextRange? CreateTextRangeFromNormalizedMatch(RichTextBox richTextBox, RichTextSearchData searchData, int normalizedMatchIndex, int normalizedMatchLength)
        {
            if (normalizedMatchIndex < 0 ||
                normalizedMatchLength <= 0 ||
                normalizedMatchIndex >= searchData.NormalizedToRawIndexMap.Count)
            {
                return null;
            }

            int normalizedEndIndex = normalizedMatchIndex + normalizedMatchLength - 1;
            if (normalizedEndIndex >= searchData.NormalizedToRawIndexMap.Count)
                return null;

            int rawStartIndex = searchData.NormalizedToRawIndexMap[normalizedMatchIndex];
            int rawEndInclusiveIndex = searchData.NormalizedToRawIndexMap[normalizedEndIndex];

            if (rawStartIndex < 0 || rawStartIndex >= searchData.RawTextPointers.Count)
                return null;

            if (rawEndInclusiveIndex < 0 || rawEndInclusiveIndex >= searchData.RawTextPointers.Count)
                return null;

            TextPointer start = searchData.RawTextPointers[rawStartIndex];
            TextPointer end = rawEndInclusiveIndex + 1 < searchData.RawTextPointers.Count
                ? searchData.RawTextPointers[rawEndInclusiveIndex + 1]
                : richTextBox.Document.ContentEnd;

            return new TextRange(start, end);
        }

        /// <summary>
        /// Clear all highlighting or spotlights of text in a RichTextBox from previous successful finds
        /// </summary>
        /// <param name="richTextBox">RichEdit control to reset</param>
        private void ResetSpotlights(RichTextBox richTextBox)
        {
            searchMatches.Clear();
            currentSearchMatchIndex = -1;

            // Walk through the RichTextBox document and remove only the yellow search/selection
            // background that this window applies. Other formatting is left alone.
            TextPointer pointer = richTextBox.Document.ContentStart;

            while (pointer != null && pointer.CompareTo(richTextBox.Document.ContentEnd) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    TextPointer start = pointer;
                    TextPointer end = pointer.GetNextContextPosition(LogicalDirection.Forward);

                    if (end != null)
                    {
                        TextRange range = new TextRange(start, end);
                        object background = range.GetPropertyValue(TextElement.BackgroundProperty);

                        if (background != null && background.Equals(Brushes.Yellow))
                            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        /// <summary>
        /// Move selection to the next highlighted search result in the current note.
        /// </summary>
        private void GoToNextSearchMatch()
        {
            if (searchMatches.Count == 0)
            {
                RunSearchFromCurrentText();
                if (searchMatches.Count == 0)
                    return;
            }

            currentSearchMatchIndex++;
            if (currentSearchMatchIndex >= searchMatches.Count)
                currentSearchMatchIndex = 0;

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
        /// </summary>
        private void RunSearchFromCurrentText()
        {
            string searchText = NoteSearch.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(searchText) || searchText == UI_TEXTBOX_STR_SEARCH)
                return;

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

        private sealed class RichTextSearchData
        {
            public List<TextPointer> RawTextPointers { get; set; } = new List<TextPointer>();
            public string NormalizedText { get; set; } = string.Empty;
            public List<int> NormalizedToRawIndexMap { get; set; } = new List<int>();
        }

        private void NotesTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    } //NoteTemplateEditor: Window
} //Namespace Jotter
