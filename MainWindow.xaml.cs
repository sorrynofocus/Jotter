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
* 
* COMPILE BUG: Sometimes you'll get a compile error saying DragEnter, DragOver, and Drop are  
* ambiguous. This is because System.Windows.Forms needs to be removed. 
* 
* 
* Map
* Guid? idIndexer = SelectedNote?.IdIndexer - selected note id
*/

using com.nobodynoze.flogger;
using com.nobodynoze.notemanager;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;

//imports for FileStream and XmlSerializer
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


//using System.Windows.Forms;
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
        public readonly string UI_TEXTBOX_STR_SEARCH = "Search...";

        //Settings Manager
        private SettingsMgr settingsManager;

        private System.Windows.Forms.NotifyIcon notifyIcon;
        

        public MainWindow()
        {
            InitializeComponent();
            
            // Init settings manager
            settingsManager = new SettingsMgr();

            //TODO VV temp function to TEST the settings manager prototype!!!
            //LoadAppSettings(settingsManager);

            //Start Jotter at bottom left of screen.
            //We can later add this to settings
            //Center - WindowStartupLocation.CenterScreen
            //Top left: this.Left and this.Top =0
            //Top right: this.Left = SystemParameters.PrimaryScreenWidth - this.Width and this.top =0
            //bottom right: this.Left = SystemParameters.WorkArea.Width - Width and this.Top = SystemParameters.WorkArea.Height - Height
            //We can additionally record the position of window this.Left and this.Top =(x) for some custom posiiton set prior.
            
            //WindowStartupLocation = WindowStartupLocation.Manual;
            //this.Left = 0;
            //this.Top = SystemParameters.WorkArea.Height - Height;
            //ABOVE COMMENTED OUT TO TEST WINDOW POS
            LoadAppSettings(settingsManager.Settings);

            logger.LogWritten += LogWrite_Handler;
            
            //Get log path custom or not
            if ((settingsManager.Settings.LogPath != string.Empty) || (settingsManager.Settings.LogPath != null))
                logger.LogFile = settingsManager.Settings.LogPath;
            else
            {
                logger.LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.log"));
                //Turn logging off if no path..
                //logger.EnableLogger = false;
                //TODO: Add code in log handler to check if log file is empty, then turn off logging.   
            }

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

            logger.LogInfo($"[mainwindow load] Loaded notes count: {Notes?.Count}");

            DataContext = noteManager;

            //Init the dragging and drop
            InitializeDragDrop();

            MyNotesListView.ItemsSource = Notes;
            MyNotesListView.SelectionChanged += MyNotesListView_SelectionChanged;

            // Subscribe to the Loaded event
            this.Loaded += MainWindow_Loaded;



            //Init NOTIFY TRAY ICON
            InitializeTrayIcon();

            // Subscribe to StateChanged event
            this.StateChanged += MainWindow_StateChanged;


        }


        /// <summary>
        /// Switch Themes
        /// </summary>
        /// <param name="themeName">LightTheme, DarkTheme, DefaultTheme.</param>
        /// TODO :This function is REDUNDANT! Put into a utils file as this is essentially the 
        /// same as the one in settings.xaml.cs
        public void SwitchTheme(string themeName)
        {
            try
            {
                var resourceUri = new Uri(@$"/Utils/Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute);
                var resourceDictionary = new ResourceDictionary() { Source = resourceUri };

                var existingDictionaries = Application.Current.Resources.MergedDictionaries.ToList();
                foreach (var dictionary in existingDictionaries)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(dictionary);
                }

                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

                Debug.WriteLine($"Theme applied successfully: {themeName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply theme: {themeName}. Error: {ex.Message}");
                MessageBox.Show($"The theme \"{themeName}\" could not be applied. Falling back to the default theme.", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Apply default theme as fallback
                try
                {
                    var defaultUri = new Uri("/Utils/Themes/DefaultTheme.xaml", UriKind.RelativeOrAbsolute);
                    var defaultDictionary = new ResourceDictionary() { Source = defaultUri };

                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(defaultDictionary);

                    Debug.WriteLine("Default theme applied as fallback.");
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"Failed to apply default theme. Error: {fallbackEx.Message}");
                }
            }
        }

        /// <summary>
        /// Load the application settings from global config
        /// </summary>
        /// <param name="settings">AppSettings config obj </param>
        private void LoadAppSettings(AppSettings settings)
        {
            double screenLeft = SystemParameters.WorkArea.Left;
            double screenTop = SystemParameters.WorkArea.Top;
            double screenRight = SystemParameters.WorkArea.Right;
            double screenBottom = SystemParameters.WorkArea.Bottom;

            // Ensure the window's position is within the screen bounds -no invalid pos.
            this.Left = Math.Max(screenLeft, Math.Min(settings.WindowLeft, screenRight - this.Width));
            this.Top = Math.Max(screenTop, Math.Min(settings.WindowTop, screenBottom - this.Height));
            this.Width = settings.WindowWidth;
            this.Height = settings.WindowHeight;

            Debug.WriteLine($"WindowLeft: {settings.WindowLeft}, WindowTop: {settings.WindowTop}");

            //Restore maximized state if applicable
            if (settings.IsMaximized)
                this.WindowState = WindowState.Maximized;


            //Themes!
            string selectedTheme = settings.Theme;  

            switch (selectedTheme)
            {
                case "Light Theme":
                    SwitchTheme("LightTheme");
                    settingsManager.Settings.Theme = selectedTheme;
                    settingsManager.SaveSettings();
                    break;
                case "Dark Theme":
                    SwitchTheme("DarkTheme");
                    settingsManager.Settings.Theme = selectedTheme;
                    settingsManager.SaveSettings();
                    break;
                case "Default Theme":
                    SwitchTheme("DefaultTheme");
                    settingsManager.Settings.Theme = selectedTheme;
                    settingsManager.SaveSettings();
                    break;
                case "Custom Theme":
                    SwitchTheme("CustomTheme");
                    settingsManager.Settings.Theme = selectedTheme;
                    settingsManager.SaveSettings();
                    break;
            }

            //...
        }

        /// <summary>
        /// Save the application settings to global config
        /// </summary>
        private void SaveAppSettings()
        {
            AppSettings settings = settingsManager.Settings;

            // Save window position and size
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;
            settings.WindowLeft = this.Left;
            settings.WindowTop = this.Top;

            // Save window state (is maximized)
            settings.IsMaximized = this.WindowState == WindowState.Maximized;

            settingsManager.SaveSettings();
        }


        /// <summary>
        /// SAve note settings on gobal config. OpenSelectedNote() uses this in mainwindow
        /// </summary>
        /// <param name="noteWindow"></param>
        /// <param name="noteSettings"></param>
        private void LoadAppNoteSettings(Window noteWindow, NoteSettings noteSettings)
        {
            //MAKE SURE that the XAML for this is set to:
            // WindowStartupLocation="Manual"
            // Took awhile to figure out I had WindowStartupLocation="Center" forcing it to 
            // center on the screen. Faaah!
            double screenLeft = SystemParameters.WorkArea.Left;
            double screenTop = SystemParameters.WorkArea.Top;
            double screenRight = SystemParameters.WorkArea.Right;
            double screenBottom = SystemParameters.WorkArea.Bottom;

            //The order of positioning is essential because the position (Left and Top) depends on the dimensions.
            noteWindow.Width = noteSettings.NoteDim.Width;
            noteWindow.Height = noteSettings.NoteDim.Height;

            // Apply POS, ensuring it's within bounds
            noteWindow.Left = Math.Max(screenLeft, Math.Min(noteSettings.NotePos.Left, screenRight - noteWindow.Width));
            noteWindow.Top = Math.Max(screenTop, Math.Min(noteSettings.NotePos.Top, screenBottom - noteWindow.Height));


            Debug.WriteLine($"NoteWindowLeft: {noteSettings.NotePos.Left}, NoteWindowTop: {noteSettings.NotePos.Top}");
            Debug.WriteLine($"NoteWindowWidth: {noteSettings.NoteDim.Width}, NoteWindowHeight: {noteSettings.NoteDim.Height}");
        }


        /// <summary>
        /// Event for the main window loaded and in view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Sort notes by creation date and refresh the MyNotesListView ListView
            SortNotesList();
        }

        /// <summary>
        /// Event for updating the title of a note in the main window   
        /// </summary>
        /// <param name="sender">reference to the ui obj who sent us</param>
        /// <param name="e">The key presse invoked</param>
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
                                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
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

        /// <summary>
        /// Event for selecting a note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyNotesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyNotesListView.SelectedItem != null) SelectedNote = (Note)MyNotesListView.SelectedItem;
            logger.LogInfo($"[selectionchanged] {SelectedNote.Title} selected");
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
                    SortNotesList();

                    // Exit the loop once the note is found and updated
                    break;
                }
            }
        }

        /// <summary>
        /// Add a new note to NoteManager, the MyNotesListView listview, and 
        /// open the NoteTemplateEditor to edit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            // Create a new note
            Note newNote = new Note 
            { 
                IdIndexer = noteManager.GenerateID(), 
                Title = "A new note", 
                Text = "Enter a new note here!",
                CreatedDate = DateTime.Now 
            };

            // Add the new note to the note manager
            noteManager?.AddUpdateNote(newNote);

            // Open the note editor window for editing the new note
            NoteTemplateEditor noteEditor = new NoteTemplateEditor(newNote);
            noteEditor.NoteUpdated += NoteEditor_NoteUpdated;
            noteEditor.Show(); 

            // Refresh the list view after the child window is closed
            UpdateListView();
            SortNotesList();
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
            //TODO, move and test SortNotesList() to this
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            if (settingsManager.Settings.IsTray)
            {
                // Hide MainWindow instead of exiting
                this.Hide();
                NotifyIcon.Visibility = Visibility.Visible;
            }
            else
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

                //Save the current mainwindow settings before exit
                SaveAppSettings();

                Environment.Exit(0);

            }

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
                    if (window.DataContext is Note openedNote && 
                        openedNote.IdIndexer == note.IdIndexer)
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


                    //Get the configuration of the the selected note
                    //If there no confgi, then default it to the right of the application
                    
                    //get new "child" window (NoteTemplateEditor) pos relative to the
                    //main window
                    
                    //Spacing between windows
                    double offset = 60; 
                    //defaulting to the right of the mainwindow
                    double childLeft = this.Left + this.Width + offset; 
                    double childTop = this.Top;

                    //If the note (NoteTEmplateEditor) has invalid screen coords, pos it to the left
                    if (childLeft + noteEditor.Width > SystemParameters.WorkArea.Right)
                        childLeft = this.Left - noteEditor.Width - offset;

                    NoteSettings noteSettings = settingsManager.GetNoteConf(note.IdIndexer.Value);
                    LoadAppNoteSettings(noteEditor, noteSettings);
                    //END of note configuration pos and dim


                    //Subscribe to event to handle clean up when the note is closed.
                    noteEditor.Closed += NoteEditor_Closed;

                    //Can you have anon noteEditor.Closed += (s, e) => ? 
                    
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
            if (sender is NoteTemplateEditor closedEditor && closedEditor.DataContext is Note closedNote)
            {
                // Save the position and dimensions of the closed note
                settingsManager.SaveNoteConf(new NoteSettings
                {
                    noteID = closedNote.IdIndexer.Value,
                    NoteDim = new Dimension
                    {
                        Width = closedEditor.Width,
                        Height = closedEditor.Height
                    },
                    NotePos = new Position
                    {
                        Left = closedEditor.Left,
                        Top = closedEditor.Top
                    }
                });

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
                // Remove note settings
                //settingsManager.RemoveNoteSettings(noteManager.  note.IdIndexer.Value);
                Guid? idIndexer = SelectedNote?.IdIndexer;

                if (idIndexer != null)
                {
                    //Remove the note settings (POS and DIM) if they exist. 
                    settingsManager.RemoveNoteConf(idIndexer.Value);

                    //TODO: remove note by ID. Not by title
                    //Create func: RemoveNoteByIdIndex()
                    //noteManager.RemoveNoteByIdIndex(idIndexer.Value);
                }

                //TODO: remove note by ID. Not by title. See above
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
            if (string.IsNullOrWhiteSpace(NoteManagerSearch.Text) 
                || NoteManagerSearch.Text == UI_TEXTBOX_STR_SEARCH)
            {
                NoteManagerSearch.Text = UI_TEXTBOX_STR_SEARCH;

                // Reset to show all ntoes
                MyNotesListView.ItemsSource = Notes;
            }
        }

        //clear the tesxtbox watermark so we can search.
        private void NoteManagerSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == UI_TEXTBOX_STR_SEARCH)
            {
                textBox.Text = string.Empty;
            }
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
                    // if (note.Title.ToLower().Contains(searchText) || note.Text.ToLower().Contains(searchText))
                    if ((note.Title != null && noteManager.SearchContext(note.Title, searchText)) || (note.Text != null && noteManager.SearchContext(note.Text, searchText)))
                        filteredNotes.Add(note);
                }

                // Update the bound collection with the filtered notes
                MyNotesListView.ItemsSource = filteredNotes;
        }

        private void ClearSearch()
        {
            //show all notes
            MyNotesListView.ItemsSource = Notes;
        }

        /// <summary>
        /// Sort notes by date creation
        /// </summary>
        /// Note:
        /// Call RefreshNotesList whenever:
        ///  - A new note is added.
        ///  - Notes are loaded.
        ///  - Notes are updated.
        public void SortNotesList()
        {
            if (Notes == null) return;

            // Sort by CreatedDate in descending order (newest first, the seconadry by alpha)
            List<Note>? sortedNotes = Notes
                .OrderByDescending(note => note.CreatedDate) 
                .ThenBy(note => note.Title)
                .ToList();

            //Notes.Clear();
            //foreach (var note in sortedNotes)
            //{
            //    Notes.Add(note);
            //}

            // Reorder ObservableCollection using Move
            for (int i = 0; i < sortedNotes.Count; i++)
            {
                var currentNote = sortedNotes[i];
                var currentIndex = Notes.IndexOf(currentNote);

                if (currentIndex != i)
                {
                    Notes.Move(currentIndex, i);
                }
            }

            logger.LogInfo("[SortNotesList] After sorting:");
            foreach (var note in Notes)
            {
                logger.LogInfo($"Note: {note.Title}, CreatedDate: {note.CreatedDate}");
            }
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
        /// TODO: Using e.Data.GetData(DataFormats.FileDrop) determine if it's a valid file. 
        /// If not, then don't change the mouse cursor
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
            logger.LogInfo($"Doing action: [MainWindow_Drop]");
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
                        logger.LogInfo($"[MainWindow_Drop] - resorting...");
                        SortNotesList();
                    }
                }
            }
            logger.LogInfo($"Ending action: [MainWindow_Drop]");
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
                        return (true); 
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
                return (true);

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
                        return (true);
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
                                      Text = text ,
                                      CreatedDate = DateTime.Now
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
            Settings settingsWindow = new Settings(settingsManager);
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

        /// <summary>
        /// Event for the Settings window closed - tries to update mainwindow to restore its location    
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Update the position of MainWindow based on the closed
                // Settings window
                if (sender is Settings settingsWindow)
                {
                    mainWindow.Left = settingsWindow.Left;
                    mainWindow.Top = settingsWindow.Top;
                }
            }
        }


        #region NOTIFY TRAY ICON OPERATIONS
        private void InitializeTrayIcon()
        {
            NotifyIcon.Visibility = Visibility.Visible;

            if (settingsManager.Settings.IsTray)
                this.StateChanged += MainWindow_StateChanged;
            else
                NotifyIcon.Visibility = Visibility.Hidden;

            NotifyIcon.TrayMouseDoubleClick += NotifyIcon_MouseDoubleClick;

            //INstead assigning to event, we can use lambda to do the same thing 
            //with sender and event args passed in.
            //NotifyIcon.TrayMouseDoubleClick += (s, e) => ShowMainWindow();
            // 
            // private void ShowMainWindow()
            // {
            //     this.Show();
            //     this.WindowState = WindowState.Normal;
            // }
        }


        private void ShowApplication_Click(object sender, RoutedEventArgs e)
        {
            NotifyIcon_MouseDoubleClick(sender, e);
        }

        // Close the application via notify tray 
        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            SaveAppSettings();
            Application.Current.Shutdown();
        }

        //Decide if the notification tray icon should show and how the applicaiton acts
        //when the main window is minimized.    
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && settingsManager.Settings.IsTray)
            {
                this.Hide();
                NotifyIcon.Visibility = Visibility.Visible;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Visible)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
                NotifyIcon.Visibility = Visibility.Hidden;
            }
        }
        #endregion NOTIFY TRAY ICON OPERATIONS

    } //End of MainWindow class
} // End of namespace
