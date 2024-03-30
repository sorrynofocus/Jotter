/*
* Jotter
* C.Winters / US / Arizona / Thinkpad T15g
* March 2024
*/

using com.nobodynoze.flogger;
using com.nobodynoze.notemanager;
using System.Collections.ObjectModel;
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

        //private void Window_Closed(object sender, EventArgs e)
        //{
        //    NoteUpdated?.Invoke(this, new NoteEventArgs((Note)DataContext));
        //}


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

        //The child window -this- is closing, so save!
        private void NoteTemplateEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check if there's a selected note and save its contents
            if (SelectedNote != null)
            {
                SaveNoteChanges();
            }
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
                SaveNoteChanges1();
            }
            this.Close();
        }


        /*
         * HELLO to SELF!
         * Had a problem with saving, so there's two funcs: SaveNoteChanges() and SaveNoteChanges1()
         * Debug and find out which func is best method to use or combine.
         * I think I got confused -where- the best area is to save -closing or exit- of the child
         * window. Also, the indexing was added to track the notes for existing ones, etc. So, there's 
         * probably redundancy.
         * 
         * SaveNoteChanges1() -> fired at ExitcurrentNote_Click(), that completely exits tthe child window
         * 
         * SaveNoteChanges() -> fired at NoteTemplateEditor_Closing(), and this happens -during- the 
         * window closing. This also gets fired at SaveTimer_Tick().
         * 
         * The difference is SaveNoteChanges1() doesn't check if the note existing by checking the index
         * 
         * Note - during test saves the data appears right 
         */

        // SAve the note, without indexing. See "HELLO to SELF!"
        private void SaveNoteChanges1()
        {
            //Get note data and fill into SelectedNote.Text
            //SelectedNote.Text = new TextRange(rchEditNote.Document.ContentStart, rchEditNote.Document.ContentEnd).Text;
            UpdateSelectedNoteContent();
            
            // Notify the parent window about the updated note before saving
            NoteUpdated?.Invoke(this, new NoteEventArgs(SelectedNote));

            //Save notes
            //noteManager?.SaveNotes(jotNotesFilePath);

            // Save the note using the NoteManager
            if (noteManager != null)
                //noteManager.Notes.Add(SelectedNote); 
                noteManager.SaveNotes(jotNotesFilePath);
            else
                MessageBox.Show("Error: NoteManager is null. Unable to save changes.", 
                                "Error", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);

            // Force update the Notes collection after saving
            Notes = noteManager?.Notes;
        }

        // SAve the note, with indexing. See "HELLO to SELF!"
        private void SaveNoteChanges()
        {
            // Update the SelectedNote with new content
            // Is this redundant? Below we do that with existingNote?
            // I think so! Test!
            UpdateSelectedNoteContent();

            // Check if the note already exists in the Notes collection based on IdIndexer
            Note existingNote = Notes.FirstOrDefault(n => n.IdIndexer == SelectedNote.IdIndexer);

            if (existingNote != null)
            {
                // Update the existing note with the content of SelectedNote
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
