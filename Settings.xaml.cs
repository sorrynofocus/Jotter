﻿using com.nobodynoze.flogger;
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

        //public class CustomToggleButton : CheckBox
        //{
        //    public CustomToggleButton()
        //    {
        //        DefaultStyleKey = typeof(CustomToggleButton);
        //    }
        //}

        //public Settings()
        public Settings(SettingsMgr sharedSettingsManager)
        {
            InitializeComponent();
            //settingsManager = new SettingsMgr();
            settingsManager = sharedSettingsManager;

            LoadAppSettings(settingsManager.Settings);

            // Mark initialization as complete because combobox active when window loads
            // May switch this to a PropertyEvent change event, will monitor
            isInitializing = false;
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


        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Set the location of the data file in UI at load up
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppSettings(settingsManager.Settings);
        }

        private void TopBorderGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void LoadAppSettings(AppSettings settings)
        {
            if (settings == null) return;

            //Set UI Logging
            TextUserData.Text = Jotter.MainWindow.jotNotesFilePath;
            TextLogFile.Text = Jotter.MainWindow.logger.LogFile;

            VersionTextBlock.Text = GetJotterVersion(); 

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
            }



        }

        private void cb_ThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (isInitializing) return;

            if (sender is ComboBox comboBox)
            {
                // Get the selected item
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

                if (selectedItem != null)
                {
                    string selectedTheme = selectedItem.Content.ToString();

                    

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
                }
                else
                    ThemeSelection.SelectedIndex = 0;
            }
        } //cb_ThemeSelectionChanged

        private void CheckedMinimizeToTray(object sender, RoutedEventArgs e)
        {
            if (settingsManager?.Settings != null)
            {
                settingsManager.Settings.IsTray = true;
                settingsManager.SaveSettings();
                Debug.WriteLine("[DEBUG] IsTray set to true and saved.");
            }
        }

        private void CheckedFullExit(object sender, RoutedEventArgs e)
        {
            if (settingsManager?.Settings != null)
            {
                settingsManager.Settings.IsTray = false;
                settingsManager.SaveSettings();
                Debug.WriteLine("[DEBUG] IsTray set to false and saved.");
            }
        }

        static string GetJotterVersion()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;

            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);

            return (versionInfo.FileVersion);
        }
    }
}
