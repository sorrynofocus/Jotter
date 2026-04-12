/*
* Jotter
* NoteManager
* C.Winters / US / Arizona / Thinkpad T15g
* March 2024
 * 
 * Remember: If it doesn't work, I didn't write it.
 * 
 * Purpose 
 * NoteManager class handles some of the CRUD operations. Currently, the data is XML based.
 * 
 * TODO: I'm noticing variables are using different casing styles. I should standardize that. 
 * This whole application has been a large sandbox and throwing around typical variable names 
 * like title and text instead of noteTitle and noteText.As the application grows, this all 
 * needs cleaning up.
 */
using Jotter;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using static Jotter.MainWindow;

namespace com.nobodynoze.notemanager
{
    /// <summary>
    /// MediaItem class represents a media attachment associated with a note.
    /// It stores the type of media, a reference to the media file, and the original file name.
    /// </summary>
    /// <remarks>
    /// This class is used by the Note class to associate media files such as images, videos, or audio
    /// with a particular note. Each MediaItem stores the type of media, a reference to the file location,
    /// and the original file name for display purposes.
    /// </remarks>
    public class MediaItem
    {
        public string? MediaType { get; set; }
        public string? MediaRef { get; set; }
        public string? OriginalFileName { get; set; }
    }
    /// <summary>
    /// MediaPreviewItem class represents a preview of a media item for display purposes in the UI.
    /// It contains a reference to the MediaItem, a path to a preview image, and properties to handle
    /// overflow display when there are more media items than can be shown at once.
    /// </summary>
    /// <remarks>
    /// This class is used by the UI layer to display a preview of media items associated with a note.
    /// It wraps a MediaItem and provides a path to a preview image for display, as well as properties
    /// to handle overflow tiles when there are more media items than can be displayed at once in the UI.
    /// The DisplayText and ToolTipText properties provide user-friendly text for overflow tiles and
    /// for showing the original file name of the media item in tooltips.
    /// </remarks>

    public class MediaPreviewItem
    {
        /// <summary>
        /// Reference to the MediaItem that this preview item represents.
        /// </summary>
        public MediaItem? MediaItem { get; set; }
        /// <summary>
        /// Path to the preview image file that will be displayed in the UI for this media item.
        /// </summary>
        public string? PreviewFilePath { get; set; }
        /// <summary>
        /// Indicates whether this preview item represents an overflow tile in the UI
        /// when there are more media items than can be displayed at once.
        /// </summary>
        public bool IsOverflowTile { get; set; }
        /// <summary>
        /// The number of additional media items that are not displayed in the UI
        /// and are represented by this overflow tile.
        /// </summary>
        /// <remarks>
        /// This property is used in conjunction with the IsOverflowTile property to indicate
        /// how many additional media items exist that are not currently displayed in the UI.
        /// When the number of media items exceeds the display capacity, an overflow tile is
        /// shown with a count of the remaining items, which is represented by this property.
        /// </remarks>
        public int OverflowCount { get; set; }
        /// <summary>
        /// DisplayText property returns the text that should be displayed on the UI tile
        /// for this media preview item. If this item represents an overflow tile, it will
        /// display the number of additional media items that are not currently visible.
        /// Otherwise, it returns an empty string.
        /// </summary>
        /// <remarks>
        /// This property is used by the UI to determine what text should be displayed on the tile
        /// representing this media preview item. If the item is an overflow tile, it will show
        /// the number of additional media items that are not currently visible in the UI, prefixed
        /// with a plus sign. If it is not an overflow tile, it returns an empty string so that
        /// no text is displayed on the tile.
        /// </remarks>
        public string DisplayText => IsOverflowTile ? $"+{OverflowCount}" : string.Empty;
        
        /// <summary>
        /// ToolTipText property returns the text that should be displayed in the tooltip
        /// for this media preview item. If this item represents an overflow tile, it will
        /// display the number of additional media items that are not currently visible.
        /// Otherwise, it will display the original file name of the media item.
        /// </summary>
        /// <remarks>
        /// This property is used by the UI to determine what text should be displayed in the tooltip
        /// when the user hovers over a media preview tile. If the item is an overflow tile, it will
        /// show the number of additional media items that are not currently visible in the UI.
        /// If it is not an overflow tile, it will show the original file name of the media item
        /// so that the user can see which file the preview represents.
        /// </remarks>
        public string ToolTipText => IsOverflowTile
            ? $"{OverflowCount} more image(s)"
            : MediaItem?.OriginalFileName ?? string.Empty;
    }

    /// <summary>
    ///Note class embodies the note data and its properties such as title, index, and body.
    /// It also tracks creation date, soft deletion status, and associated media items.
    /// </summary>
    [XmlRoot("ArrayOfNote")]
    public class Note : INotifyPropertyChanged
    {
        private Guid? IdIndex =Guid.Empty;
        private string? title;
        private string? text;
        private bool isDeleted; // New property for soft delete - added in for experimental
        private DateTime createdDate;
        private List<MediaItem>? media;

        ////Width x Height
        //public double noteWidth { get; set; } = 450;
        //public double noteHeight { get; set; } = 600;
        ////Xpos
        //public double noteLeft { get; set; } = 100;
        ////YPos
        //public double noteTop { get; set; } = 100;

        //Property change handler - happens when a property changes (in the Note class)
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The IdIndex property in the Note class stores unique identifiers for each notes created.
        /// </summary>
        public Guid? IdIndexer
        {
            get { return IdIndex; }
            set
            {
                if (IdIndex != value)
                {
                    IdIndex = value;
                    OnPropertyChanged(nameof(IdIndex));
                }
            }
        }

        /// <summary>
        /// The title property of each note
        /// </summary>
        public string? Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        /// <summary>
        /// The body property of each note
        /// </summary>
        public string? Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        /// <summary>
        /// Flag the note to be removed from the NoteManager
        /// </summary>
        public bool IsDeleted
        {
            get { return isDeleted; }
            set
            {
                if (isDeleted != value)
                {
                    isDeleted = value;
                    OnPropertyChanged(nameof(IsDeleted));
                }
            }
        }

        /// <summary>
        /// Date the note was created property.
        /// </summary>
        public DateTime CreatedDate
        {
            get { return createdDate; }
            set
            {
                if (createdDate != value)
                {
                    createdDate = value;
                    OnPropertyChanged(nameof(CreatedDate));
                }
            }
        }

        /// <summary>
        /// Collection of media items associated with this note.
        /// </summary>
        /// <remarks>
        /// This property holds the list of media items that are attached
        /// to this note. It is serialized as an XML array when the note is saved to disk, and
        /// each individual media item is represented as a MediaItem element within that array.
        /// The UI can use this collection to display previews of the media associated with the note.
        /// </remarks>
        [XmlArray("Media")]
        [XmlArrayItem("MediaItem")]
        public List<MediaItem>? Media
        {
            get { return media; }
            set
            {
                if (!ReferenceEquals(media, value))
                {
                    media = value;
                    OnPropertyChanged(nameof(Media));
                }
            }
        }


        [XmlIgnore]
        public List<MediaPreviewItem> PreviewMedia => NoteMediaStorage.BuildPreviewItems(media, 4);

        ///// <summary>
        ///// Position of the note property on screen.
        ///// </summary>
        //public (double Left, double Top) NotePos
        //{
        //    get => (noteLeft, noteTop);
        //    set
        //    {
        //        noteLeft = value.Left;
        //        noteTop = value.Top;
        //    }
        //}

        ///// <summary>
        ///// Dimension of the note property. (grouped property)
        ///// </summary>
        //public (double Width, double Height) NoteDim
        //{
        //    get => (noteWidth, noteHeight);
        //    set
        //    {
        //        noteWidth = value.Width;
        //        noteHeight = value.Height;
        //    }
        //}


        /// <summary>
        /// Snitches to the listeners a property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// NoteMediaStorage class handles the storage and management of media files associated with 
    /// notes, including determining file paths, mapping media types to file extensions, 
    /// and managing media file deletion.
    /// </summary>
    public static class NoteMediaStorage
    {
        public const string MediaDirectoryName = "Media";
        private const int MAX_RETRY_DELETE_MEDIA_ATTEMPTS = 3;

        /// <summary>
        /// Gets the file path to the media directory where media files associated with notes are stored.
        /// </summary>
        /// <returns>The file path to the media directory.</returns>
        public static string GetMediaDirectoryPath()
        {
            string? notesDirectory = Path.GetDirectoryName(MainWindow.jotNotesFilePath);

            if (string.IsNullOrWhiteSpace(notesDirectory))
                notesDirectory = SettingsMgr.DefaultDataDirectory;

            return Path.Combine(notesDirectory, MediaDirectoryName);
        }

        /// <summary>
        /// This function checks if the media directory exists and creates it if 
        /// it doesn't. This ensures that there is a valid location for storing media 
        /// files associated with notes, preventing errors when trying to save media 
        /// files to a non-existent directory. It should be called during application 
        /// initialization to set up the necessary directory.
        /// </summary>
        public static void InitMediaDirectoryExists()
        {
            string mediaDirectoryPath = GetMediaDirectoryPath();

            if (!Directory.Exists(mediaDirectoryPath))
                Directory.CreateDirectory(mediaDirectoryPath);
        }

        /// <summary>
        /// This function maps a media type (MIME type) to a corresponding file extension for storage purposes.
        /// </summary>
        /// <param name="mediaType">The media type (MIME type) to map to a file extension.</param>
        /// <returns>The corresponding file extension for the given media type, or an empty string if unsupported.</returns>
        public static string GetStoredFileExtension(string? mediaType)
        {
            return mediaType?.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/tiff" => ".tif",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the media type (MIME type) based on the file extension of the media file. 
        /// This is used to determine the type of media being handled and to ensure that 
        /// only supported media types are processed.
        /// </summary>
        /// <param name="fileExtension">The file extension of the media file.</param>
        /// <returns>The corresponding media type (MIME type) for the given file extension, or an empty string if unsupported.</returns>
        public static string GetMediaTypeFromExtension(string? fileExtension)
        {
            return fileExtension?.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tif" => "image/tiff",
                ".tiff" => "image/tiff",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Does a quick check to see if the file extension of the provided file path 
        /// corresponds to a supported image media type.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file is a supported image file, false otherwise.</returns>
        public static bool IsSupportedImageFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            string fileExtension = Path.GetExtension(filePath);
            return GetMediaTypeFromExtension(fileExtension) != string.Empty;
        }

        /// <summary>
        /// This function constructs the file path for a media item based on its 
        /// MediaRef and MediaType properties.
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <returns></returns>
        public static string GetMediaFilePath(MediaItem? mediaItem)
        {
            if (mediaItem == null || string.IsNullOrWhiteSpace(mediaItem.MediaRef))
                return string.Empty;

            string fileExtension = GetStoredFileExtension(mediaItem.MediaType);
            if (string.IsNullOrWhiteSpace(fileExtension))
                return string.Empty;

            return Path.Combine(GetMediaDirectoryPath(), $"{mediaItem.MediaRef}{fileExtension}");
        }

        /// <summary>
        /// Deletes the media files associated with the provided list of MediaItem objects. 
        /// It checks if each media file exists and deletes it from the file system.
        /// </summary>
        /// <param name="mediaItems">The list of media items whose associated files are to be deleted.</param>
        public static void DeleteMediaFiles(List<MediaItem>? mediaItems)
        {
            if (mediaItems == null || mediaItems.Count == 0)
                return;

            foreach (MediaItem mediaItem in mediaItems)
            {
                string mediaFilePath = GetMediaFilePath(mediaItem);
                if (!string.IsNullOrWhiteSpace(mediaFilePath) && File.Exists(mediaFilePath))
                    File.Delete(mediaFilePath);
            }
        }

        /// <summary>
        /// This function attempts to delete a media file associated with a given MediaItem. 
        /// It checks if the file exists and tries to delete it, with retries in case of IO 
        /// exceptions or unauthorized access exceptions. It returns true if the file was 
        /// successfully deleted or if it doesn't exist, and false if it failed to delete 
        /// after multiple attempts.
        /// </summary>
        /// <param name="mediaItem">The media item whose associated file is to be deleted.</param>
        /// <returns>True if the file was successfully deleted or doesn't exist, false otherwise.</returns>
        public static bool TryDeleteMediaFile(MediaItem? mediaItem)
        {
            logger.LogInfo(new List<string> { "[Doing action] TryDeleteMediaFile", "Attempting to delete media file." });

            string mediaFilePath = GetMediaFilePath(mediaItem);
            
            if (string.IsNullOrWhiteSpace(mediaFilePath) || !File.Exists(mediaFilePath))
            {
                logger.LogInfo(new List<string> { "[Ending action] TryDeleteMediaFile", $"Media file does not exist, no need to delete: {mediaFilePath}" });
                return true;
            }

            for (int retry = 0; retry < MAX_RETRY_DELETE_MEDIA_ATTEMPTS; retry++)
            {
                try
                {
                    File.Delete(mediaFilePath);
                    logger.LogInfo(new List<string> { "[Ending action] TryDeleteMediaFile", $"Successfully deleted media file: {mediaFilePath}" });
                    return true;
                }
                catch (IOException)
                {
                    logger.LogInfo(new List<string> { "[Retrying action] TryDeleteMediaFile", $"IOException encountered when trying to delete media file: {mediaFilePath}. Retrying..." });
                    System.Threading.Thread.Sleep(125);
                }
                catch (UnauthorizedAccessException)
                {
                    logger.LogInfo(new List<string> { "[Retrying action] TryDeleteMediaFile", $"UnauthorizedAccessException encountered when trying to delete media file: {mediaFilePath}. Retrying..." });
                    System.Threading.Thread.Sleep(125);
                }
            }
            logger.LogInfo(new List<string> { "[Ending action] TryDeleteMediaFile", $"Failed to delete media file: {mediaFilePath}" });
            return false;
        }

        /// <summary>
        /// This function builds a list of MediaPreviewItem objects for a given list of MediaItem objects, 
        /// considering the maximum number of visible slots. If the number of media items exceeds the 
        /// maximum visible slots, an overflow tile is added to indicate the remaining items.
        /// </summary>
        /// <param name="mediaItems">The list of media items to build preview items for.</param>
        /// <param name="maxVisibleSlots">The maximum number of visible slots for preview items.</param>
        /// <returns>A list of MediaPreviewItem objects representing the preview items.</returns>
        public static List<MediaPreviewItem> BuildPreviewItems(List<MediaItem>? mediaItems, int maxVisibleSlots)
        {
            logger.LogInfo(new List<string> { "[Doing action] BuildPreviewItems", "Building media preview items." });

            List<MediaPreviewItem> previewItems = new List<MediaPreviewItem>();

            if (mediaItems == null || mediaItems.Count == 0 || maxVisibleSlots <= 0)
                return previewItems;

            // If the number of media items is less than or equal to the maximum visible slots, create preview items for all media items.
            if (mediaItems.Count <= maxVisibleSlots)
            {
                foreach (MediaItem mediaItem in mediaItems)
                {
                    previewItems.Add(new MediaPreviewItem
                    {
                        MediaItem = mediaItem,
                        PreviewFilePath = GetMediaFilePath(mediaItem)
                    });
                }
                logger.LogInfo(new List<string> { "[Ending action] BuildPreviewItems", $"Finish building media preview items. Number of media items: {mediaItems.Count}" });
                return previewItems;
            }

            // If there are more media items than the maximum visible slots, create preview items for the first (maxVisibleSlots - 1)
            // media items and add an overflow tile to indicate the number of remaining items. 
            int standardTileCount = Math.Max(1, maxVisibleSlots - 1);
            for (int i = 0; i < standardTileCount; i++)
            {
                MediaItem mediaItem = mediaItems[i];
                previewItems.Add(new MediaPreviewItem
                {
                    MediaItem = mediaItem,
                    PreviewFilePath = GetMediaFilePath(mediaItem)
                });
            }

            // Add an overflow tile to indicate the number of remaining media items that are not displayed in the preview.
            previewItems.Add(new MediaPreviewItem
            {
                IsOverflowTile = true,
                OverflowCount = mediaItems.Count - standardTileCount
            });

            logger.LogInfo(new List<string> { "[Ending action] BuildPreviewItems", $"Finish building media preview items. Number of media items: {mediaItems.Count}, Number of preview items: {previewItems.Count}" });

            return previewItems;
        }
    }

    //~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*~-*

    /// <summary>
    /// NoteManager class handles XAML CRUD operations for note taking
    /// </summary>
    public class NoteManager
    {
        //Tie the UI to collection
        //In this application ListView Name="MyNotesListView" is Binded to "Notes" class var
        public ObservableCollection<Note>? Notes { get; set; }

        //Use XML for CRUD operations
        private XmlSerializer? serializer;

        //public NoteManager()
        //{
        //    Notes = LoadNotes(serializer);
        //      if (Notes == null)
        //        JustExit();
        //}
        //Instead of use block body, use expression body for constructor
        public NoteManager() => Notes = LoadNotes(serializer, jotNotesFilePath) ?? JustExit();


        //Keep a dictionary of note indexing. Can't figure out how to get the title text box tags since data is dynamic
        public Dictionary<Guid, Note> idIndexerNoteMap = new Dictionary<Guid, Note>();

        /// <summary>
        /// Loads and deserializes notes from XML-based data file into a NoteManager 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private ObservableCollection<Note>? LoadNotes(XmlSerializer serializer, string? filePath)
        {
            logger.LogInfo(new List<string> { "[Doing action] LoadNotes", "Reading notes into RAM." });

            if (File.Exists(filePath))
            {
                    try
                    {
                        serializer = new XmlSerializer(typeof(ObservableCollection<Note>));

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                        {
                            // return serializer.Deserialize(fileStream) as ObservableCollection<Note>;
                            ObservableCollection<Note>? loadedNotes = serializer.Deserialize(fileStream) as ObservableCollection<Note>;

                            if (loadedNotes != null)
                            {
                                idIndexerNoteMap.Clear();

                                // Populate the dictionary with notes and their IdIndexer values
                                //If you want to set defaults in your notes, do it here!
                                foreach (Note note in loadedNotes)
                                {
                                    if (note.IdIndexer != null)
                                        idIndexerNoteMap[note.IdIndexer.Value] = note;

                                    // Check if CreatedDate is missing or not initialized.
                                    //IF not, assign today's date as a fallback.
                                    if (note.CreatedDate == default(DateTime))
                                        note.CreatedDate = DateTime.Now; 
                                }
                            }
                            return loadedNotes;
                        }
                    }
                    catch (Exception? ex)
                    {
                        string? exceptionMessage = string.Empty;

                        //message will try to show area in XML the problem is
                        //ex.Message example: There is an error in XML document (6, 6).
                        string[]? arBits = ex.Message.Split(new char[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                        string? rowNum = string.Empty;
                        string? columnNum = string.Empty;

                        if (arBits.Length >= 2)
                        {
                            rowNum = arBits[1].Trim();
                            columnNum = arBits[2].Trim();
                            //FormatException: Unrecognized Guid format
                            // There is an error in XML document (6, 6).
                            exceptionMessage = ex.InnerException?.Message ?? ex.Message;
                            exceptionMessage += $" Examine Row {rowNum}, Column {columnNum} in the notes data file: {filePath}.";
                        }


                        if (ex.InnerException?.Message.Contains("Unrecognized Guid format") == true ||
                            ex.InnerException?.Message.Contains("invalid XmlNodeType") == true ||
                            ex.Message.Contains("error in XML") == true)
                            {
                                MessageBoxResult result = MessageBox.Show(exceptionMessage, "Invalid config!", MessageBoxButton.OK);
                        
                                CreateBackup(filePath);

                            }
                            //eh.. return an empty collection
                            return (null);
                    }
            }
            else
                return new ObservableCollection<Note>();
        }

        //Safety check function if loading doesn't work based on bad data.
        private ObservableCollection<Note> JustExit()
        {
            MessageBox.Show("Failed to load notes. Exiting application.", "Error", MessageBoxButton.OK);
            Environment.Exit(1); 
            return new ObservableCollection<Note>(); 
        }

        public int GetIndexOfSelectedNoteById(Guid idIndexer)
        {
            if (Notes == null)
                return -1;

            for (int i = 0; i < Notes.Count; i++)
            {
                if (Notes[i].IdIndexer == idIndexer)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Add or update a note into the NoteManager
        /// </summary>
        /// <param name="note"></param>
        public void AddUpdateNote(Note note)
        {
            logger.LogInfo(new List<string> { "[Doing action] AddUpdateNote", "Adding/Updating selected note." });
            Note? existingNote = Notes.FirstOrDefault(n => n.Title == note.Title);

            if (existingNote != null)
            {
                existingNote.Text = note.Text;
                existingNote.Media = note.Media;
            }
            else
                Notes.Add(note);

            SaveNotes(jotNotesFilePath);
            logger.LogInfo("[Ending action] AddUpdateNote");
        }

        /// <summary>
        /// Removes a note from the NoteManager
        /// </summary>
        /// <param name="title"></param>
        public void RemoveNote(string title)
        {
            //TODO: remove note by ID. Not by title
            //Create func: RemoveNoteByIdIndex()
            //noteManager.RemoveNoteByIdIndex(idIndexer.Value);

            logger.LogInfo(new List<string> { "[Doing action] RemoveNote", "Deleting selected note." });

            Note? noteToRemove = Notes.FirstOrDefault(n => n.Title == title);
            if (noteToRemove != null)
            {
                NoteMediaStorage.DeleteMediaFiles(noteToRemove.Media);
                Notes.Remove(noteToRemove);
                SaveNotes(jotNotesFilePath);
            }
            logger.LogInfo("[Ending action] RemoveNote");
        }

        /// <summary>
        /// Serializes and saves a note into an XML-based data file
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveNotes(string? filePath)
        {
            logger.LogInfo(new List<string> { "[Doing action] SaveNotes", "Recording note in RAM." });

            if ((filePath == string.Empty))
            {
                MessageBox.Show("Cannot save data to file ${filePath}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
                serializer.Serialize(fileStream, Notes);
            }
            logger.LogInfo("[Ending action] SaveNotes");
        }

        /// <summary>
        /// Generate a GUID to be used for the indexer in the NoteManager
        /// </summary>
        /// <returns></returns>
        public Guid GenerateID() => Guid.NewGuid();

        /// <summary>
        /// Search for note with specified IdIndexer and update it
        /// </summary>
        /// <param name="idIndexer">the GUID index to look up</param>
        /// <param name="newTitle">the new title of the note</param>
        /// <param name="newText">the new text/body of the note</param>
        /// Example: noteManager.UpdateNoteByIdIndexer(idIndexer, newTitle, newText);
        public void UpdateNoteByIdIndexer(Guid idIndexer, string? newTitle, string? newText = null)
        {
            
            Note noteToUpdate = null;
            foreach (Note note in Notes)
            {
                if (note.IdIndexer.Equals(idIndexer))
                {
                    noteToUpdate = note;
                    //Found note
                    break; 
                }
            }

            if (noteToUpdate != null)
            {
                // Update note props
                if (newTitle != null)
                    noteToUpdate.Title = newTitle;

                if (newText != null)
                    noteToUpdate.Text = newText;
            }
        }


        /// <summary>
        /// Create backup of a source file with the unique timestamp.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// TODO - Instead of pop up message, perhaps return a string of backed up file.
        static void CreateBackup(string sourceFile)
        {
            try
            {
                // Get the filename from the source file
                string fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFile);
                string extension = System.IO.Path.GetExtension(sourceFile);

                // Generate a unique timestamp for the backup file name
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                string backupFileName = $"{fileName}_{timestamp}{extension}";
                string backupFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sourceFile), backupFileName);
                
                File.Copy(sourceFile, backupFilePath, true);

                MessageBox.Show($"Successfully copied {sourceFile}\n\r to {backupFilePath} as a backup!", "A BACKUP HAD BEEN CREATED!", MessageBoxButton.OK);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error copying file: {e.Message}", "Error!", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Searches text for a specific input within a note
        /// </summary>
        /// <param name="input">string to input you want to search</param>
        /// <param name="searchText">the text to search into</param>
        /// <returns>Returns true is the input is found in searchText, false otherwise.</returns>
        public bool SearchContext(string? input, string? searchText)
        {
            searchText = searchText?.Trim();
            input = input?.Trim();

            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(searchText))
                return (false);

            string[] wordContext = input.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in wordContext)
            {
                if (word.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    return (true);

                if (word.ToLower().Contains(searchText.ToLower()))
                    return (true);
            }

            return (false);
        }

    } //Internal class NoteManager



    /// <summary>
    /// Notify subscribers a note has been updated and pass updated note data along with the event.
    /// </summary>
    public class NoteEventArgs : EventArgs
    {
        //In NoteTemplateEditor.xaml.cs
        // Notify the parent window about the updated note before saving
        //We create a subscirption here:
        //Line: public event EventHandler<NoteEventArgs> NoteUpdated;
        //And under SaveNoteChanges1()
        // Line: NoteUpdated?.Invoke(this, new NoteEventArgs(SelectedNote));
        //I can't remember if I got SaveNoteChanges() working. I see both being used.
        //TODO: Let's see which one better since both seem to work.

        public Note UpdatedNote { get; }
        public NoteEventArgs(Note updatedNote)
        {
            UpdatedNote = updatedNote;
        }
    }

}
