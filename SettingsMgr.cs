using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Notes:

TransitionToSettings() (located in MainWindow.xaml.cs) is called to transition and open the settings manager. 
Ultimately, is calls settingsWindow.ShowDialog() to display the dialog window. A handler is subscribed to the
settingsWindow.Closed event, which will call SaveSettings() to save the settings when the settings manger window
is closed.

SettingsMgr - mostly dealing with settings outside UI
Settings.xaml.cs - deals with UI and elements of the settings manager window, and calls SettingsMgr to load and save the settings.

* MAIN Windows settings such as width, height, position, and other global settings are stored HERE!

Flow of SettingsManager:

1. When the application starts, an instance of SettingsMgr is created, which automatically loads the application 
   settings and note settings from their respective XML files (if they exist). If the files do not exist or there
   is an error during loading, default settings are used. Environment variables can be used in the configuration
   paths, and they will be expanded to their full paths when loading the settings. The variables used to track 
   data path and note settings path are `CurrentDataFilePath` and `CurrentNoteSettingsFilePath`, which are updated
   based on the loaded settings.
2. The application settings (AppSettings) include properties such as window dimensions, save interval, log path, 
   data path, theme, and various boolean flags for features like date/time stamps and deletion confirmation.
3. The note settings (NotesConfiguration) is a collection of NoteSettings, where each NoteSettings contains a 
   unique noteID (UID), position (Left and Top), and dimension (Width and Height) for an individual note.
4. The SettingsMgr class provides methods to save the application settings and note settings back to their 
   respective XML files. It also includes a method to relocate the data files to a new directory, which involves
   copying existing files to the new location and updating the settings accordingly.
5. The application can retrieve and update individual note configurations using the GetNoteConf, SaveNoteConf,
   and RemoveNoteConf methods, which operate on the NotesConfig collection. These are used to manage the settings
   for each note, such as when a note is created, moved, resized, or deleted.

 */




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
        public double WindowWidth { get; set; } = 660;
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

        //Data path
        //This will also depend on class SettingsMgr() -> SettingsFilePath and NoteSettingsFilePath 
        public string DataPath { get; set; } = string.Empty;


        //Fully exit or minimized to tray? 
        public bool IsTray { get; set; } = false;

        public string Theme { get; set; } = "Default Theme";

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
        public const string NotesFileName = "JotterNotes.xml";
        public const string NoteSettingsFileName = "JotterNoteSettings.xml";

        public static string DefaultDataDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jotter"
        );

        /// <summary>
        /// Jotter application settings file path
        /// </summary>
        private static string SettingsFilePath => Path.Combine(DefaultDataDirectory, "JotterSettings.xml");

        /// <summary>
        /// Jotter note settings file path  
        /// </summary>
        private string NoteSettingsFilePath => GetNoteSettingsFilePath(Settings);

        public static string CurrentDataFilePath { get; private set; } = GetDefaultDataFilePath();

        public static string CurrentNoteSettingsFilePath { get; private set; } = GetDefaultNoteSettingsFilePath();

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

        public string DataFilePath => GetDataFilePath(Settings);

        public string NoteSettingsDataFilePath => GetNoteSettingsFilePath(Settings);

        public static string GetDefaultDataFilePath()
        {
            return (Path.Combine(DefaultDataDirectory, NotesFileName));
        }

        public static string GetDefaultNoteSettingsFilePath()
        {
            return (Path.Combine(DefaultDataDirectory, NoteSettingsFileName));
        }

        public static string GetDataDirectory(AppSettings? settings)
        {
            if (!string.IsNullOrWhiteSpace(settings?.DataPath))
            {
                string configuredPath = ExpandConfiguredPath(settings.DataPath);

                if (string.Equals(Path.GetExtension(configuredPath), ".xml", StringComparison.OrdinalIgnoreCase))
                    return Path.GetDirectoryName(configuredPath) ?? DefaultDataDirectory;

                return configuredPath;
            }

            return DefaultDataDirectory;
        }

        //public static string GetDataFilePath(AppSettings? settings)
        //{
        //    if (!string.IsNullOrWhiteSpace(settings?.DataPath))
        //    {
        //        string configuredPath = settings.DataPath.Trim();

        //        if (string.Equals(Path.GetExtension(configuredPath), ".xml", StringComparison.OrdinalIgnoreCase))
        //            return (configuredPath);

        //        return (Path.Combine(configuredPath, NotesFileName));
        //    }

        //    return (GetDefaultDataFilePath());
        //}

        /// <summary>
        /// This method determines the data file path based on the provided settings. It checks if a 
        /// custom data path is specified in the settings, and if so, it expands any environment variables 
        /// in the path. If the expanded path points to an XML file, it returns that path directly. 
        /// Otherwise, it treats the expanded path as a directory and appends the default notes file name 
        /// to it. If no custom data path is specified, it defaults to using the standard data directory 
        /// with the default notes file name.
        /// </summary>
        /// <param name="settings">The application settings instance.</param>
        /// <returns>The full path to the data file.</returns>
        public static string GetDataFilePath(AppSettings? settings)
        {
            if (!string.IsNullOrWhiteSpace(settings?.DataPath))
            {
                string configuredPath = ExpandConfiguredPath(settings.DataPath);

                if (string.Equals(Path.GetExtension(configuredPath), ".xml", StringComparison.OrdinalIgnoreCase))
                    return configuredPath;
            }
            return Path.Combine(GetDataDirectory(settings), NotesFileName);
        }

        public static string GetNoteSettingsFilePath(AppSettings? settings)
        {
            //string dataFilePath = GetDataFilePath(settings);
            //string? dataDirectory = Path.GetDirectoryName(dataFilePath);

            //if (string.IsNullOrWhiteSpace(dataDirectory))
            //    dataDirectory = DefaultDataDirectory;

            //return (Path.Combine(dataDirectory, NoteSettingsFileName));

            return Path.Combine(GetDataDirectory(settings), NoteSettingsFileName);
        }

        private void SyncCurrentPaths()
        {
            CurrentDataFilePath = GetDataFilePath(Settings);
            CurrentNoteSettingsFilePath = GetNoteSettingsFilePath(Settings);
        }

        /// <summary>
        /// This method is to centralize environment variable expansion for paths.
        ///
        /// </summary>
        /// <param name="path">The path to expand.</param>
        /// <returns>The expanded path.</returns>
        /// Note: This method is static and can be called without an instance of SettingsMgr.
        /// The purpose of this function is to ensure that any path provided in the settings 
        /// that contains environment variables (%APPDATA%) is properly expanded to its full
        /// path before being used by the application.
        public static string ExpandConfiguredPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (string.Empty);

            return (Environment.ExpandEnvironmentVariables(path.Trim()));
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

            if (string.IsNullOrWhiteSpace(Settings.DataPath))
                Settings.DataPath = DefaultDataDirectory;

            SyncCurrentPaths();
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
                SyncCurrentPaths();
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
        /// This method is to move the data files (notes and note settings) to a new directory. 
        /// It will copy the existing files to the new location, update the settings with the 
        /// new path, and save the settings and note settings. If the new directory does not 
        /// exist, it will be created. If the new directory is the same as the 
        /// current, no action will be taken. 
        /// </summary>
        /// <param name="newDirectory">The new directory to move the data files to.</param>
        public void RelocateDataFiles(string newDirectory)
        {
            if (string.IsNullOrWhiteSpace(newDirectory))
                return;

            string configuredDirectory = newDirectory.Trim();
            string expandedDirectory = ExpandConfiguredPath(configuredDirectory);

            string currentDataFilePath = DataFilePath;
            string currentNoteSettingsFilePath = NoteSettingsDataFilePath;
            string newDataFilePath = Path.Combine(expandedDirectory, NotesFileName);
            string newNoteSettingsFilePath = Path.Combine(expandedDirectory, NoteSettingsFileName);

            if (!Directory.Exists(expandedDirectory))
                Directory.CreateDirectory(expandedDirectory);

            if (!string.Equals(currentDataFilePath, newDataFilePath, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(currentDataFilePath))
                    File.Copy(currentDataFilePath, newDataFilePath, true);

                if (File.Exists(currentNoteSettingsFilePath))
                    File.Copy(currentNoteSettingsFilePath, newNoteSettingsFilePath, true);
            }

            Settings.DataPath = configuredDirectory;
            SyncCurrentPaths();
            SaveSettings();
            SaveNoteSettings();
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
