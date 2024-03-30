/*
* Jotter
* C.Winters / US / Arizona / Thinkpad T15g
* Feb 2024
 * 
 * Remember: If it doesn't work, I didn't write it.
 * 
 * Purpose 
 * A note taking application. Wrote this to learn WPF and to use at work because work will not allow applicaitons like Sticky notes.
 * Ideally, would like to have this /not/ cloud based, unless you desire to move the save file to your own cloud based platform.
 */

using com.nobodynoze.flogger;
using com.nobodynoze.notemanager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
//imports for FileStream and XmlSerializer
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Jotter.MainWindow;

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


        public MainWindow()
        {
            InitializeComponent();

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

            MyNotesListView.ItemsSource = Notes;
            MyNotesListView.SelectionChanged += MyNotesListView_SelectionChanged;
        }

        //UI if title is changed and the keypress is ENTER
        //TODO: not implemented, BUT if user closes mainwindow, the data is saved and retained. 
        private void TitleInMainTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                // Save changes when Enter or Return key is pressed
                //NOT IMPLEMENTED YET
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

        //here for testing. May remove later.
        private void NoteEditor_NoteUpdated(object sender, NoteEventArgs e)
        {
            // Handle NoteUpdated event here accessing the updated note via e.UpdatedNote
            Note? test = e.UpdatedNote;
            logger.LogInfo("[TEST - NoteEditor_NoteUpdated] " + test.Title);
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

            //Get rid of our subscriptions
            MyNotesListView.SelectionChanged -= MyNotesListView_SelectionChanged;
            logger.LogWritten -= LogWrite_Handler;
            noteEditor.NoteUpdated -= NoteEditor_NoteUpdated;

            Environment.Exit(0);
        }

        private void AppSettings_Click(object sender, RoutedEventArgs e)
        {
            // Open AppSettings window or handle app settings here
            //NOT YET IMPLEMENTED
        }

        private void OpenSelectedNote()
        {
            if (MyNotesListView.SelectedItem is Note note)
            {
                NoteTemplateEditor noteEditor = new NoteTemplateEditor(note);
                noteEditor.Show();
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
    }
}
