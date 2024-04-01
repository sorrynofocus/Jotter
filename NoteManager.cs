using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using System.ComponentModel;
using com.nobodynoze.flogger;
using static Jotter.MainWindow;
using System.Windows.Shapes;
using System.Diagnostics;


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
        //      if (Notes == null)
        //        JustExit();
        //}
        //Instead of use block body, use expression body for constructor
        public NoteManager() => Notes = LoadNotes(serializer, jotNotesFilePath) ?? JustExit();

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
                            return serializer.Deserialize(fileStream) as ObservableCollection<Note>;
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
            Environment.Exit(1); // Exit the application with an error code
            return new ObservableCollection<Note>(); // This line is added to satisfy the return type
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
