/*
* Jotter
* C.Winters / US / Arizona / Thinkpad T15g
* March 2024
*/

using com.nobodynoze.flogger;
using com.nobodynoze.notemanager;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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

            // Initialize last activity time
            lastActivityTime = DateTime.Now;
            /////////////////////////////////////////

            //DataContext = this;
            DataContext = note;

            //SelectedNote = selectedNote;

            InitializeComponent();

            // Subscribe to text changed event of the RichTextBox tracking user activity
            rchEditNote.TextChanged += RchEditNote_TextChanged;

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
            StickyNotesTitle.Text = noteTitle;

            //Set the richedit with the note contents
            //XAML RichTextBox x:Name="rchEditNote"
            rchEditNote.Document.Blocks.Clear();
            rchEditNote.Document.Blocks.Add(new Paragraph(new Run(noteContents)));
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
        private void StickyNotesTitleTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return) UpdateSelectedNoteTitle();
        }

        /// <summary>
        /// With the current note, change the title, then clear focus.
        /// </summary>
        private void UpdateSelectedNoteTitle()
        {
            if (SelectedNote != null && SelectedNote.Title != StickyNotesTitle.Text)
            {
                SelectedNote.Title = StickyNotesTitle.Text;
            }
            Keyboard.ClearFocus();
        }

    } //NoteTemplateEditor: Window
} //Namespace Jotter
