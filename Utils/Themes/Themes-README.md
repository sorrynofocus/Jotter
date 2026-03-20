  Theme support ended up becoming two related systems:
  1. Built-in themes compiled into the application under `Utils/Themes`
  2. User-editable custom skins discovered from `%LOCALAPPDATA%\Jotter\CustomThemes`

  The important distinction is that built-in themes are trusted full resource dictionaries, but custom themes are treated more like a "skin" layer. That means custom themes are intentionally restricted to known brush keys only, so users can recolor the app without replacing templates, fonts, layouts, or control behavior.

  **Built-in theme structure**

  Built-in themes remain:
  - `DefaultTheme.xaml`
  - `LightTheme.xaml`
  - `DarkTheme.xaml`
  - `SharedResources.xaml`

  `SharedResources.xaml` continues to hold reusable styles and common control resources. The built-in theme files provide the brush/color values that those controls consume. This keeps theme switching practical because the app does not need separate full UI definitions per theme.

  The application still starts from the merged resource model in `App.xaml`, and built-in theme selection is applied through the theme switching logic used by both `MainWindow.xaml.cs` and `Settings.xaml.cs`.

  **Why custom skins were added**

  The original concern with custom themes was that allowing arbitrary user XAML would easily break the application. Fonts, templates, and random style keys can make WPF windows render incorrectly or become unreadable.

  The better answer was to let users override colors only. So the custom theme system was re-scoped as a "skin" system rather than a full theme replacement system.

  **Where custom skins live**

  Custom skins now live in:

  `%LOCALAPPDATA%\Jotter\CustomThemes`

  This location was chosen because:
  - custom skins survive application updates
  - users can drop in or edit files without touching the install/build output
  - the feature works even for a portable style workflow around local app data

  **How discovery works**

  `ThemeSkinManager.cs` is the core of the custom-skin system.

  The flow is:
  - ensure the custom themes directory exists
  - auto-create a starter file if needed
  - scan the directory for `*.xaml`
  - use each filename (without extension) as a selectable theme name in Settings
  - load the selected custom file dynamically at runtime

  This means a file like:

  `EarlKelly.xaml`

  appears in the Settings theme dropdown as:

  `EarlKelly`

  There is no longer a requirement that a user theme must be named `CustomTheme.xaml`.

  **Why the starter file is auto-created**

  The auto-created starter skin is intentional.

  It serves three purposes:
  - makes the feature discoverable
  - gives the user a valid sample file to edit
  - guarantees there is a known custom themes folder for scanning/open-folder behavior

  The starter file was renamed from `CustomTheme.xaml` to `EarlKelly.xaml` to make it feel more like an example skin rather than a hard-coded required filename.

  **How custom skin loading works**

  A custom skin does not replace the whole app theme.

  Instead the runtime loads:
  1. `DefaultTheme.xaml`
  2. then overlays the custom skin dictionary on top

  That design matters because it guarantees that any brush the custom file does not define will still fall back to a valid built-in value from the default theme.

  In other words:
  - built-in theme = full baseline
  - custom skin = selective override layer

  **Validation rules**

  Validation was required because loose XAML loading is risky.

  `ThemeSkinManager.cs` validates that a custom skin:
  - loads successfully as a `ResourceDictionary`
  - only contains approved keys
  - uses `SolidColorBrush` values for those keys

  If validation fails:
  - the app shows a theme error message
  - the theme falls back to `Default Theme`
  - the saved theme setting is corrected back to the fallback

  This prevents a broken skin file from leaving the application in an unusable visual state.

  **Allowed custom skin keys**

  The allowed keys were built up over time as different windows were wired into the skin system. At this point they include:
  - `WindowBackgroundBrush`
  - `WindowBorderBrush`
  - `MainGridBackgroundBrush`
  - `HeaderBackgroundBrush`
  - `HeaderForegroundBrush`
  - `SearchBoxBackgroundBrush`
  - `SearchBoxForegroundBrush`
  - `ListViewBackgroundBrush`
  - `ListViewForegroundBrush`
  - `ContentBackgroundBrush`
  - `NoteEditBackgroundBrush`
  - `NoteEditForegroundBrush`
  - `SelectedNoteBackgroundBrush`
  - `SelectedNoteBorderBrush`
  - `NoteBlockBackgroundBrush`
  - `NoteBlockBackgroundInnerBrush`
  - `NoteBlockBorderBrush`
  - `NoteDateForegroundBrush`
  - `NoteTitleBackgroundBrush`
  - `NoteTitleForegroundBrush`
  - `NoteTextForegroundBrush`
  - `Button.Static.Background`
  - `Button.Static.Border`
  - `Button.MouseOver.Background`
  - `Button.MouseOver.Border`
  - `Button.Pressed.Background`
  - `Button.Pressed.Border`
  - `Button.Disabled.Background`
  - `Button.Disabled.Border`
  - `Button.Disabled.Foreground`
  - `Button.Static.Background1`
  - `Button.Static.Border1`
  - `Button.MouseOver.Background1`
  - `Button.MouseOver.Border1`
  - `Button.Pressed.Background1`
  - `Button.Pressed.Border1`
  - `Button.Disabled.Background1`
  - `Button.Disabled.Border1`
  - `Button.Disabled.Foreground1`

  These are brush-only on purpose. No styles, no control templates, no fonts, and no layout resources should be allowed in a user skin file.

  **Windows that needed to be wired into theme resources**

  Getting custom skins to "work" was not only about file discovery. The bigger issue was that several windows still had hard-coded colors or local resources that bypassed the theme system.

  The major fixes were:

  - `MainWindow`
    Already mostly theme-driven, but became the baseline for custom skins.

  - `Settings.xaml`
    Had several hard-coded colors, especially on:
    - top header bar
    - text foregrounds
    - textboxes
    - separators
    - toggle visuals
    - radio button text

    These had to be converted to `DynamicResource` brush lookups so the current theme could flow in.

  - `NoteTemplateEditor.xaml`
    This window originally carried local brush resources for buttons and header areas, which effectively overrode the selected theme. Those local brush definitions had to be removed or switched to dynamic resource references.

  - RichEdit body in the note editor
    This was one of the trickiest issues. The note editor window chrome could be themed, but the `RichTextBox` body still looked wrong because the editing surface and text color needed their own dedicated resources.

    That led to:
    - `NoteEditBackgroundBrush`
    - `NoteEditForegroundBrush`

    `NoteEditForegroundBrush` became necessary because the note-card preview text color and the RichEdit body text color are not always the same. A dark theme might want light preview text on cards, while a light editor surface still needs darker editable text to remain readable.

  **Why the editor text color had to be split out**

  Originally, the note editor body reused general note text color assumptions. That caused problems when:
  - the skin used pale editor backgrounds
  - the note preview color looked good in the manager list
  - but the RichEdit body text washed out because the foreground did not match the editing surface

  The fix was to give the editor its own brush key and then apply that brush both:
  - in XAML for the `RichTextBox`
  - in code-behind when loading the document content

  In `NoteTemplateEditor.xaml.cs`, the loaded document text now has its foreground applied using the themed editor brush so existing content is readable immediately after loading.

  **Settings window theme sync bug**

  One subtle bug appeared when reopening Settings after saving a custom skin.

  The issue:
  - the theme was saved correctly
  - the main window reopened with the right theme
  - but Settings reopened showing `Light Theme`
  - opening Settings also caused the app to change to Light Theme unexpectedly

  The cause was startup order in `Settings.xaml.cs`:
  - settings were loaded in the constructor
  - then loaded again in `Window_Loaded`
  - by the second load, `isInitializing` was already false
  - the theme ComboBox change event could fire and overwrite the saved theme

  The fix was:
  - do the real Settings load only in `Window_Loaded`
  - keep `isInitializing = true` during that load
  - set `isInitializing = false` only after the controls finish being populated

  This keeps the ComboBox from saving an unintended selection during window setup.

  **Theme folder UI support**

  The Settings window now includes a small `Open folder` button beside the theme dropdown.

  That button:
  - appears only when the custom themes directory exists
  - opens `%LOCALAPPDATA%\Jotter\CustomThemes` in Explorer

  Since the app auto-creates the custom themes folder, this gives users a direct path into the skin files.

  **Documentation inside the starter skin**

  The starter `EarlKelly.xaml` file was also updated so its comment block explains what each supported brush changes. That was important because otherwise the file is just a list of brushes with no clue which part of the UI they affect.

  The embedded starter template in `ThemeSkinManager.cs` also had to be updated, not just the live file under LocalAppData, because if the file is deleted the app recreates it from the code template.

  **Things learned / pain points**

  1. In WPF, theme support is rarely "just add a new dictionary." Any local hard-coded color or local resource can bypass the theme unexpectedly.
  2. `DynamicResource` matters. If a control is still using hard-coded values or local `StaticResource` brush definitions that shadow the app-level theme keys, custom skins will not flow through.
  3. The editor surface is its own problem. RichEdit text readability must be handled separately from note preview readability.
  4. ComboBox initialization order matters. Theme selection can be accidentally saved during setup if guard flags are not timed correctly.
  5. For user-editable themes, validation is not optional. Bad XAML should always fall back safely.

  **Current outcome**

  The final result is:
  - built-in themes still work
  - custom skins are discovered automatically from `%LOCALAPPDATA%\Jotter\CustomThemes`
  - the starter file is `EarlKelly.xaml`
  - the main window, Settings window, and note editor all honor the active skin
  - the RichEdit body now has its own readable foreground/background brushes
  - invalid custom skins fall back to `Default Theme`
  - the user can open the custom skin folder directly from Settings

  If this feature ever breaks in the future, the first places to inspect are:
  - `Utils/Themes/ThemeSkinManager.cs`
  - `Settings.xaml.cs` startup order and theme ComboBox events
  - hard-coded colors or local brush resources in `Settings.xaml`
  - hard-coded colors or local brush resources in `NoteTemplateEditor.xaml`
  - whether a missing brush key was added to the XAML but not added to the allowed key list in `ThemeSkinManager.cs`

