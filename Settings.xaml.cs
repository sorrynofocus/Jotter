using com.nobodynoze.flogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.IO;

/*
 * examepl toggle in XAML
 *  <CheckBox x:Name="cbToggle" Height="45" Width="65" IsChecked="False" Checked="cbToggle_Checked" Unchecked="cbToggle_Unchecked" />
 *  
 */

namespace Jotter
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        private bool isInitializing = true;
        //private SettingsMgr settingsManager;
        private readonly SettingsMgr settingsManager;

        Logger logger = Jotter.MainWindow.logger;

        //Prop3erties for binding to textbox -logs
        public string txtUserData { get; set; } = "C:\\Jotter\\userdata.xml";
        public string txtLogFile { get; set; } = "C:\\Jotter\\logfile.log";

        //public class CustomToggleButton : CheckBox
        //{
        //    public CustomToggleButton()
        //    {
        //        DefaultStyleKey = typeof(CustomToggleButton);
        //    }
        //}

        /// <summary>
        /// Create the Settings window and attach the shared settings manager used by MainWindow.
        /// This also prepares theme discovery so the UI can populate the theme drop-down on load.
        /// </summary>
        /// <param name="sharedSettingsManager">Shared application settings manager instance.</param>
        public Settings(SettingsMgr sharedSettingsManager)
        {
            InitializeComponent();
            DataContext = this;

            //settingsManager = new SettingsMgr();
            settingsManager = sharedSettingsManager;
            ThemeSkinManager.EnsureCustomThemeTemplateExists();
            LoadThemeSelectionItems();
        }

        //private bool SwitchTheme(string themeName)
        //{
        //    //ResourceDictionary lightTheme = new ResourceDictionary { Source = new Uri("Utils/Themes/LightTheme.xaml", UriKind.Relative) };
        //    //Application.Current.Resources.MergedDictionaries.Clear();
        //    //Application.Current.Resources.MergedDictionaries.Add(lightTheme);


        //    //logger.LogInfo($"[switchtheme] Doing action");
        //    Application.Current.Resources.MergedDictionaries.Clear();

        //    foreach (var key in Application.Current.Resources.Keys)
        //    {
        //        Debug.WriteLine($"Reources: {key}");
        //        //logger.LogInfo($"Resources: {key}");
        //    }

        //    try 
        //    {
        //        Uri resourceUri = new Uri(@$"/Utils/Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute);
        //        ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = resourceUri };
        //    }
        //    catch
        //    {
        //        MessageBox.Show($"The theme \"{themeName}\" could not be applied. Please ensure it exists.", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return (false);
        //    }

        //    try
        //    {
        //        // Remove existing theme dictionaries
        //        var existingDictionaries = Application.Current.Resources.MergedDictionaries.ToList();
        //        foreach (var dictionary in existingDictionaries)
        //        {
        //            Application.Current.Resources.MergedDictionaries.Remove(dictionary);
        //        }

        //        // Add the new theme dictionary
        //        Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

        //        foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
        //        {
        //            Debug.WriteLine($"Loaded Dictionary: {dictionary.Source}");
        //            //logger.LogInfo($"Loaded Dictionary: {dictionary.Source}");
        //        }

        //        Debug.WriteLine($"Theme applied: {themeName}");
        //        //logger.LogInfo($"Theme applied: {themeName}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Failed to apply theme: {ex.Message}");
        //        //logger.LogInfo($"Failed to apply theme: {ex.Message}");
        //    }

        //    //logger.LogInfo($"[switchtheme] Doing action");
        //    return (true);
        //}

        /// <summary>
        /// Apply the requested theme or custom skin to the current application resources.
        /// If validation fails, the theme manager falls back to the default theme and updates the saved setting.
        /// </summary>
        /// <param name="themeName">Display name of the built-in theme or discovered custom skin.</param>
        public void SwitchTheme(string themeName)
        {
            if (ThemeSkinManager.ApplyTheme(themeName, out string appliedTheme, out string validationMessage))
            {
                settingsManager.Settings.Theme = appliedTheme;
                Debug.WriteLine($"Theme applied successfully: {themeName}");
                return;
            }

            Debug.WriteLine($"Failed to apply theme: {themeName}. Error: {validationMessage}");
            MessageBox.Show($"The theme \"{themeName}\" could not be applied. Falling back to the default theme.", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            SelectThemeComboBoxItem(appliedTheme);
            settingsManager.Settings.Theme = appliedTheme;
            logger.LogError("[SwitchTheme] Default theme applied as fallback.");
        }

        /// <summary>
        /// Persist the current Settings window choices back into AppSettings, then close the dialog.
        /// This is the final save point for values such as logging, data path, and note behavior options.
        /// </summary>
        /// <param name="sender">Close button sender.</param>
        /// <param name="e">Click event arguments.</param>
        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            ApplyDataPathChange();
            ApplyLogPathChange();
            settingsManager.Settings.IsKennyLoggings = chkEnableLogging.IsChecked == true;
            settingsManager.Settings.IsDateTimeStamp = chkDateTimeStamp.IsChecked == true;
            settingsManager.Settings.IsDeletionConfirm = chkDeletionConfirm.IsChecked == true;

            //settingsManager.Settings.DataPath is the variable in class AppSettings()
            //Adding in 3/24/2025 but need to add code for reloading and figuring out the note settings for the new data path.
            //settingsManager.Settings.DataPath = txtUserData;    
            logger.EnableLogger = settingsManager.Settings.IsKennyLoggings;
            settingsManager.SaveSettings();

            this.Close();
        }

        /// <summary>
        /// Initialize the Settings window state after the XAML has fully loaded.
        /// The initialization flag is held during this load so combo boxes and checkboxes do not save prematurely.
        /// </summary>
        /// <param name="sender">Window sender.</param>
        /// <param name="e">Loaded event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isInitializing = true;
            LoadAppSettings(settingsManager.Settings);
            isInitializing = false;
        }
        /// <summary>
        /// Allow the custom top border to drag the Settings window because WindowStyle is set to None.
        /// </summary>
        /// <param name="sender">Top border grid sender.</param>
        /// <param name="e">Mouse button event arguments.</param>
        private void TopBorderGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        /// <summary>
        /// Populate the Settings UI from the current AppSettings values and supporting runtime state.
        /// This is used when the Settings window opens so the controls reflect the saved configuration.
        /// </summary>
        /// <param name="settings">Current application settings instance.</param>
        public void LoadAppSettings(AppSettings settings)
        {
            if (settings == null) return;
            LoadThemeSelectionItems();
            SwitchTheme(settings.Theme);

            //Get log file path 
            if (!string.IsNullOrWhiteSpace(settingsManager.Settings.LogPath))
                logger.LogFile = settingsManager.Settings.LogPath;
            else
                logger.LogFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               System.IO.Path.Join(Assembly.GetExecutingAssembly().GetName().Name, "JotterNotes.log"));

            logger.EnableLogger = settings.IsKennyLoggings;
            logger.EnableDTStamps = true;
            logger.LogInfo("[Doing action] Settings- LoadAppSettings");



            //Set UI Logging into bound properties to the textbox in UI
            txtUserData = System.IO.Path.GetDirectoryName(settingsManager.DataFilePath) ?? SettingsMgr.DefaultDataDirectory;
            txtLogFile = !string.IsNullOrWhiteSpace(settingsManager.Settings.LogPath)
                ? settingsManager.Settings.LogPath
                : logger.LogFile;
            TextUserData.Text = txtUserData;
            TextLogFile.Text = txtLogFile;
            chkEnableLogging.IsChecked = settings.IsKennyLoggings;
            chkDateTimeStamp.IsChecked = settings.IsDateTimeStamp;
            chkDeletionConfirm.IsChecked = settings.IsDeletionConfirm;
            UpdateOpenLogButton();
            UpdateOpenThemesFolderButton();
            //TextUserData.Text = Jotter.MainWindow.jotNotesFilePath;
            //TextLogFile.Text = Jotter.MainWindow.logger.LogFile;


            logger.LogInfo("[Doing action] Settings- GetJotterVersion started");
            VersionTextBlock.Text = GetJotterVersion();

            logger.LogInfo("[Doing action] Settings- GetJotterVersion finished");



            //Set the radio button for minmize or full exit
            bool isTray = settingsManager.Settings.IsTray;

            // Find the radio buttons in the UI and set their state
            //foreach (var child in LogicalTreeHelper.GetChildren(stackPanel_ExitBehavior))
            //{
            //    if (child is RadioButton radioButton)
            //    {
            //        //if (isTray && radioButton.Content.ToString() == "Minimize to tray")
            //        if (isTray && radioButton.Name == "rb_MinimizeToTray")
            //        {
            //            radioButton.IsChecked = true;
            //            settingsManager.Settings.IsTray = true;
            //            Debug.WriteLine("[DEBUG] MinimizeToTray_Checked fired, IsTray set to true");
            //        }
            //        //else if (!isTray && radioButton.Content.ToString() == "Fully exit")
            //        else if (!isTray && radioButton.Name == "rb_FullExit")
            //        {
            //            radioButton.IsChecked = true;
            //            settingsManager.Settings.IsTray = false;
            //            Debug.WriteLine("[DEBUG] FullyExit_Checked fired, IsTray set to false");
            //        }
            //    }
            //}

            rb_MinimizeToTray.IsChecked = settings.IsTray;
            rb_FullExit.IsChecked = !settings.IsTray;



            //Check the themes
            if (ThemeSelection != null)
            {
                // Iterate through ComboBox to locate -and- select the matching theme
                foreach (var item in ThemeSelection.Items)
                {
                    if (item is ComboBoxItem comboBoxItem &&
                        comboBoxItem.Content.ToString() == settings.Theme)
                    {
                        ThemeSelection.SelectedItem = comboBoxItem;
                        break;
                    }
                }

                if (ThemeSelection.SelectedItem == null)
                    SelectThemeComboBoxItem("Default Theme");
            }

            //Check the save technique
            if (AutoSave != null)
            {
                foreach (ComboBoxItem item in AutoSave.Items)
                {
                    if (int.TryParse(item.Tag?.ToString(), out int tagValue) && tagValue == settings.SaveInterval)
                    {
                        AutoSave.SelectedItem = item;
                        break;
                    }
                }
            }



            logger.LogInfo("[Ending action] Settings- LoadAppSettings");

        }

        /// <summary>
        /// Handle theme selection changes from the drop-down and save the new theme immediately.
        /// Initialization changes are ignored so the saved theme is not overwritten while the window is loading.
        /// </summary>
        /// <param name="sender">Theme ComboBox sender.</param>
        /// <param name="e">Selection changed event arguments.</param>
        private void cb_ThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Catch here because all selections will change when window is brought up.
            //Therefore, settings Manager will not be init'd yet.
            if (isInitializing) return;

            if (sender is ComboBox comboBox)
            {
                // Get the selected item
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

                if (selectedItem != null)
                {
                    string selectedTheme = selectedItem.Content.ToString();

                    
                    SwitchTheme(selectedTheme);
                    settingsManager.SaveSettings();
                }
                else
                    ThemeSelection.SelectedIndex = 0;
            }
        } //cb_ThemeSelectionChanged
        /// <summary>
        /// Select the matching item in the theme drop-down based on the saved theme name.
        /// This is used after loading settings and after fallback-to-default theme handling.
        /// </summary>
        /// <param name="themeName">Theme display name to locate in the ComboBox.</param>
        private void SelectThemeComboBoxItem(string themeName)
        {
            foreach (var item in ThemeSelection.Items)
            {
                if (item is ComboBoxItem comboBoxItem &&
                    string.Equals(comboBoxItem.Content?.ToString(), themeName, StringComparison.Ordinal))
                {
                    ThemeSelection.SelectedItem = comboBoxItem;
                    break;
                }
            }
        }
        /// <summary>
        /// Rebuild the theme drop-down using built-in themes plus any custom skins found on disk.
        /// This keeps the Settings window in sync with files stored in the CustomThemes folder.
        /// </summary>
        private void LoadThemeSelectionItems()
        {
            if (ThemeSelection == null)
                return;

            ThemeSelection.Items.Clear();

            foreach (string builtInThemeName in ThemeSkinManager.GetBuiltInThemeDisplayNames())
                ThemeSelection.Items.Add(new ComboBoxItem() { Content = builtInThemeName });

            foreach (string customThemeName in ThemeSkinManager.GetAvailableCustomThemeDisplayNames())
                ThemeSelection.Items.Add(new ComboBoxItem() { Content = customThemeName });
        }
        /// <summary>
        /// Show or hide the "Open folder" button depending on whether the custom themes folder exists.
        /// The button is only useful when the skin directory is present on disk.
        /// </summary>
        private void UpdateOpenThemesFolderButton()
        {
            if (btnOpenThemesFolder == null)
                return;

            btnOpenThemesFolder.Visibility = Directory.Exists(ThemeSkinManager.CustomThemesDirectory)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Save the selected autosave interval when the user changes the ComboBox selection.
        /// The ComboBoxItem Tag value is used as the persisted interval in seconds.
        /// </summary>
        /// <param name="sender">Autosave ComboBox sender.</param>
        /// <param name="e">Selection changed event arguments.</param>
        private void cb_AutoSaveValChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            if (AutoSave.SelectedItem is ComboBoxItem selectedItem &&
                int.TryParse(selectedItem.Tag?.ToString(), out int tagValue))
            {
                settingsManager.Settings.SaveInterval = tagValue; 
                settingsManager.SaveSettings();
            }
        }//cb_ThemeSelectionChanged


        /// <summary>
        /// Persist the "Minimize to tray" choice when its radio button is selected.
        /// </summary>
        /// <param name="sender">Radio button sender.</param>
        /// <param name="e">Checked event arguments.</param>
        private void CheckedMinimizeToTray(object sender, RoutedEventArgs e)
        {
            if (settingsManager?.Settings != null)
            {
                settingsManager.Settings.IsTray = true;
                settingsManager.SaveSettings();
                Debug.WriteLine("[DEBUG] IsTray set to true and saved.");
            }
        }
        /// <summary>
        /// Persist the "Fully exit" choice when its radio button is selected.
        /// </summary>
        /// <param name="sender">Radio button sender.</param>
        /// <param name="e">Checked event arguments.</param>
        private void CheckedFullExit(object sender, RoutedEventArgs e)
        {
            if (settingsManager?.Settings != null)
            {
                settingsManager.Settings.IsTray = false;
                settingsManager.SaveSettings();
                Debug.WriteLine("[DEBUG] IsTray set to false and saved.");
            }
        }
        /// <summary>
        /// Enable logging from the Settings toggle and immediately save the updated preference.
        /// The current log path is applied first so future writes use the latest destination.
        /// </summary>
        /// <param name="sender">Checkbox sender.</param>
        /// <param name="e">Checked event arguments.</param>
        private void chkEnableLogging_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || settingsManager?.Settings == null)
                return;

            ApplyLogPathChange();
            settingsManager.Settings.IsKennyLoggings = true;
            logger.EnableLogger = true;
            settingsManager.SaveSettings();
            UpdateOpenLogButton();
        }
        /// <summary>
        /// Disable logging from the Settings toggle and immediately save the updated preference.
        /// </summary>
        /// <param name="sender">Checkbox sender.</param>
        /// <param name="e">Unchecked event arguments.</param>
        private void chkEnableLogging_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || settingsManager?.Settings == null)
                return;

            settingsManager.Settings.IsKennyLoggings = false;
            logger.EnableLogger = false;
            settingsManager.SaveSettings();
            UpdateOpenLogButton();
        }
        /// <summary>
        /// Commit any edited log path when focus leaves the log path textbox.
        /// This allows users to type a path manually without needing a separate Save button.
        /// </summary>
        /// <param name="sender">Textbox sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void TextLogFile_LostFocus(object sender, RoutedEventArgs e)
        {
            if (settingsManager?.Settings == null)
                return;

            ApplyLogPathChange();
        }
        /// <summary>
        /// Commit any edited user data path when focus leaves the data path textbox.
        /// The directory is normalized and then used to relocate the notes files if needed.
        /// </summary>
        /// <param name="sender">Textbox sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void TextUserData_LostFocus(object sender, RoutedEventArgs e)
        {
            if (settingsManager?.Settings == null)
                return;

            ApplyDataPathChange();
        }
        /// <summary>
        /// Open a folder picker so the user can choose a new base directory for note data files.
        /// After selection, the notes XML files are relocated and the textbox is refreshed.
        /// </summary>
        /// <param name="sender">Browse button sender.</param>
        /// <param name="e">Click event arguments.</param>
        private void btnBrowseUserData_Click(object sender, RoutedEventArgs e)
        {
            string initialDirectory = GetUserDataDirectoryFromInput(TextUserData.Text);

            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                folderDialog.InitialDirectory = Directory.Exists(initialDirectory)
                    ? initialDirectory
                    : System.IO.Path.GetDirectoryName(settingsManager.DataFilePath) ?? initialDirectory;
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtUserData = System.IO.Path.Combine(folderDialog.SelectedPath, SettingsMgr.NotesFileName);
                    TextUserData.Text = txtUserData;
                    ApplyDataPathChange();
                }
            }
        }
        /// <summary>
        /// Open the current log file in Notepad when it exists on disk.
        /// If the file is missing, the button visibility is refreshed instead of launching anything.
        /// </summary>
        /// <param name="sender">Open log button sender.</param>
        /// <param name="e">Click event arguments.</param>
        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            string logPath = !string.IsNullOrWhiteSpace(txtLogFile) ? txtLogFile.Trim() : logger.LogFile;

            if (!File.Exists(logPath))
            {
                UpdateOpenLogButton();
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{logPath}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        /// <summary>
        /// Open the custom themes folder in Explorer so users can manage skin files directly.
        /// If the folder has been removed, the button visibility is refreshed and nothing is opened.
        /// </summary>
        /// <param name="sender">Open folder button sender.</param>
        /// <param name="e">Click event arguments.</param>
        private void btnOpenThemesFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(ThemeSkinManager.CustomThemesDirectory))
            {
                UpdateOpenThemesFolderButton();
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ThemeSkinManager.CustomThemesDirectory,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        /// <summary>
        /// Persist the note date/time stamp option when the toggle is turned on.
        /// </summary>
        /// <param name="sender">Checkbox sender.</param>
        /// <param name="e">Checked event arguments.</param>
        private void chkDateTimeStamp_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || settingsManager?.Settings == null)
                return;

            settingsManager.Settings.IsDateTimeStamp = true;
            settingsManager.SaveSettings();
        }
        /// <summary>
        /// Persist the note date/time stamp option when the toggle is turned off.
        /// </summary>
        /// <param name="sender">Checkbox sender.</param>
        /// <param name="e">Unchecked event arguments.</param>
        private void chkDateTimeStamp_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || settingsManager?.Settings == null)
                return;

            settingsManager.Settings.IsDateTimeStamp = false;
            settingsManager.SaveSettings();
        }
        /// <summary>
        /// Persist the note deletion confirmation option when the toggle is turned on.
        /// </summary>
        /// <param name="sender">Checkbox sender.</param>
        /// <param name="e">Checked event arguments.</param>
        private void chkDeletionConfirm_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || settingsManager?.Settings == null)
                return;

            settingsManager.Settings.IsDeletionConfirm = true;
            settingsManager.SaveSettings();
        }
        /// <summary>
        /// Persist the note deletion confirmation option when the toggle is turned off.
        /// </summary>
        /// <param name="sender">Checkbox sender.</param>
        /// <param name="e">Unchecked event arguments.</param>
        private void chkDeletionConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || settingsManager?.Settings == null)
                return;

            settingsManager.Settings.IsDeletionConfirm = false;
            settingsManager.SaveSettings();
        }
        /// <summary>
        /// Normalize and save the log file path entered in the UI.
        /// Logging is temporarily paused while the path is updated so writes do not target a stale file handle.
        /// </summary>
        private void ApplyLogPathChange()
        {
            string newLogPath = txtLogFile?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(newLogPath))
            {
                UpdateOpenLogButton();
                return;
            }

            bool wasLoggingEnabled = logger.EnableLogger;
            logger.EnableLogger = false;

            string? logDirectory = System.IO.Path.GetDirectoryName(newLogPath);
            if (!string.IsNullOrWhiteSpace(logDirectory) && !Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            txtLogFile = newLogPath;
            settingsManager.Settings.LogPath = newLogPath;
            logger.LogFile = newLogPath;
            logger.EnableLogger = settingsManager.Settings.IsKennyLoggings;
            settingsManager.SaveSettings();

            if (!wasLoggingEnabled && !settingsManager.Settings.IsKennyLoggings)
                logger.EnableLogger = false;

            UpdateOpenLogButton();
        }
        /// <summary>
        /// Normalize and apply the user data directory entered in the UI.
        /// This relocates the paired notes files and then refreshes the textbox with the resolved directory.
        /// </summary>
        private void ApplyDataPathChange()
        {
            string userDataInput = TextUserData.Text?.Trim() ?? string.Empty;
            string newDirectory = GetUserDataDirectoryFromInput(userDataInput);

            if (string.IsNullOrWhiteSpace(newDirectory))
                return;

            settingsManager.RelocateDataFiles(newDirectory);
            txtUserData = System.IO.Path.GetDirectoryName(settingsManager.DataFilePath) ?? SettingsMgr.DefaultDataDirectory;
            TextUserData.Text = txtUserData;
        }
        /// <summary>
        /// Convert the user-entered data path into a directory path the app can use for relocation.
        /// If the user types a full XML file path, only the parent directory is returned.
        /// </summary>
        /// <param name="userDataInput">Raw textbox value entered by the user.</param>
        /// <returns>Resolved directory path for note data storage.</returns>
        private string GetUserDataDirectoryFromInput(string? userDataInput)
        {
            string normalizedInput = userDataInput?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedInput))
                return (System.IO.Path.GetDirectoryName(settingsManager.DataFilePath) ?? string.Empty);

            if (string.Equals(System.IO.Path.GetExtension(normalizedInput), ".xml", StringComparison.OrdinalIgnoreCase))
                return (System.IO.Path.GetDirectoryName(normalizedInput) ?? string.Empty);

            return (normalizedInput);
        }
        /// <summary>
        /// Show or hide the "Open log" button based on whether the resolved log file exists.
        /// This keeps the button from appearing when there is nothing to open yet.
        /// </summary>
        private void UpdateOpenLogButton()
        {
            string logPath = !string.IsNullOrWhiteSpace(txtLogFile) ? txtLogFile.Trim() : logger.LogFile;
            btnOpenLog.Visibility = File.Exists(logPath) ? Visibility.Visible : Visibility.Collapsed;
        }
        /// <summary>
        /// Read the version metadata from the running assembly for display in Settings.
        /// FileVersion is preferred because it matches the build-generated version shown in EXE properties.
        /// </summary>
        /// <returns>Resolved application version string for the Settings footer.</returns>
        static string GetJotterVersion()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

            if (!string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                return (versionInfo.FileVersion);

            if (!string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
                return (versionInfo.ProductVersion);

            Assembly assembly = Assembly.GetExecutingAssembly();
            return (assembly.GetName().Version?.ToString() ?? string.Empty);

        }
    }
}
