/*
* Jotter
* C.Winters / US / Arizona / Thinkpad T15g
* Feb 2024
* 
* Remember: If it doesn't work, I didn't write it.
* 
* Purpose 
* A note taking application. Wrote this to "learn" WPF some work places don't support applicaitons 
* like Sticky notes (personal cloud). Ideally, would like to have this /not/ cloud based, unless 
* you desire to move the data file to your own cloud based platform.
*/

using com.nobodynoze.flogger;
using com.nobodynoze.notemanager;
using System.Collections.ObjectModel;
using System.Diagnostics;
//imports for FileStream and XmlSerializer
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Jotter
{
    public partial class MainWindow : Window
    {
        public static string jotNotesFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                              Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.xml"));
        public static Logger logger = new Logger();
        public ObservableCollection<Note> Notes { get; set; }
        public NoteManager? noteManager;
        public Note SelectedNote { get; set; }
        private NoteTemplateEditor noteEditor;

        //Track opened notes. This is so we don't have open redundant open notes. ^_^
        private List<NoteTemplateEditor> openNoteWindows = new List<NoteTemplateEditor>();
        private string SearchValue= "Search...";


        public MainWindow()
        {
            InitializeComponent();

            //Start Jotter at bottom left of screen.
            //We can later add this to settings
            //Center - WindowStartupLocation.CenterScreen
            //Top left: this.Left and this.Top =0
            //Top right: this.Left = SystemParameters.PrimaryScreenWidth - this.Width and this.top =0
            //bottom right: this.Left = SystemParameters.WorkArea.Width - Width and this.Top = SystemParameters.WorkArea.Height - Height
            //We can additionally record the position of window this.Left and this.Top =(x) for some custom posiiton set prior.
            WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = 0;
            this.Top = SystemParameters.WorkArea.Height - Height;

            logger.LogWritten += LogWrite_Handler;
            logger.LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                           Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.log"));
            logger.EnableDTStamps = true;

            //Dir has to be created first before any log written. Access error appears if logging first.
            //We may get rid of this in favor of a customized location. Future TODO?
            CreateDirectoryFullAccess(Path.GetDirectoryName(jotNotesFilePath));

            logger.LogInfo(new List<string> { " ", "------------------------------------------" });
            logger.LogInfo("[Doing action] Started up the generator!");
            logger.LogInfo("[Doing action] Starting logs: " +
                           (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                           Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.log")));

            noteManager = new NoteManager();
            Notes = noteManager.Notes;
            DataContext = noteManager;

            //Initialise the dragging and drop
            InitializeDragDrop();

            MyNotesListView.ItemsSource = Notes;
            MyNotesListView.SelectionChanged += MyNotesListView_SelectionChanged;

            // Attach the Loaded event handler to the Window or TextBox
            //Loaded += MainWindow_Loaded;


        }

        //UI if title is changed and the keypress is ENTER
        //TODO: not implemented, BUT if user closes mainwindow, the data is saved and retained. 
        //private void TitleInMainTextBox_KeyUp(object sender, KeyEventArgs e)
        //{
        //  if (e.Key == Key.Enter || e.Key == Key.Return)
        //    {
        //        // Save changes when Enter or Return key is pressed
        //        //NOT IMPLEMENTED YET
        //        TextBox textBox = sender as TextBox;
        //        if (textBox != null)
        //        {
        //            //Get text in ui
        //            string newTitle = textBox.Text;
        //            noteManager.GetIndexOfSelectedNoteById(??);
        //            noteManager.UpdateNoteByIdIndexer(idIndexer, newTitle);

        //        }

        //    }
        //}


        private void TitleInMainTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    string newTitle = textBox.Text;

                    // Get the index of the selected note
                    Guid? idIndexer = SelectedNote?.IdIndexer;
                    //note may not be selected, then try if (textBox.Tag is Guid idIndexer)...

                    if (idIndexer != null)
                    {
                        //Get the index, compare with selected note
                        int selectedIndex = noteManager.GetIndexOfSelectedNoteById(idIndexer.Value);

                        if (selectedIndex != -1)
                            // Update title
                            noteManager.UpdateNoteByIdIndexer(idIndexer.Value, newTitle);
                        else
                        {
                            //FUTURE - handle if the note cannot be updated. Ignore for now
                        }
                    }
                    else
                    {
                        //User did NOT select the note, only text box for title
                        // Retrieve the IdIndexer from the TextBox DataContext
                        //DAMN this took time to figure out!
                        if (textBox.DataContext is Note note)
                        {
                            // Get the IdIndexer from the Note object
                            Guid curIdIndexer = note.IdIndexer.Value;

                            // Iterate through the dictionary to find the correct Note object
                            //TODO hide the dict - make a function to do something with it. 
                            //This works, but within the mainwindow. 
                            //Discovered this will not work if:
                            //- open the note, change title, close note. data changes -fine.
                            //- change title on main window, press enter. focus doesn't clear and title not updated. 
                            // -data only updated when closing application
                            foreach (KeyValuePair<Guid, Note> kvp in noteManager.idIndexerNoteMap)
                            {
                                if (kvp.Value.IdIndexer == curIdIndexer)
                                {
                                    // Update the title of the Note object
                                    kvp.Value.Title = textBox.Text;
                                    noteManager.UpdateNoteByIdIndexer(curIdIndexer, newTitle);
                                    noteManager.SaveNotes(jotNotesFilePath);
                                    //textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                                    this.Focus();
                                    //the event has been handled
                                    e.Handled = true;
                                    break; 
                                }
                            }
                        }

                    }
                }
            }
        }



        /// <summary>
        /// During the debugging of application, show the logging in the compiler output window. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void LogWrite_Handler(object source, LogEventArgs e)
        {
            Debug.WriteLine($"Log: {e.LogMessage}");
        }

        /// <summary>
        /// Create directory with full access
        /// </summary>
        /// <param name="file"></param>
        private void CreateDirectoryFullAccess(string file)
        {
            //logger.LogInfo(new List<string> { "[Doing action] CreateDirectoryFullAccess", "Verifying save location" });
            //Can't write into destination directory if first time!

            bool exists = Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = Directory.CreateDirectory(file);
                Debug.WriteLine($"The Folder {file} is created Successfully");
                logger.LogInfo(new List<string> { $"- {file} verified as created" });
            }
            else
                logger.LogInfo(new List<string> { $"- {file} verified" });

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

        private void MyNotesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyNotesListView.SelectedItem != null) SelectedNote = (Note)MyNotesListView.SelectedItem;
        }

        //note updated handler. take out older code. will trace later.
        //this was needed for sarerch
        private void NoteEditor_NoteUpdated(object sender, NoteEventArgs e)
        {
            Note? test = e.UpdatedNote;
            logger.LogInfo("[TEST - NoteEditor_NoteUpdated] " + test.Title);

           // Handle NoteUpdated event here accessing the updated note via e.UpdatedNote (event arg)
            Note updatedNote = e.UpdatedNote;

            // Log for testing
            logger.LogInfo("[NoteEditor_NoteUpdated] Updated Note Title: " + updatedNote.Title);

            foreach (var note in Notes)
            {
                if (note.IdIndexer == updatedNote.IdIndexer)
                {
                    note.Title = updatedNote.Title;
                    note.Text = updatedNote.Text;

                    // Optionally refresh the ListView UI here if necessary
                    UpdateListView();

                    // Exit the loop once the note is found and updated
                    break;
                }
            }
        }

        /// <summary>
        /// Add a new note to NoteManager, the listview, and open the NoteTemplateEditor to edit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            // Create a new note
            Note newNote = new Note { IdIndexer = noteManager.GenerateID() , Title = "A new note", Text = "Enter a new note here!" };

            // Add the new note to the note manager
            noteManager?.AddUpdateNote(newNote);

            // Open the note editor window for editing the new note
            NoteTemplateEditor noteEditor = new NoteTemplateEditor(newNote);
            noteEditor.NoteUpdated += NoteEditor_NoteUpdated;
            noteEditor.Show(); 

            // Refresh the list view after the child window is closed
            UpdateListView();
        }

        //Remove a note from the MyNotesListView listview via "-" button
        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            //if (MyNotesListView.SelectedItem != null)
            //{
            //    noteManager.RemoveNote(((Note)MyNotesListView.SelectedItem).Title);
            //    UpdateListView();
            //}
            DeleteSelectedNote();
        }

        //Remove a note from the MyNotesListView listview via context menu
        private void DeleteNoteFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            //if (MyNotesListView.SelectedItem != null)
            //    Notes.Remove((Note)MyNotesListView.SelectedItem);
            DeleteSelectedNote();
        }

        /// <summary>
        /// Update the Listview on the main window
        /// </summary>
        private void UpdateListView()
        {
            MyNotesListView.Items.Refresh();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            // Save the notes before exiting the application
            noteManager?.SaveNotes(jotNotesFilePath);

            //Iter and close all open windows.
            // Add all opened child windows to the lists of windows to close
            // The list will contain window data.
            // Created "windowsToClose" (separate list) to store the child windows
            // that need to be closed -- then close them outside the foreach loop.
            // Previously, used a single list "chilDwindow" but the for loop modifies it,
            // thus getting exception System.InvalidOperationException:
            // 'Collection was modified; enumeration operation may not execute.'
            List<NoteTemplateEditor> windowsToClose = new List<NoteTemplateEditor>();

            foreach (NoteTemplateEditor? childWindow in openNoteWindows)
            {
                windowsToClose.Add(childWindow);
            }

            // Close all windows from the separate list
            foreach (NoteTemplateEditor? windowToClose in windowsToClose)
            {
                windowToClose.Close();
            }

            //Get rid of our subscriptions
            MyNotesListView.SelectionChanged -= MyNotesListView_SelectionChanged;
            logger.LogWritten -= LogWrite_Handler;

            Environment.Exit(0);
        }

        private void AppSettings_Click(object sender, RoutedEventArgs e)
        {
            // Open AppSettings window or handle app settings here
            //NOT YET IMPLEMENTED
            TransitionToSettings();
        }


        private void OpenSelectedNote()
        {
            if (MyNotesListView.SelectedItem is Note note)
            {
                bool noteIsOpen = false;

                // Is note already open via its IdIndexer?
                foreach (var window in openNoteWindows)
                {
                    if (window.DataContext is Note openedNote && openedNote.IdIndexer == note.IdIndexer)
                    {
                        noteIsOpen = true;
                        // a'right, then go back to the opened note
                        window.Focus();
                        break;
                    }
                }

                if (!noteIsOpen)
                {
                    //IF it's not open, then go ahead and open it
                    NoteTemplateEditor noteEditor = new NoteTemplateEditor(note);
                    //Subscribe to event to handle clean up when the note is closed.
                    noteEditor.Closed += NoteEditor_Closed;
                    //ADDED
                    noteEditor.NoteUpdated += NoteEditor_NoteUpdated;
                    noteEditor.Show();

                    // Add the note editor window to the tracking list
                    openNoteWindows.Add(noteEditor);
                }
            }
        }



        // Cleanup method when a note window is closed
        // Used by OpenSelectedNote() as a subscriber
        private void NoteEditor_Closed(object sender, EventArgs e)
        {
            if (sender is NoteTemplateEditor closedEditor)
            {
                // Remove the closed window from the tracking list
                openNoteWindows.Remove(closedEditor);
            }
        }

        /// <summary>
        /// Delete a note from the NoteManager, via selected item from the MyNoteListView
        /// </summary>
        private void DeleteSelectedNote()
        {
            if (MyNotesListView.SelectedItem != null)
            {
                noteManager.RemoveNote(((Note)MyNotesListView.SelectedItem).Title);
                UpdateListView();
            }
        }

        //Open a note via "+" button
        private void OpenNote_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedNote();
        }

        //Open a note via double-clicking on note
        private void Note_DoubleClick(object sender, RoutedEventArgs e)
        {
            //if (sender is ListViewItem item && item.DataContext is Note note)
            //{
            //    NoteTemplateEditor noteEditor = new NoteTemplateEditor(note);
            //    noteEditor.Show();
            //}
            OpenSelectedNote();
        }

        //Holding left clikc button and moving the mouse will move the window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        //Once we click elsewhere, reset the search box watermark, or html "placeholder"
        private void NoteManagerSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoteManagerSearch.Text) || NoteManagerSearch.Text == "Search...")
            {
                NoteManagerSearch.Text = "Search...";

                // Reset to show all ntoes
                MyNotesListView.ItemsSource = Notes;
            }
        }

        //clear the tesxtbox watermark so we can search.
        private void NoteManagerSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Search...")
            {
                textBox.Text = string.Empty;
            }

            //if (NoteManagerSearch.Text == "Search...")
            //{
            //    NoteManagerSearch.Text = string.Empty;
            //}
        }

        //Begin the search after pressing enter.
        private void NoteManagerSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    // Convert to lowercase for cheap-ass case-insensitive search
                    string searchText = textBox.Text.Trim().ToLower(); 
                    PerformSearch(NoteManagerSearch.Text);
                }
            }
            else if (NoteManagerSearch.Text.Length == 0 && e.Key == Key.Back || e.Key == Key.Delete)
            {
                // Clear the search when the textbox is empty and the user presses Backspace
                ClearSearch();
            }
        }

        private void PerformSearch(string searchText)
        {
                // Create a temp collection to hold filtered notes
                ObservableCollection<Note> filteredNotes = new ObservableCollection<Note>();

                foreach (Note note in noteManager.Notes)
                {
                    if (note.Title.ToLower().Contains(searchText) || note.Text.ToLower().Contains(searchText))
                        filteredNotes.Add(note); 
                }

                // Update the UI with filtered notes
                UpdateUIWithFilteredNotes(filteredNotes, searchText);
        }

        private void ClearSearch()
        {
            //NoteManagerSearch.Text = "Search...";
            //show all notes
            MyNotesListView.ItemsSource = Notes;
        }


        private void UpdateUIWithFilteredNotes(ObservableCollection<Note> notes, string searchText)
        {
            //MyNotesListView.Items.Clear();
            // System.InvalidOperationException: 'Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.'

            //// Add the filtered notes to the UI
            //foreach (Note note in filteredNotes)
            //{
            //    MyNotesListView.Items.Add(note);
            //}

            ObservableCollection<Note> filteredNotes = new ObservableCollection<Note>();

            foreach (Note note in noteManager.Notes)
            {
                if (note.Title.ToLower().Contains(searchText) || note.Text.ToLower().Contains(searchText))
                {
                    filteredNotes.Add(note); // Add the note to the filtered collection
                }
            }

            // Update the bound collection with the filtered notes
            MyNotesListView.ItemsSource = filteredNotes;
        }

        // Drag/Drop handling
        /*
           (Comments for education... WPF handles this a bit differently than WinForms):

            ----
            Summary of Drag/Drop events to add a note from a text file.
            ----

            InitializeDragDrop() 
            Helper func sets up the "drag and drop" functionality by attaching event handlers to the necessary events.
            
            MainWindow_DragEnter() / MainWindow_DragOver() funcs 
            Handle the drag enter and drag over events to allow dropping files. Text files are in mind, and there may be 
            side effects on others. Will handle those later.
            
            MainWindow_Drop() func 
            Processes dropped files, reads the text content, and adds a new note to the NoteManager.
            
            AddNewNoteWithText() helper func 
            Creates a new note object with new content and adds it to the notes collection.

            ----    
            Handle Drag Events 
            ----

            - In the DragEnter event, check if the data being dragged is of type DataFormats.FileDrop (which indicates a file).
            - In the DragOver event, set the Effects property to DragDropEffects.Copy to indicate that the drop action is allowed.
            - In the Drop event, extract the file path from the dropped data and read the contents of the text file.

            And, process the file and its contents:

            - Get filename and full path, read the contents.
            - Create new note object with the file contents.
            - Add new note object to the NoteManager (collection of notes) and update UI.
            - .TXT files are supported at the moment for this initial part of drag/drop

            ----
            UI
            ----
            No UI components need property adjustments. Mostly subscriptions and events handling.

         */
        private void InitializeDragDrop()
        {
            AllowDrop = true;
            DragEnter += MainWindow_DragEnter;
            DragOver += MainWindow_DragOver;
            Drop += MainWindow_Drop;
        }

        /// <summary>
        /// Event trigger when a file is dragged INTO (or entering) the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// TODO: Using e.Data.GetData(DataFormats.FileDrop) determine if it's a valid file. If not, then don't change the mouse cursor
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Event trigger when a file is dragged OVER the main window. The DragEvent arguments tag this as a copy-from the source (file)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        /// <summary>
        /// The event trigger when a file is dropped onto the main window to add a new note from a text or html file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] arFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string fileItem in arFiles)
            {
                //REMmed for the moment. We should test other files, but the foramts won't be supported
                //For this initial code, let's do a cheap check for binary files.
                //if (File.Exists(fileItem) && Path.GetExtension(fileItem).Equals(".txt", StringComparison.OrdinalIgnoreCase))

                //if ( File.Exists(fileItem) && 
                //     Path.GetExtension(fileItem).Equals(".txt", StringComparison.OrdinalIgnoreCase)
                //   )
                if (File.Exists(fileItem) )
                {
                    bool isBinary = IsBinaryFile(fileItem);
                    string? newNoteTitle = Path.GetFileNameWithoutExtension(fileItem);
                    string? convertedText = string.Empty;

                    if (!isBinary)
                    {
                        if (IsHtmlFile(fileItem))
                        {
                            convertedText = ConvertHTMLToText(fileItem);
                            
                            if (convertedText != string.Empty)     
                                AddNewNoteWithText(newNoteTitle, convertedText);
                        }
                        else
                        {
                            convertedText = File.ReadAllText(fileItem);
                            AddNewNoteWithText(newNoteTitle, convertedText);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cheap way to check for binary file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true if binary file detect, false otherwise</returns>
        private bool IsBinaryFile(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                for (int i = 0; i < fileStream.Length; i++)
                {
                    int byteValue = fileStream.ReadByte();
                    //Is null char?
                    if (byteValue == 0) 
                        //Yep, ok, bin file!
                        return true; 
                }
            }
            return (false); 
        }

        /// <summary>
        /// Check if the file source is a valid HTML file (by checking basic contents)
        /// </summary>
        /// <param name="filePath">file source to process for valid content</param>
        /// <returns>true - if a "valid" HTML file</returns>
        private bool IsHtmlFile(string filePath)
        {
            logger.LogInfo($"Doing action: [IsHtmlFile]");

            string extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".html" || extension == ".htm")
                return true;

            //Try peeking into the file, reading a few lines. Look for standard HTML stuff
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string content = reader.ReadLine() + reader.ReadLine(); 
                    
                    //Add more checks here to check for valid HTML file... 
                    if (content != null &&
                       (content.Contains("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase) ||
                        content.Contains("<html>", StringComparison.OrdinalIgnoreCase)))
                        return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"(IsHtmlFile) Error processing file: {ex.Message}");
            }
            
            logger.LogInfo($"Ending action: [IsHtmlFile] {filePath} Not a valid HTML");
            return (false); 
        }

        /// <summary>
        ///  Read the HTML content and remove HTML tags
        /// </summary>
        /// <param name="htmlFilePath">string - content of text to convert</param>
        /// <returns>string - results from conversion</returns>
        /// NOTES: I'm new to WebUtility only finding a basic result. Will improve if it needs
        public static string ConvertHTMLToText(string htmlFilePath)
        {
            try
            {
                //Read the HTML content remove HTML tags (example: &quot; -> ")
                string? htmlContent = File.ReadAllText(htmlFilePath);
                string? plainText = Regex.Replace(htmlContent, "<.*?>", string.Empty);
                plainText = WebUtility.HtmlDecode(plainText);
                
                if (plainText != null) 
                    return (plainText);
            }
            catch (Exception ex)
            {
                logger.LogError($"(ConvertHTMLToText) Error processing file: {ex.Message}");
                return ($"Error processing file: {ex.Message}");
            }
            return (null);
        }


        /// <summary>
        ///  Create new note from the dropped file
        /// </summary>
        /// <param name="title">string to title of a new note</param>
        /// <param name="text">string to content of note</param>
        private void AddNewNoteWithText(string title, string text)
        {
            //- Create a new note with contents
            //- Add the new note object to note manager
            //- Save the notes in the note manager
            //- Update list view.
            Note newNote = new Note { IdIndexer = noteManager.GenerateID(), 
                                      Title = title, 
                                      Text = text 
                                    };

            Notes.Add(newNote);
            noteManager.SaveNotes(jotNotesFilePath);

            UpdateListView();
        }
        // End rag and drop handling

        //Cheap transition to settings screen and back to main
        //Found this under the VS2022 "see examples". Did not know about this
        //little gem!
        //This may end up being changed.
        private async void TransitionToSettings()
        {
            // Save the current positions of MainWindow
            double mainLeft = Application.Current.MainWindow.Left;
            double mainTop = Application.Current.MainWindow.Top;

            // Fade out MainWindow
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            this.BeginAnimation(Window.OpacityProperty, fadeOutAnimation);

            // Wait for the fade-out transition animation to complete
            await Task.Delay(TimeSpan.FromSeconds(0.2));

            this.Hide();

            // Loaded up the settings!
            Settings settingsWindow = new Settings();
            //settingsWindow.Left = Application.Current.MainWindow.Left;
            //settingsWindow.Top = Application.Current.MainWindow.Top;
            settingsWindow.Left = mainLeft; 
            settingsWindow.Top = mainTop;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            settingsWindow.Closed += SettingsWindow_Closed;

            settingsWindow.ShowDialog();
            //Application.Current.MainWindow.Left = Window.GetWindow(this).Left;
            //Application.Current.MainWindow.Top = Window.GetWindow(this).Top;


            // Show MainWindow again and fade in
            this.Opacity = 0;
            this.Show();

            DoubleAnimation fadeInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(Window.OpacityProperty, fadeInAnimation);
            
            // Restore MainWindow's position
            //Application.Current.MainWindow.Left = mainLeft;
            //Application.Current.MainWindow.Top = mainTop;
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Update the position of MainWindow based on the closed Settings window
                if (sender is Settings settingsWindow)
                {
                    mainWindow.Left = settingsWindow.Left;
                    mainWindow.Top = settingsWindow.Top;
                }
            }
        }

    }
}
