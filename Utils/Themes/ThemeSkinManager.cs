using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Jotter
{
    public static class ThemeSkinManager
    {
        private const string DefaultThemeDisplayName = "Default Theme";
        private const string DefaultThemeResourceName = "DefaultTheme";
        private const string CustomThemeFileName = "EarlKelly.xaml";

        private static readonly string[] BuiltInThemeDisplayNames =
        {
            "Light Theme",
            "Dark Theme",
            DefaultThemeDisplayName
        };

        private static readonly HashSet<string> AllowedCustomSkinKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "WindowBackgroundBrush",
            "WindowBorderBrush",
            "MainGridBackgroundBrush",
            "HeaderBackgroundBrush",
            "HeaderForegroundBrush",
            "SearchBoxBackgroundBrush",
            "SearchBoxForegroundBrush",
            "ListViewBackgroundBrush",
            "ListViewForegroundBrush",
            "ContentBackgroundBrush",
            "NoteEditBackgroundBrush",
            "NoteEditForegroundBrush",
            "SelectedNoteBackgroundBrush",
            "SelectedNoteBorderBrush",
            "NoteBlockBackgroundBrush",
            "NoteBlockBackgroundInnerBrush",
            "NoteBlockBorderBrush",
            "NoteShadowColor",
            "NoteDateForegroundBrush",
            "NoteTitleBackgroundBrush",
            "NoteTitleForegroundBrush",
            "NoteTextForegroundBrush",
            "Button.Static.Background",
            "Button.Static.Border",
            "Button.MouseOver.Background",
            "Button.MouseOver.Border",
            "Button.Pressed.Background",
            "Button.Pressed.Border",
            "Button.Disabled.Background",
            "Button.Disabled.Border",
            "Button.Disabled.Foreground",
            "Button.Static.Background1",
            "Button.Static.Border1",
            "Button.MouseOver.Background1",
            "Button.MouseOver.Border1",
            "Button.Pressed.Background1",
            "Button.Pressed.Border1",
            "Button.Disabled.Background1",
            "Button.Disabled.Border1",
            "Button.Disabled.Foreground1"
        };

        public static string CustomThemesDirectory => Path.Combine(SettingsMgr.DefaultDataDirectory, "CustomThemes");

        public static string CustomThemeFilePath => Path.Combine(CustomThemesDirectory, CustomThemeFileName);

        public static IReadOnlyList<string> GetBuiltInThemeDisplayNames()
        {
            return (BuiltInThemeDisplayNames);
        }

        public static List<string> GetAvailableCustomThemeDisplayNames()
        {
            EnsureCustomThemeTemplateExists();

            List<string> customThemes = Directory
                .GetFiles(CustomThemesDirectory, "*.xaml")
                .Select(filePath => Path.GetFileNameWithoutExtension(filePath))
                .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (customThemes);
        }

        public static void EnsureCustomThemeTemplateExists()
        {
            if (!Directory.Exists(CustomThemesDirectory))
                Directory.CreateDirectory(CustomThemesDirectory);

            if (File.Exists(CustomThemeFilePath))
                return;

            File.WriteAllText(CustomThemeFilePath,
@"<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">

    <!--
    Custom skin rules:
    - Only override brush keys already used by Jotter.
    - Do not add Style, ControlTemplate, FontFamily, or layout resources.
    - Remove any keys you do not want to override.

    Brush guide:
    - WindowBackgroundBrush: Main window background for Settings and note windows.
    - WindowBorderBrush: Main outer window border color.
    - MainGridBackgroundBrush: Main Note Manager surface behind the note list.
    - HeaderBackgroundBrush: Top title/header bars in the app windows.
    - HeaderForegroundBrush: Text/icons shown on the header bars.
    - SearchBoxBackgroundBrush: Search box background in the note manager.
    - SearchBoxForegroundBrush: Search box text color in the note manager.
    - ListViewBackgroundBrush: Background of the note list area.
    - ContentBackgroundBrush: Main content region behind note cards in the manager.
    - SelectedNoteBackgroundBrush: Background used when a note card is selected.
    - SelectedNoteBorderBrush: Border used when a note card is selected.
    - NoteBlockBackgroundBrush: Outer note card background in the manager list.
    - NoteBlockBackgroundInnerBrush: Inner body fill of a note card in the manager list.
    - NoteBlockBorderBrush: Border color around note cards.
    - NoteDateForegroundBrush: Date/time text color shown on note cards.
    - NoteTitleBackgroundBrush: Background behind note titles in the manager list.
    - NoteTitleForegroundBrush: Note title text color in the manager list.
    - NoteTextForegroundBrush: Main text color for note previews and general themed text.
    - NoteEditBackgroundBrush: Background color of the RichEdit note editor surface.
    - NoteEditForegroundBrush: Text color inside the RichEdit note editor surface.
    - NoteShadowColor: Color used by the note card drop shadow.

    Tip:
    - For dark skins, set NoteEditForegroundBrush and NoteTextForegroundBrush to light colors.
    - For light skins, use darker text colors so note content stays readable.
    - Use a softer shadow color instead of pure white unless the theme is intentionally stylized.
    -->

    <SolidColorBrush x:Key=""WindowBackgroundBrush"" Color=""#FFF9F3E8"" />
    <SolidColorBrush x:Key=""WindowBorderBrush"" Color=""#FF6B4A34"" />
    <SolidColorBrush x:Key=""MainGridBackgroundBrush"" Color=""#FFF4E8D6"" />
    <SolidColorBrush x:Key=""HeaderBackgroundBrush"" Color=""#FF6B4A34"" />
    <SolidColorBrush x:Key=""HeaderForegroundBrush"" Color=""#FFFFFFFF"" />
    <SolidColorBrush x:Key=""SearchBoxBackgroundBrush"" Color=""#FFF6E4CB"" />
    <SolidColorBrush x:Key=""SearchBoxForegroundBrush"" Color=""#FF2E241C"" />
    <SolidColorBrush x:Key=""ListViewBackgroundBrush"" Color=""#FFF8EEDF"" />
    <SolidColorBrush x:Key=""ContentBackgroundBrush"" Color=""#FFF4E8D6"" />
    <SolidColorBrush x:Key=""NoteEditBackgroundBrush"" Color=""#FFFBF4E8"" />
    <SolidColorBrush x:Key=""NoteEditForegroundBrush"" Color=""#FF2E241C"" />
    <SolidColorBrush x:Key=""SelectedNoteBackgroundBrush"" Color=""#FFF1D4B1"" />
    <SolidColorBrush x:Key=""SelectedNoteBorderBrush"" Color=""#FF8C5A2B"" />
    <SolidColorBrush x:Key=""NoteBlockBackgroundBrush"" Color=""#FFE7C79D"" />
    <SolidColorBrush x:Key=""NoteBlockBackgroundInnerBrush"" Color=""#FFFCF6EC"" />
    <SolidColorBrush x:Key=""NoteBlockBorderBrush"" Color=""#FFBC7A3F"" />
    <SolidColorBrush x:Key=""NoteDateForegroundBrush"" Color=""#FF6A503C"" />
    <SolidColorBrush x:Key=""NoteTitleBackgroundBrush"" Color=""#FFF6E4CB"" />
    <SolidColorBrush x:Key=""NoteTitleForegroundBrush"" Color=""#FF2E241C"" />
    <SolidColorBrush x:Key=""NoteTextForegroundBrush"" Color=""#FF2E241C"" />
    <Color x:Key=""NoteShadowColor"">#FFB88A5A</Color>

</ResourceDictionary>
");
        }

        public static bool ApplyTheme(string selectedTheme, out string appliedTheme, out string validationMessage)
        {
            appliedTheme = selectedTheme;
            validationMessage = string.Empty;

            try
            {
                if (IsBuiltInTheme(selectedTheme))
                {
                    ResourceDictionary resourceDictionary = LoadThemeResource(GetThemeResourceName(selectedTheme));
                    ApplyMergedDictionaries(resourceDictionary);
                    return (true);
                }

                if (string.Equals(selectedTheme, "Custom Theme", StringComparison.Ordinal))
                    selectedTheme = Path.GetFileNameWithoutExtension(CustomThemeFileName);

                if (CustomThemeExists(selectedTheme))
                {
                    EnsureCustomThemeTemplateExists();

                    ResourceDictionary defaultDictionary = LoadThemeResource(DefaultThemeResourceName);
                    ResourceDictionary customDictionary = LoadCustomThemeDictionary(selectedTheme);

                    validationMessage = ValidateCustomThemeDictionary(customDictionary);
                    if (!string.IsNullOrWhiteSpace(validationMessage))
                    {
                        ApplyMergedDictionaries(defaultDictionary);
                        appliedTheme = DefaultThemeDisplayName;
                        return (false);
                    }

                    ApplyMergedDictionaries(defaultDictionary, customDictionary);
                    appliedTheme = selectedTheme;
                    return (true);
                }

                validationMessage = $"The theme \"{selectedTheme}\" does not exist.";
                ResourceDictionary fallbackDictionary = LoadThemeResource(DefaultThemeResourceName);
                ApplyMergedDictionaries(fallbackDictionary);
                appliedTheme = DefaultThemeDisplayName;
                return (false);
            }
            catch (Exception ex)
            {
                validationMessage = ex.Message;

                try
                {
                    ResourceDictionary defaultDictionary = LoadThemeResource(DefaultThemeResourceName);
                    ApplyMergedDictionaries(defaultDictionary);
                }
                catch
                {
                }

                appliedTheme = DefaultThemeDisplayName;
                return (false);
            }
        }

        private static bool IsBuiltInTheme(string selectedTheme)
        {
            return (BuiltInThemeDisplayNames.Contains(selectedTheme, StringComparer.Ordinal));
        }

        private static bool CustomThemeExists(string selectedTheme)
        {
            string customThemePath = Path.Combine(CustomThemesDirectory, $"{selectedTheme}.xaml");
            return (File.Exists(customThemePath));
        }

        private static string GetThemeResourceName(string selectedTheme)
        {
            switch (selectedTheme)
            {
                case "Light Theme":
                    return ("LightTheme");
                case "Dark Theme":
                    return ("DarkTheme");
                case DefaultThemeDisplayName:
                default:
                    return (DefaultThemeResourceName);
            }
        }

        private static ResourceDictionary LoadThemeResource(string themeName)
        {
            Uri resourceUri = new Uri(@$"/Utils/Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute);
            return (new ResourceDictionary() { Source = resourceUri });
        }

        private static ResourceDictionary LoadCustomThemeDictionary(string themeName)
        {
            string customThemePath = Path.Combine(CustomThemesDirectory, $"{themeName}.xaml");

            using (FileStream fs = new FileStream(customThemePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                object loadedTheme = XamlReader.Load(fs);
                if (loadedTheme is ResourceDictionary resourceDictionary)
                    return (resourceDictionary);
            }

            throw new InvalidOperationException("The custom theme file could not be loaded as a ResourceDictionary.");
        }

        private static string ValidateCustomThemeDictionary(ResourceDictionary customDictionary)
        {
            foreach (DictionaryEntry entry in customDictionary)
            {
                if (entry.Key is not string key)
                    return ("The custom theme contains a resource key that is not text.");

                if (!AllowedCustomSkinKeys.Contains(key))
                    return ($"The custom theme key \"{key}\" is not allowed.");

                if (!IsAllowedCustomSkinValue(key, entry.Value))
                    return ($"The custom theme key \"{key}\" uses an unsupported resource type.");
            }

            return (string.Empty);
        }

        private static bool IsAllowedCustomSkinValue(string key, object value)
        {
            switch (key)
            {
                case "NoteShadowColor":
                    return (value is Color);
                default:
                    return (value is SolidColorBrush);
            }
        }

        private static void ApplyMergedDictionaries(params ResourceDictionary[] dictionaries)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            foreach (ResourceDictionary dictionary in dictionaries)
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }
    }
}
