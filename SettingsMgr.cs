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


    }


    /// <summary>
    /// Handles saving and loading of Jotter application settings, via XML file
    /// </summary>
    public class SettingsMgr
    {
        private static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jotter", "JotterSettings.xml"
        );

        /// <summary>
        /// Data structure of the settings (see above this class)
        /// </summary>
        public AppSettings Settings { get; set; }

        /// <summary>
        /// CLass ctr to start it all and contorl the application loading and closing its settings
        /// </summary>
        public SettingsMgr()
        {
            LoadSettings();
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
                Settings = new AppSettings(); // Default settings
        }

        /// <summary>
        /// Save Jotter's settings
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
    }
}
