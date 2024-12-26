using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jotter
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Xml.Serialization;


    #region APPLICATION SETTINGS DATA STRUCTURE  
    /// <summary>
    /// Data struct for the jotter application global settings
    /// XML Root is "JotterConf"
    /// </summary>
    [XmlRoot("JotterConf")]
    public class AppSettings
    {
        //Width x Height
        public double WindowWidth { get; set; } = 450;
        public double WindowHeight { get; set; } = 600;
        //Xpos
        public double WindowLeft { get; set; } = 100;
        //YPos
        public double WindowTop { get; set; } = 100;
        //Is MaximizeD?
        public bool IsMaximized { get; set; } = false;

        //Save interval
        //default 3000 milliseconds = 3 seconds
        public int SaveInterval { get; set; } = 3000;

        //Log path
        public string LogPath { get; set; } = string.Empty;

        //Fully exit or minimized to tray? 
        public bool IsTray { get; set; } = false;

        public string Theme { get; set; } = "Light";

        //Add date/time stamp to note
        public bool IsDateTimeStamp { get; set; } = false;

        //Confirm deletion?
        public bool IsDeletionConfirm { get; set; } = true;

        //Enable logging?
        public bool IsKennyLoggings { get; set; } = false;


    } //AppSettings

    #endregion



    #region INDIVIDUAL NOTE SETTING DATA STRUCTURE

    /// <summary>
    /// Data struct for the jotter application Note settings
    /// The classes Position and Dimension are used to store the note's position and dimension. 
    /// These custom types are better for serialization than using four variables to store the
    /// note's position and dimension.
    /// 
    /// Examples on how to use:
    /// 
    /// Setter
    /// NoteSettings.NotePos = new Position { Left = 2099, Top = 616 };
    /// NoteSettings.NoteDim = new Dimension { Width = 450, Height = 765 };
    /// 
    /// or
    /// 
    /// noteSettings.NotePos = new Position
    /// {
    ///   Left = noteWindow.Left,
    ///   Top = noteWindow.Top
    /// };
    ///
    /// noteSettings.NoteDim = new Dimension
    /// {
    ///   Width = noteWindow.Width,
    ///   Height = noteWindow.Height
    /// };
    /// 
    /// 
    /// Getter:
    /// double left = NoteSettings.NotePos.Left;
    /// double top = NoteSettings.NotePos.Top;
    /// double width = NoteSettings.NoteDim.Width;
    /// double height = NoteSettings.NoteDim.Height;
    /// 
    /// 
    /// </summary>
    public class Position
    {
        public double Left { get; set; } = 450;
        public double Top { get; set; } = 600;  
    }

    public class Dimension
    {
        public double Width { get; set; } = 450;
        public double Height { get; set; } = 600;
    }

    /// <summary>
    /// Each item of notes configuration entity
    /// </summary>
    public class NoteSettings
    {
        public Guid noteID { get; set; }

        public Position NotePos { get; set; } = new Position();
        public Dimension NoteDim { get; set; } = new Dimension();
    }

    /// <summary>
    /// A collection container to hold each note's settings (NoteSettings)
    /// </summary>
    [XmlRoot("JotterNoteConfigs")]
    public class NotesConfiguration
    {
        public List<NoteSettings> Notes { get; set; } = new List<NoteSettings>();
    }
    #endregion



    #region MAIN SETTINGS MANAGER
    //Operations to load and save the settings for the application and notes.

    /// <summary>
    /// Handles saving and loading of Jotter application settings, via XML file
    /// </summary>
    public class SettingsMgr
    {
        /// <summary>
        /// Jotter application settings file path
        /// </summary>
        private static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jotter", "JotterSettings.xml"
        );

        /// <summary>
        /// Jotter note settings file path  
        /// </summary>
        private static string NoteSettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jotter", "JotterNoteSettings.xml"
        );

        /// <summary>
        /// Settings - property of the application settings.
        /// </summary>
        public AppSettings Settings { get; set; }

        /// <summary>
        /// NoteConfig  - property of the container collection of notes settings
        /// </summary>
        public NotesConfiguration NotesConfig { get; set; } = new NotesConfiguration();

        /// <summary>
        /// CLass ctr to start it all and contorl the application loading and closing its settings
        /// </summary>
        public SettingsMgr()
        {
            LoadSettings();
            LoadNoteSettings();
        }

        /// <summary>
        /// Load Jotter's global settings
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    using (FileStream fs = new FileStream(SettingsFilePath, FileMode.Open))
                    {
                        Settings = (AppSettings)serializer.Deserialize(fs);
                    }
                }
                catch
                {
                    //Something bad happend, you better fallback.
                    Settings = new AppSettings();
                }
            }
            else
                //Defaults
                Settings = new AppSettings(); 
        }

        /// <summary>
        /// Save Jotter's global Note settings
        ///
        /// example call:
        /// 
        /// settingsManager.Settings.WindowWidth = 800;
        /// settingsManager.SaveSettings();
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                string appPathLoc = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(appPathLoc))
                    Directory.CreateDirectory(appPathLoc);

                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (FileStream fs = new FileStream(SettingsFilePath, FileMode.Create))
                {
                    serializer.Serialize(fs, Settings);
                }
            }
            catch (Exception ex)
            {
                // TODO: pop up message for ex.Message 
            }
        }


        /// <summary>
        /// Load Jotter's global individual Note settings
        /// </summary>
        public void LoadNoteSettings()
        {
            if (File.Exists(NoteSettingsFilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(NotesConfiguration));
                    using (FileStream fs = new FileStream(NoteSettingsFilePath, FileMode.Open))
                    {
                        NotesConfig = (NotesConfiguration)serializer.Deserialize(fs);
                    }
                }
                catch
                {
                    NotesConfig = new NotesConfiguration(); // Fallback to default
                }
            }
            else
                NotesConfig = new NotesConfiguration();
        }


        /// <summary>
        /// Save Jotter's Note settings
        /// </summary>
        public void SaveNoteSettings()
        {
            try
            {
                string appPathLoc = Path.GetDirectoryName(NoteSettingsFilePath);
                if (!Directory.Exists(appPathLoc))
                    Directory.CreateDirectory(appPathLoc);

                XmlSerializer serializer = new XmlSerializer(typeof(NotesConfiguration));
                using (FileStream fs = new FileStream(NoteSettingsFilePath, FileMode.Create))
                {
                    serializer.Serialize(fs, NotesConfig);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving note settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Find a specific note configuration by an ID
        /// </summary>
        /// <param name="noteID">the identification GUID of the note</param>
        /// <returns></returns>
        public NoteSettings GetNoteConf(Guid noteID)
        {
            return (NotesConfig.Notes.FirstOrDefault(n => n.noteID == noteID) ?? new NoteSettings { noteID = noteID });
        }

        /// <summary>
        /// Add, or update an individual note configuration. This is a helper method to save note settings. 
        /// </summary>
        /// <param name="noteSettings">NoteSettings obj that contains settings for a note</param>
        public void SaveNoteConf(NoteSettings noteSettings)
        {
            NoteSettings? existingNote = NotesConfig.Notes.FirstOrDefault(n => n.noteID == noteSettings.noteID);
            if (existingNote != null)
            {
                existingNote.NotePos = noteSettings.NotePos;
                existingNote.NoteDim = noteSettings.NoteDim;
            }
            else
                NotesConfig.Notes.Add(noteSettings);

            SaveNoteSettings();
        }

        /// <summary>
        /// Remove a note configuration by its ID. This is a helper method to remove 
        /// and save note settings.   
        /// </summary>
        /// <param name="noteID">the identification GUID of the note</param>
        public void RemoveNoteConf(Guid noteID)
        {
            NoteSettings? noteToRemove = NotesConfig.Notes.FirstOrDefault(n => n.noteID == noteID);

            if (noteToRemove != null)
            {
                NotesConfig.Notes.Remove(noteToRemove);
                SaveNoteSettings();
            }
        }

    } //SettingsMgr
    #endregion

} //namespace
