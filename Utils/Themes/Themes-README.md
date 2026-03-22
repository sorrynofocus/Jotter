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

  **Step-by-step: how to add a UI element to theme skinning**

  Use this workflow any time a control still looks wrong after a theme switch.

  1. Find the real visual owner of the UI
  - In WPF, the thing you want to recolor is often not the `ListView`, `TextBox`, or parent panel itself.
  - For the note cards in `MainWindow.xaml`, the real visual owner is the border named `NoteBlockChrome`.
  - The selected-state visual is not on the border markup itself. It is controlled by a `DataTrigger` inside `DataTemplate.Triggers`.

  2. Identify which properties need to become theme-driven
  - Ask which exact properties define the look:
    - `Background`
    - `BorderBrush`
    - sometimes `Foreground`
    - sometimes `BorderThickness`
  - In the note card case, the important properties were:
    - base state on `NoteBlockChrome`
    - selected state on the trigger for `IsSelected`

  3. Add or reuse brush resource keys
  - Create clear brush names that describe the job, not the current color.
  - For the note card case, the needed keys are:
    - `NoteBlockBackgroundBrush`
    - `NoteBlockBorderBrush`
    - `SelectedNoteBackgroundBrush`
    - `SelectedNoteBorderBrush`
  - If the UI has an inner surface separate from the outer chrome, give it its own key.
  - In this note template that inner surface is the `Grid` using `NoteBlockBackgroundInnerBrush`.

  4. Replace hard-coded values with `DynamicResource`
  - Use `DynamicResource` so runtime theme switching updates the UI immediately.
  - For the note card base chrome, the markup should be:

```xml
<Border x:Name="NoteBlockChrome"
        Background="{DynamicResource NoteBlockBackgroundBrush}"
        BorderBrush="{DynamicResource NoteBlockBorderBrush}"
        BorderThickness="1,1,1,1">
```

  - For the selected state, the trigger should be:

```xml
<DataTemplate.Triggers>
    <DataTrigger Binding="{Binding IsSelected, RelativeSource={RelativeSource AncestorType=ListViewItem}}" Value="True">
        <Setter TargetName="NoteBlockChrome" Property="BorderBrush" Value="{DynamicResource SelectedNoteBorderBrush}" />
        <Setter TargetName="NoteBlockChrome" Property="BorderThickness" Value="2" />
        <Setter TargetName="NoteBlockChrome" Property="Background" Value="{DynamicResource SelectedNoteBackgroundBrush}" />
    </DataTrigger>
</DataTemplate.Triggers>
```

  5. Define the new keys in every built-in theme
  - This step is easy to miss.
  - If a brush key is referenced in XAML, every built-in theme should define it unless you intentionally want fallback behavior.
  - For this note-card case:
    - `DefaultTheme.xaml` already defined the note card brushes.
    - `LightTheme.xaml` and `DarkTheme.xaml` also need the note-card brushes, including `NoteBlockBackgroundInnerBrush`.
  - If one built-in theme forgets a key, the UI may fall back incorrectly or stop matching the rest of the theme.

  6. If custom skins are supported, allow the key there too
  - Jotter supports restricted custom skin files through `ThemeSkinManager.cs`.
  - Any brush that users should be allowed to recolor must be listed in `AllowedCustomSkinKeys`.
  - For the selected-note case, the relevant allowed keys are:
    - `SelectedNoteBackgroundBrush`
    - `SelectedNoteBorderBrush`
    - `NoteBlockBackgroundBrush`
    - `NoteBlockBackgroundInnerBrush`
    - `NoteBlockBorderBrush`
  - If you add a new brush key to XAML but not to that allowlist, custom skins cannot use it.

  7. Update the starter custom skin template when needed
  - If the starter file created by `ThemeSkinManager.cs` should expose the new brush, add it to the template text there too.
  - This matters because deleting the custom theme file causes the app to regenerate it from that embedded template.

  8. Validate with the smallest focused check
  - Launch Jotter.
  - Switch between `Default Theme`, `Light Theme`, and `Dark Theme`.
  - Confirm the note card base background changes.
  - Select a note and confirm the selected background and border change.
  - If custom skins are in play, override the related brush keys in a custom `.xaml` skin and confirm the selected card updates.

  **Case study: selected note styling in `MainWindow.xaml`**

  The selected-note styling is a good example because it has two separate theming surfaces:
  - normal note appearance on `NoteBlockChrome`
  - selected note appearance inside `DataTemplate.Triggers`

  The exact implementation pattern is:
  - base appearance:
    - `Background="{DynamicResource NoteBlockBackgroundBrush}"`
    - `BorderBrush="{DynamicResource NoteBlockBorderBrush}"`
  - selected appearance:
    - `SelectedNoteBorderBrush`
    - `SelectedNoteBackgroundBrush`

  That means the process for future skinning work is:
  - locate the visual owner
  - locate any state triggers changing that owner
  - replace local values with `DynamicResource`
  - define the keys in all built-in themes
  - if custom skins should support it, add the keys to `ThemeSkinManager.cs`
  - test theme switching and state changes together

  **Rule of thumb**

  If a UI element does not respond to theme changes, it is usually one of these:
  - the control still uses a hard-coded color
  - the control uses a local resource that shadows the app theme
  - the state-specific trigger still has a hard-coded value
  - the brush key was added in one theme file but not the others
  - the custom-skin allowlist was not updated

  **Detailed Themes To UI Components**

  This section is the quick reference for how theme keys map to visible UI components in Jotter.

  **Drop Shadow Ownership**

  The note drop shadow is not chosen directly by the `ListView`.

  The ownership chain is:
  - `MainWindow.xaml` defines `MyNotesListView`
  - `MyNotesListView` sets `ListView.ItemContainerStyle`
  - that style is `BasedOn="{StaticResource MyNotesListViewContainerStyle}"`
  - `MyNotesListViewContainerStyle` owns the `Effect` applied to each `ListViewItem`
  - the visible note shadow is therefore controlled by the `ListViewItem` container style and the theme resources it consumes

  Important meaning:
  - the `ListView` itself does not decide the shadow color
  - the note card border does not decide the shadow color
  - the shadow is applied at the `ListViewItem` container style level

  Current stable shadow model:
  - custom skins support `NoteShadowColor`
  - custom skins do not support `NoteShadowOpacity`
  - custom skins do not support `NoteShadowDepth`
  - custom skins do not support `NoteShadowBlurRadius`
  - built-in theme shadow size remains fixed:
  - `ShadowDepth="5"`
  - `BlurRadius="8"`

  Guidance for shadow color:
  - use a slightly lighter tone than the surrounding background
  - make the note card lift visually without looking like a hard glow
  - avoid pure white unless the theme is intentionally stylized
  - a softened border or accent color usually works better than a fully saturated version

  **General Window Shell**

  - `WindowBackgroundBrush`
  - main outer window background
  - affects the base shell behind content areas

  - `WindowBorderBrush`
  - main window border color
  - affects the visible outer edge of the window chrome

  - `MainGridBackgroundBrush`
  - main manager surface inside the window
  - affects the root grid behind the header and content sections

  **Header Area**

  - `HeaderBackgroundBrush`
  - top header bar background
  - affects the title strip that contains add, delete, settings, and exit controls

  - `HeaderForegroundBrush`
  - foreground color for header text and header-oriented content
  - affects the `Jotter` title text and other header foreground usage

  - `SearchBoxBackgroundBrush`
  - intended search box background color for the note manager
  - use when styling the manager search box to match the active theme

  - `SearchBoxForegroundBrush`
  - intended search box text color for the note manager
  - use when styling the search text and placeholder behavior

  **List And Content Surfaces**

  - `ListViewBackgroundBrush`
  - background of the note list area
  - affects the manager note list surface

  - `ListViewForegroundBrush`
  - foreground color for list content where needed
  - affects general list text usage in themed scenarios

  - `ContentBackgroundBrush`
  - background behind the note cards
  - affects the main content region under the `ListView`

  **Note Card Chrome And Body**

```xml
<!-- Outer Border color for note -->
<SolidColorBrush x:Key="NoteBlockBackgroundBrush" Color="#1FF223" />
<!-- Inner "preview note" color for note -->
<SolidColorBrush x:Key="NoteBlockBackgroundInnerBrush" Color="#F21FEE" />

<!-- Inner border color for selected note -->
<SolidColorBrush x:Key="SelectedNoteBackgroundBrush" Color="#EEDFE9" />
<SolidColorBrush x:Key="SelectedNoteBorderBrush" Color="#F3C550" />
<SolidColorBrush x:Key="NoteBlockBorderBrush" Color="#F3C550" />
```

  What each one means:

  - `NoteBlockBackgroundBrush`
  - outer note card surface
  - affects the outer visible card color on `NoteBlockChrome`
  - think of this as the note card shell or chrome

  - `NoteBlockBackgroundInnerBrush`
  - inner note preview body surface
  - affects the inner content region inside the note card
  - think of this as the note body fill

  - `NoteBlockBorderBrush`
  - normal border color for a note card
  - affects the note card border when the note is not selected

  - `SelectedNoteBackgroundBrush`
  - selected-note background color
  - affects the `NoteBlockChrome` background when a note is selected

  - `SelectedNoteBorderBrush`
  - selected-note border color
  - affects the `NoteBlockChrome` border when a note is selected

  Selected note behavior:
  - the normal note card appearance is controlled by:
  - `NoteBlockBackgroundBrush`
  - `NoteBlockBorderBrush`
  - the selected note appearance is controlled by:
  - `SelectedNoteBackgroundBrush`
  - `SelectedNoteBorderBrush`

  Important implementation note:
  - selected note visuals are applied in `DataTemplate.Triggers`
  - the visual owner being changed is `NoteBlockChrome`

  **Note Card Text And Metadata**

  - `NoteDateForegroundBrush`
  - date text color on each note card
  - affects the created-date stamp shown near the top-right of the note card

  - `NoteTitleBackgroundBrush`
  - note title field background
  - affects the title row area inside the note card

  - `NoteTitleForegroundBrush`
  - note title text color
  - affects the note title text shown in the manager

  - `NoteTextForegroundBrush`
  - note preview text color
  - affects the preview body text shown in the note card

  **Note Editor**

  - `NoteEditBackgroundBrush`
  - background of the note editor surface
  - affects the editing area in the note editor window

  - `NoteEditForegroundBrush`
  - foreground color of note editor text
  - affects readable text inside the editor surface

  **Buttons And Shared Controls**

  - `Button.Static.Background`
  - normal button background

  - `Button.Static.Border`
  - normal button border

  - `Button.MouseOver.Background`
  - button background when hovered

  - `Button.MouseOver.Border`
  - button border when hovered

  - `Button.Pressed.Background`
  - button background when pressed

  - `Button.Pressed.Border`
  - button border when pressed

  - `Button.Disabled.Background`
  - button background when disabled

  - `Button.Disabled.Border`
  - button border when disabled

  - `Button.Disabled.Foreground`
  - button text color when disabled

  - `Button.Static.Background1`
  - alternate static button background used by the settings or hamburger button style

  - `Button.Static.Border1`
  - alternate static button border used by the settings or hamburger button style

  - `Button.MouseOver.Background1`
  - alternate hover background used by the settings or hamburger button style

  - `Button.MouseOver.Border1`
  - alternate hover border used by the settings or hamburger button style

  - `Button.Pressed.Background1`
  - alternate pressed background used by the settings or hamburger button style

  - `Button.Pressed.Border1`
  - alternate pressed border used by the settings or hamburger button style

  - `Button.Disabled.Background1`
  - alternate disabled background used by the settings or hamburger button style

  - `Button.Disabled.Border1`
  - alternate disabled border used by the settings or hamburger button style

  - `Button.Disabled.Foreground1`
  - alternate disabled foreground used by the settings or hamburger button style

  **Drop Shadow Key**

  - `NoteShadowColor`
  - color used by the note card drop shadow
  - affects the `DropShadowEffect` used by the note item container style
  - this is a `Color` resource, not a `SolidColorBrush`

  Example:

```xml
<Color x:Key="NoteShadowColor">#BFCCD9</Color>
```

  Good practical rule:
  - derive it from `NoteBlockBorderBrush`
  - make it a little lighter
  - make it a little less saturated

  **Quick Authoring Rules**

  1. If a key is meant for custom skins, it must be allowed by `ThemeSkinManager.cs`
  2. Most custom skin keys are `SolidColorBrush`
  3. `NoteShadowColor` is the exception and must be a `Color`
  4. If a visual is state-based, inspect triggers as well as the base control markup
  5. If a theme change seems to apply only on hover or only when selected, check whether the resting and state-specific visuals are using the same resource path

