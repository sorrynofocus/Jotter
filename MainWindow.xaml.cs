/*
 * Jotter
 * C. Winters / US / Arizona / Thinkpad T15g
 * Feb 2024
 * 
 * Remember: If it doesn't work, I didn't write it.
 * 
 * Purpose 
 * A note taking application. Wrote this to learn WPF and to use at work because work will not allow applicaitons like Sticky notes.
 * Ideally, would like to have this /not/ cloud based, unless you desire to move the save file to your own cloud based platform.
 */

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//imports for FileStream and XmlSerializer
using System.IO;
using System.Xml.Serialization;
using static Jotter.MainWindow;
using System.Reflection;

using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections.Specialized;
using System.ComponentModel;
using com.nobodynoze.flogger;

/*
 * TODO:
 * - Find all TODOs in code, complete those.
 * - Clean up this code. It's messy at the moment. 
 * - Move all subscriptions to another file
 * - move all utiltiies to another file
 * - move all data types; classes, data, etc to another file. 
 * - Add a "settings" to the application
 * - Change the buttons from text-based ("+" is for add, for example) to image based
 * - Test the icons. Icon test in explorer is good. Great a final icon in future releases
 * - more comments in code. For beginners looking into this, it would help greatly. 
 * - Move all these TODOs in the notes section, alnog with release notes.
 * 
 */


namespace Jotter
{
    public partial class MainWindow : Window
    {
        // Data file here: C:\Users\<USER>\AppData\Local\Jotter\JotterNotes.xml
        public static string jotNotesFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                                 Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.xml") );
        
        //Logging instance using older flogger code written a long time ago.
        public static Logger logger = new Logger();

        /*
         * Changes to the Title and Text properties of a Note object will raise the PropertyChanged event. 
         * We subscribe to this event for each Note object in the ObservableCollection<Note>. We update 
         * the isCollectionChanged flag accordingly whenever a property changes. This helps determine is 
         * the collection has been changed. At the moment, I have not decided if I need it for textbox control.
         * TODO: Fully test this.
         */
        public class Note: INotifyPropertyChanged
        {
            private string? title;
            private string? text;
            /*
             * TODO
             * - Add Id to track notes.
             * - Add tags. When user adds #{Tag} then let's find similar notes
             */

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

            //property change event handler to detect prop changes
            public event PropertyChangedEventHandler? PropertyChanged;

            //TODO: To capture we need to write Notes_PropertyChanged() and subscribe to it for change tracking. 
            // 
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<Note> Notes { get; set; }
        

        public NoteManager? noteManager;

        //Event handler for the ObserableCollection CollectionChanged event. It will be called whenever the Notes collection is modified.
        //Check the 'Action' property of the NotifyCollectionChangedEventArgs parameter to determine the type of change that occurred
        // (e.g., items added, removed, replaced, etc.).
        private bool isCollectionChanged = false;
        public void NotesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Set the flag to indicate that the collection was modified
            isCollectionChanged = true;
        }

        static void LogWrite_Handler(object source, LogEventArgs e)
        {
            Debug.WriteLine($"Log message: {e.LogMessage}");
        }


        //Entrypoint
        public MainWindow()
        {
            // Subscribe to the log writing event
            logger.LogWritten += LogWrite_Handler;
            logger.LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                 Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.log"));
            logger.EnableDTStamps = true;
            
            logger.LogInfo(new List<string> { " ", "------------------------------------------" });
            logger.LogInfo("[Doing action] Started up the generator!");

            //InitializeComponent();
            //Notes = new ObservableCollection<Note>();
            //noteManager = new NoteManager();
            //DataContext = this;
            System.Diagnostics.Debug.WriteLine(jotNotesFilePath);

            //Create directory if it does not exist for our stored data
            // Create the directory
            //string directoryPath = Path.GetDirectoryName(jotNotesFilePath);
            //System.IO.Directory.CreateDirectory(directoryPath);
            logger.LogInfo(new List<string> { "[Doing action] Creating application save/log directory: ", Path.GetDirectoryName(jotNotesFilePath) });
            CreateDirectoryFullAccess(Path.GetDirectoryName(jotNotesFilePath));


            logger.LogInfo("[Doing action] initializing components... ");
            InitializeComponent();

            logger.LogInfo("[Doing action] Init'ing NoteManager...");
            noteManager = new NoteManager();
            Notes = noteManager.Notes; // Bind to the Notes collection of NoteManager
                                       // Subscribe to the CollectionChanged event

            logger.LogInfo("[Doing action] Subscribing to collection handler...");
            //Subscribe to the Notes collection change to a handler.
            //TODO Notes properties handler for changes not implemented yet.
            Notes.CollectionChanged += NotesCollectionChangedHandler;

            logger.LogInfo("[Doing action] Starting logs: " + 
                           (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                           Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.log") ) );
            logger.LogInfo("[Doing action] Subscribing to logging handler...");

            DataContext = this;
        }



        /// <summary>
        /// NoteManager handles CRUD operations for note taking
        /// </summary>
        public class NoteManager
        {
           
            //Tie the UI to collection
            public ObservableCollection<Note>? Notes { get; set; }
            
            //Use XML for crud operations
            private XmlSerializer? serializer;


            //public NoteManager()
            //{
            //    Notes = LoadNotes(serializer);
            //}
            //Instead of use block body, use expression body for constructor
            public NoteManager() => Notes = LoadNotes(serializer, jotNotesFilePath);

            /// <summary>
            /// Add or update a note into NoteManager
            /// </summary>
            /// <param name="note"></param>
            public void AddUpdateNote(Note note)
            {
                logger.LogInfo(new List<string> { "[Doing action] AddUpdateNote", "Adding/Updating selected note." });
                Note? existingNote = Notes.FirstOrDefault(n => n.Title == note.Title);
                if (existingNote != null)
                    existingNote.Text = note.Text;
                else
                    Notes.Add(note);

                SaveNotes(jotNotesFilePath);
                logger.LogInfo("[Ending action] AddUpdateNote");
            }

            public void RemoveNote(string title)
            {
                logger.LogInfo(new List<string> { "[Doing action] RemoveNote", "Deleting selected note." });

                Note? noteToRemove = Notes.FirstOrDefault(n => n.Title == title);
                if (noteToRemove != null)
                {
                    Notes.Remove(noteToRemove);
                    SaveNotes(jotNotesFilePath);
                }
                logger.LogInfo("[Ending action] RemoveNote");
            }

            private ObservableCollection<Note>? LoadNotes(XmlSerializer serializer, string? filePath)
            {
                logger.LogInfo(new List<string> { "[Doing action] LoadNotes", "Reading notes into RAM." });

                if (File.Exists(filePath))
                {
                    using (FileStream? fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
                        //return (ObservableCollection<Note>)serializer.Deserialize(fileStream);
                        //Possible null ref, declare private ObservableCollection<Note> as nullable
                        return serializer.Deserialize(fileStream) as ObservableCollection<Note>;
                    }
                }
                else
                    return new ObservableCollection<Note>();
            }

            public void SaveNotes(string? filePath)
            {
                logger.LogInfo(new List<string> { "[Doing action] SaveNotes", "Recording note in RAM." });

                if ( (filePath == string.Empty) )
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
        }


        private void CreateDirectoryFullAccess(string file)
        {
            logger.LogInfo(new List<string> { "[Doing action] Note_DoubleClick", "Pulling up data now." });

            bool exists = System.IO.Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(file);
                Debug.WriteLine($"The Folder {file} is created Sucessfully");
            }
            else
            {
                Debug.WriteLine($"The Folder {file} already exists");
            }
            DirectoryInfo dInfo = new DirectoryInfo(file);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
                                                                                    FileSystemRights.Modify, 
                                                                                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, 
                                                                                    PropagationFlags.NoPropagateInherit, 
                                                                                    AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            logger.LogInfo("[Ending action] CreateDirectoryFullAccess");
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            logger.LogInfo(new List<string> { "[Doing action] Note_DoubleClick", "Pulling up data now." });
            //Notes.Add(new Note { Title = "New Note", Text = "Enter your text here." });
            noteManager.AddUpdateNote(new Note { Title = "New Note", Text = "Enter your text here." });
            logger.LogInfo("[Ending action] AddNote_Click");
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            logger.LogInfo(new List<string> { "[Doing action] Note_DoubleClick", "Pulling up data now." });
            if (MyNotesListView.SelectedItem != null)
                //Notes.Remove((Note)MyNotesListView.SelectedItem);
                // Pass title instead of Note object
                // Deciding in to keep the "-" or not.
                noteManager.RemoveNote(((Note)MyNotesListView.SelectedItem).Title);
            logger.LogInfo("[Ending action] DeleteNote_Click");
        }

        private void DeleteNoteFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            logger.LogInfo(new List<string> { "[Doing action] Note_DoubleClick", "Pulling up data now." });
            if (MyNotesListView.SelectedItem != null)
                Notes.Remove((Note)MyNotesListView.SelectedItem);
            logger.LogInfo("[Ending action] DeleteNoteFromContextMenu_Click");
        }

        private void Note_DoubleClick(object sender, RoutedEventArgs e)
        {
            logger.LogInfo(new List<string> { "[Doing action] Note_DoubleClick", "Pulling up data now." });
            if (sender is ListViewItem item && item.DataContext is Note note)
                MessageBox.Show($"Double-clicked on: {note.Title}\n\nText: {note.Text}");
            logger.LogInfo("[Ending action] Note_DoubleClick");
        }

        private void OpenNote_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for opening a note
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            // Logic for saving settings/notes
            // and check if the collection was changed
            // -only on add, remove, not on controls.
            if (isCollectionChanged)
            {
                noteManager.SaveNotes(jotNotesFilePath);
                Debug.WriteLine("Notes collection, changed");
            }
            else
            {
                Debug.WriteLine("Notes collection, NOT changed");
            }

            logger.LogInfo(new List<string> { "ExitApp_Click called.", "Exitign application in progress." });

            Notes.CollectionChanged -= NotesCollectionChangedHandler;
            logger.LogWritten -= LogWrite_Handler;

            Environment.Exit(0);
        }

        //Drag window since windowstyle = none
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void AppSettings_Click(object sender, RoutedEventArgs e)
        {
            //Settings
        }
    }
}
