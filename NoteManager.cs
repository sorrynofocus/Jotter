using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using System.ComponentModel;
using com.nobodynoze.flogger;
using static Jotter.MainWindow;


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
 */

namespace com.nobodynoze.notemanager
{
    /// <summary>
    ///Note class embodies the note data and its properties such as title, index, and body.
    /// </summary>
    public class Note : INotifyPropertyChanged
    {
        private Guid? IdIndex =Guid.Empty;
        private string? title;
        private string? text;
        private bool isDeleted; // New property for soft delete - added in for experimental

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
        /// Snitches to the listeners a property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        //}
        //Instead of use block body, use expression body for constructor
        public NoteManager() => Notes = LoadNotes(serializer, jotNotesFilePath);

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
                using (FileStream? fileStream = new FileStream(filePath, FileMode.Open))
                {
                    /* TODO check exception error: System.InvalidOperationException
                     * Inner exception: XmlException: 'Element' is an invalid XmlNodeType.
                    */
                    serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
                    //return (ObservableCollection<Note>)serializer.Deserialize(fileStream);
                    //Possible null ref, declare private ObservableCollection<Note> as nullable
                    return serializer.Deserialize(fileStream) as ObservableCollection<Note>;

                    /*
                    TODO: This needs some heavy error checking.   
                    Other errors to check
                    System.InvalidOperationException: 'There is an error in XML document (5, 6).'
                    Inner Exception FormatException: Unrecognized Guid format.
                     */

                }
            }
            else
                return new ObservableCollection<Note>();
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
                existingNote.Text = note.Text;
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
            logger.LogInfo(new List<string> { "[Doing action] RemoveNote", "Deleting selected note." });

            Note? noteToRemove = Notes.FirstOrDefault(n => n.Title == title);
            if (noteToRemove != null)
            {
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
        public Guid GenerateID()
        { 
            return Guid.NewGuid(); 
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
