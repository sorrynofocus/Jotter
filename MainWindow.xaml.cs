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
 * 
 */


namespace Jotter
{
    public partial class MainWindow : Window
    {
        // Data file here: C:\Users\<USER>\AppData\Local\Jotter\JotterNotes.xml
        public static string jotNotesFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                                 Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.xml") );

        /*
         * changes to the Title and Text properties of a Note object will raise the PropertyChanged event. You can then subscribe to this event for each Note object in the ObservableCollection<Note> and update the isCollectionChanged flag accordingly whenever a property changes.
         */
        public class Note: INotifyPropertyChanged
        {
            private string? title;
            private string? text;

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

            public event PropertyChangedEventHandler? PropertyChanged;

            //TODO: To capture we need to write Notes_PropertyChanged() nad subscribe to the properties. 
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



        public MainWindow()
        {
            //InitializeComponent();
            //Notes = new ObservableCollection<Note>();
            //noteManager = new NoteManager();
            //DataContext = this;
            System.Diagnostics.Debug.WriteLine(jotNotesFilePath);

            //Create directory if it does not exist for our stored data
            // Create the directory
            //string directoryPath = Path.GetDirectoryName(jotNotesFilePath);
            //System.IO.Directory.CreateDirectory(directoryPath);
            CreateDirectoryFullAccess(Path.GetDirectoryName(jotNotesFilePath));



            InitializeComponent();
            noteManager = new NoteManager();
            Notes = noteManager.Notes; // Bind to the Notes collection of NoteManager
                                       // Subscribe to the CollectionChanged event
            
            //Subscribe to the Notes collection change to a handler.
            //TODO Notes properties handler for changes not implemented yet.
            Notes.CollectionChanged += NotesCollectionChangedHandler;
            DataContext = this;
        }

        public class NoteManager
        {
           
            public ObservableCollection<Note>? Notes { get; set; }
            private XmlSerializer? serializer;


            //public NoteManager()
            //{
            //    Notes = LoadNotes(serializer);
            //}
            //Instead of use block body, use expression body for constructor
            public NoteManager() => Notes = LoadNotes(serializer, jotNotesFilePath);

            public void AddOrUpdate(Note note)
            {
                Note? existingNote = Notes.FirstOrDefault(n => n.Title == note.Title);
                if (existingNote != null)
                {
                    existingNote.Text = note.Text;
                }
                else
                {
                    Notes.Add(note);
                }

                SaveNotes(jotNotesFilePath);
            }

            public void Remove(string title)
            {
                Note? noteToRemove = Notes.FirstOrDefault(n => n.Title == title);
                if (noteToRemove != null)
                {
                    Notes.Remove(noteToRemove);
                    SaveNotes(jotNotesFilePath);
                }
            }

            private ObservableCollection<Note>? LoadNotes(XmlSerializer serializer, string? filePath)
            {
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
            }
        }


        private static void CreateDirectoryFullAccess(string file)
        {
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
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            //Notes.Add(new Note { Title = "New Note", Text = "Enter your text here." });
            noteManager.AddOrUpdate(new Note { Title = "New Note", Text = "Enter your text here." });
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (MyNotesListView.SelectedItem != null)
                //Notes.Remove((Note)MyNotesListView.SelectedItem);
                noteManager.Remove(((Note)MyNotesListView.SelectedItem).Title); // Pass title instead of Note object
        }

        private void DeleteNoteFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MyNotesListView.SelectedItem != null)
                Notes.Remove((Note)MyNotesListView.SelectedItem);
        }

        private void Note_DoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is Note note)
                MessageBox.Show($"Double-clicked on: {note.Title}\n\nText: {note.Text}");
        }

        private void OpenNote_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for opening a note
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for saving settings/notes
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

            Notes.CollectionChanged -= NotesCollectionChangedHandler;
            Environment.Exit(0);
        }

        //Coee behind for dragging window since windowstyle = none
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }



    }
}
