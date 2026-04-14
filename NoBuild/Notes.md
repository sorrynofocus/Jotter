# Notes - Change log

## 2024.02.19

**Jotter.csproj**
- Added resources for hamburger image. Not sure of error. 

```
System.Windows.Markup.XamlParseException: ''Provide value on 'System.Windows.Baml2006.TypeConverterMarkupExtension' threw an exception.' Line number '85' and line position '10'.'

DirectoryNotFoundException: Could not find a part of the path 'C:\Projects\Jotter\bin\x64\Debug\net8.0-windows\build\Icos\hamburger-menu-icon-png-white-11.jpg'.
```

Line:
```
 <ImageBrush x:Name="rscHambugerMenu" x:Key="rscHambugerMenu"  ImageSource="/build/Icos/hamburger-menu-icon-png-white-11.jpg" />
 ```

 Commenting out for now.

**MainWindow.xaml**
- adding button image for settings. no implementation yet.

**MainWindow.xaml.cs**
- Adding logging

**New files**

  ./Utils/Logging/LogHandler.cs
  Logging logic

  ./build/Icos/hamburger-menu-icon-png-white-11.jpg
  Hamburger menu icon

  ---

## 2024.02.19 11:12:21pm

**Jotter.csproj**
Fixed settings button image resource. Instead of content like the app icon, the button is considered a resource.

**MainWindow.xaml**
- Completed the UI for mouse over button. Trying to get a hold of control temapltes. So much to re-do a control.


## 2024.02.20 1:58am
**MainWindow.xaml**
- UI: trying to have notes adjust width automatically with main window.
I may just go with a fixed with on the ListView. I think making the listbox items rounded at the corners maybe in the future. Main, this WPF xaml stuff is time consuming!
The LsitView items also show note data up to a few lines. Future will have a separate window appear to enter the notes. 

**MainWindow.xaml.cs**
- Added a TODO to catch excpetion if the XML data is bad. In my test XML, I've added data and reached that exception.


## 2024.03.24 
- Added NoteManager.cs 
- Added NoteTemplateEditor.xaml and NoteTemplateEditor.xaml.cs

 
[Settings]
https://thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
Things easy as setting height and width of mainwindow cane be done like this:
Application.Current.MainWindow.Height = 800;
- Remember window settings on exit
- Choose themes
- Set number of seconds of auto save (auto [3 seconds], 25, 45 or 60 seconds)
- Set logging file.
- Set logging file debug/info/warn/etc.
- Comfirm deletion of notes
- In settings, show location of save file. Show location of log file.

**Testing**
This was found that the log was writing to the log dir and the dir was not created yet.
I have removed the call to the logger. The logger _should_ try to roll-create dirs first.

Application: Jotter.exe
CoreCLR Version: 8.0.324.11423
.NET Version: 8.0.3
Description: The process was terminated due to an unhandled exception.
Exception Info: System.Windows.Markup.XamlParseException: 'The invocation of the constructor on type 'Jotter.MainWindow' that matches the specified binding constraints threw an exception.' Line number '9' and line position '9'.
 ---> System.IO.DirectoryNotFoundException: Could not find a part of the path 'C:\Users\chris_winters\AppData\Local\Jotter\JotterNotes.log'.
   at Microsoft.Win32.SafeHandles.SafeFileHandle.CreateFile(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
   at Microsoft.Win32.SafeHandles.SafeFileHandle.Open(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
   at System.IO.Strategies.OSFileStreamStrategy..ctor(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
   at System.IO.Strategies.FileStreamHelpers.ChooseStrategyCore(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
   at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
   at com.nobodynoze.flogger.Logger.WriteLog(String sLogMsg, LogDifficultyLvl LogDiffLvl) in C:\Projects\Jotter\Utils\Logging\LogHandler.cs:line 221
   at com.nobodynoze.flogger.Logger.Log(List`1 Message, LogDifficultyLvl LogDiffLvl) in C:\Projects\Jotter\Utils\Logging\LogHandler.cs:line 188
   at com.nobodynoze.flogger.Logger.LogInfo(List`1 lMessage) in C:\Projects\Jotter\Utils\Logging\LogHandler.cs:line 209
   at Jotter.MainWindow.CreateDirectoryFullAccess(String file) in C:\Projects\Jotter\MainWindow.xaml.cs:line 107
   at Jotter.MainWindow..ctor() in C:\Projects\Jotter\MainWindow.xaml.cs:line 60
   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean wrapExceptions)
   --- End of inner exception stack trace ---
   at System.Windows.Markup.XamlReader.RewrapException(Exception e, IXamlLineInfo lineInfo, Uri baseUri)
   at System.Windows.Markup.WpfXamlLoader.Load(XamlReader xamlReader, IXamlObjectWriterFactory writerFactory, Boolean skipJournaledProperties, Object rootObject, XamlObjectWriterSettings settings, Uri baseUri)
   at System.Windows.Markup.WpfXamlLoader.LoadBaml(XamlReader xamlReader, Boolean skipJournaledProperties, Object rootObject, XamlAccessLevel accessLevel, Uri baseUri)
   at System.Windows.Markup.XamlReader.LoadBaml(Stream stream, ParserContext parserContext, Object parent, Boolean closeStream)
   at System.Windows.Application.LoadBamlStreamWithSyncInfo(Stream stream, ParserContext pc)
   at System.Windows.Application.DoStartup()
   at System.Windows.Application.<.ctor>b__1_0(Object unused)
   at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
   at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)
   at System.Windows.Threading.DispatcherOperation.InvokeImpl()
   at MS.Internal.CulturePreservingExecutionContext.CallbackWrapper(Object obj)
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at MS.Internal.CulturePreservingExecutionContext.Run(CulturePreservingExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Windows.Threading.DispatcherOperation.Invoke()
   at System.Windows.Threading.Dispatcher.ProcessQueue()
   at System.Windows.Threading.Dispatcher.WndProcHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)
   at MS.Win32.HwndWrapper.WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)
   at MS.Win32.HwndSubclass.DispatcherCallbackOperation(Object o)
   at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
   at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)
   at System.Windows.Threading.Dispatcher.LegacyInvokeImpl(DispatcherPriority priority, TimeSpan timeout, Delegate method, Object args, Int32 numArgs)
   at MS.Win32.HwndSubclass.SubclassWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
   at MS.Win32.UnsafeNativeMethods.DispatchMessage(MSG& msg)
   at System.Windows.Threading.Dispatcher.PushFrameImpl(DispatcherFrame frame)
   at System.Windows.Application.RunDispatcher(Object ignore)
   at System.Windows.Application.RunInternal(Window window)
   at Jotter.App.Main()


## 2024.03.27 
Tried updating readme with inline base64 image. Looks like github won't support it -  [issue 270](https://github.com/github/markup/issues/270)

## 2024.03.29 
Fixed the README to include a ref to screenshot exhibit. 

Fixes:
- If you open a note more than one time, duplicate note will appear. Added functionality to open the note only one time.
- When closing the application, child windows do not close. Added functionality to close all child windows.


## <a name="2024-03-31"></a> 2024.03.31
- LoadNotes() in NoteManagers.cs needed try catches against invalid XML data files. Created a backup function to back the current data file. 
  User would need to fix the data if they modified it incorrectly. Application will exit after an invalid data file.


## 2024.04.01 23h18m (11:18pm)
- Done! April Fool's!
- Add vertical scrollbars in notes. Main window has it, but not the notes. -COMPLETED
- In the main window (note manager) the Title has a box around it, a side effect from default textbox. (BorderBrush="Transparent") -COMPLETED
- In the note manager, the notes have a harsh font. Change to the Windows 10/11 Segoe UI font at 11pt. -COMPLETED
- Beginning to work on the "search" UI for both Note Manager and the Note templates.
- In version.props, adjusted the <InformationalVersion> to match same value as <FileVersion>. Tested - [ok]


## 2024.04.02 23h48m (11:48pm)
- MainWindow.xaml.cs
Added drag and drop text type files to NoteMAnager. This will enable user to drag and drop files to add a new note. Multiple drag/drop of non-binary/text files are supported.

TitleInMainTextBox_KeyUp() function event needed attention. If user enters a new title in the NoteMAnager, the title get updated. Previously this did not work. This was also challenging as hell -code -wise and trying to figure out where. Dicovered if note was selected, then title can be updated, but if the note wasn't then there's no way of getting the index of the note, because title doesn't select the note. 

- NoteMAnager.cs
For updating the note, additional helper functions where needed to update notes with index look ups. In addition, the textbox datacontext helped out trying to figure out indexes. Had to create a dictionary to keep a list of all the notes Still think there's a better way, but this will work for now. 
There _is_ an issue though.... Updating via note editor will break the title update under the note manager. This is because the notee ditor _also_ needs to update the map. Should really move the map into some sort of helper function to look up and _do_ stuff.

_Here's the TODO_ in the MainWindow.xaml.cs 
```
//TODO hide the dict - make a function to do something with it. 
//This works, but within the mainwindow. 
//Discovered this will not work if:
//- open the note, change title, close note. data changes -fine.
//- change title on main window, press enter. focus doesn't clear and title not updated. 
// -data only updated when closing application
```

Getting late. Tired.


## 2024.04.07 12h10 (12:10am)
- Adding Settings UI
Settings UI has a small transition animation from main screen and back. It's plain at the moment, but it will suffice.
- Slight mods to the NoteManager (MainWindow) UI's scrollbar and NoteManager width. Also, found a neat little trick for smooth scrolling using `VirtualizingPanel.ScrollUnit="Pixel" smooth scrolling`
- `MainWindow.xaml.cs` Decided to open application on the Bottom left side of the screen. Using the applicaiton, I end up moving the app to the left bottom anyway. I've added comments to open in all four corners including custom window positions. Will add these the settings later.

Added `TransitionToSettings()` and `SettingsWindow_Closed` to perform transitionig to settings and back, remembering position of windows if either move. Keepin the main and settings windows follow each other has consistency in postional behavior. 


## 2024.04.08 12h26 (12:26am)
- `MainWindow.xaml` Edited the `NoteManagerSearch` edited the tag and text props.
- `MainWindow.xaml.cs` finally got around to `NoteEditor_NoteUpdated` event (was kept for testing) but now used to handle note updates. TODO is to remove or disable the older methods in updating notes. The subscription is initated in OpenSelectedNote()
- Added search capabilities to the following funcs: `NoteManagerSearch_LostFocus(), NoteManagerSearch_GotFocus(), NoteManagerSearch_KeyUp(), PerformSearch() (new), ClearSearch() (new), UpdateUIWithFilteredNotes() (new)`
- `NoteTemplateEditor.xaml` modded `SaveNoteChanges()` to include event handler to handle note update. If user searches, finds the note, edits it, closes it, the the UI wasn't updated. The note and listview ui needs to be updated.
- Added the mainwindow to open to bottom left side of screen so notes can open in center of screen. Will add options to save position later or to certain parts of screen like top right, top left, etc.



## 2024.11.23 12h26 (06:50pm)
When editing or adding notes, a double linefeed was present in the note (RichEdit control under NoteTemplateEditor).

Additions:
- Added data structure in NoBuilds sub-folder
- Changed auto-build flag from debug to release in auto-build.cmd (build command)

## 2024.11.24 12h26 (02:35am)
- Added support to convert basic HTML to text when dragging and dropping notes in MainWindow_Drop(). New  funcs: IsHtmlFile() and ConvertHTMLToText().
- Modded AddNewNoteWithText() func to add a title used from MainWindow_Drop(). Instead of adding a default text value of "A New Note", the file you drop in will default to the file name as a title.
- Added LogError() in Utils -> Logging -> LogHandler.cs. Did not have an error type log. Added. TODO: NEED to add more logging like warnings, etc.
- Improved build version info. See document Build-info.md


## 2024.12.03 23h02 (11:02pm)
- When entering title update on main window for a note, the note title will be updated but the focus should be moved back to the main window.
- Set default font family to Segoe UI on mainwindow and notetemplate.

## 2024.12.04 23h55 (11:55pm)
- Add date/time stamps to the mainwindow. Date/time is to the left side of the note. 
- Sort notes by created first, descending down. 
- Added new file: Feature-Requests-Known-Issues.md in NoBuilds. Moved the known issues and feature requests there. 
- Cleaned up some comments
Files updated: MainWindow.xaml, MainWindow.xaml.cs, NoteManager.cs

## 2024.12.17 - 6pm
- Adjust mainwindow and notes can stretch


## 2024.12.17 - 8:42pm
- Adding settings
- Add new file: SettingsMgr.cs to handle loading/saving and handling settings data struct. 
- MainWindow implementing prototype settings LoadAppSettings() and SaveAppSettings() to start off with saving/loading Mainwindow position
- Added other settings based off settings.xaml UI. 
- Added new file under NoBuild: DataFile-SettingsMgr.xml that best describes the settings structure. TODO: Configuration needs to be documented.
- Prototype saving/loading window position is working.

## 2024-12-18 11:15
- MAJOR overhaul on UI just for settings and themes! Heavy modification in getting window.resources moved into theme shared resources! 
  With this new work, re-connecting the UI elements to styles was a _pain!_ 
- Able to get dark, light, and default mode working. Added new files under /Utils/Themes:
  DarkTheme.xaml (dark mode theme), DefaultTheme.xaml (default "note" mode theme), LightTheme.xaml (light mode theme), SharedResources.xaml 
  (shared UI element styles on the MainWindow)
- Modded the app.xaml to create resource dictionary. 
- Themes will continue... The Settings Manager UI would need the XAML to be updated. Also need to move SwitchTheme(string themeName)
  to the /utils/themes under its own .cs file. This way the mainwindow, notetemplate, and settings can all use the theme switcher.
- Managed to get the Theme settings to work and save/load theme settings by importing settings manager. Need to import logging manager.
- Moved SwitchTheme() over to settings area. Theme will now change.
- Side thought: May want to move the date/time on the notes heqader from the left towards the right side.

Notes on themes: This was a challenge. First, create the themes: light and dark mode was created. Under properties sheet, for each theme, 
make it _as a resource_ so it can be compiled with the project.
Then, app.xaml was modded to add resource dictionary. Then, function created: SwitchTheme(string themeName) that can switch the themes
out. When creating themes ALL elements must be touched or the binding will not happen correctly. Previously, I thought a few UI elements
could not be touched, or modded, but this is untrue. 
I've discovered if you created a theme and tried to run the application with no interaction, this means an exception happened. 
Try running bare bones debugging by starting to main. Before main hits, the app.xml will be read in. 
When that's read in, you'll see defaultthemes.xaml is read in. Before that whole resource has processed you'll see defaulttheme.xaml
also depends on SharedResources.xaml. Typically a crash will tell you the line number where it failed but you have to trace _which_ 
file it is. In this case, the SharedResources.xaml is where it could happen.

---


## 2024-12-20 12:01
- Themes themes, themes... Just working on adjuting the color to be a bit more modern and easy on eyes.. Trying ot a get a solid 
  default style and then I'll make then for dark and light mode. No coding now.
- https://redketchup.io/color-picker is a _great_ tool site.
- Stopped on tracking down the light blue borderline when mouse hovers over a note. The border is a light blue thin line. 
  Need to track down. I _see_ it at the _txtTitle_ but the _blueish_ color isn't defined. Not sure where it's at right now.

---

## 2024-12-23 -> 24 1:01am
- Working with the ListView.ItemContainerStyle changing the behavior of the border and appearance of being elevated. This was a bit
  difficult to figure out. Still working with the default theme. Once I get it locked in, I'll move to the dark and light themes clean ups.

---

## 2024-12-24 -> 10:21pm
- Still working with themes. Have been moving stuff around to make it more organized. Added better UI mostly for the default theme with better colors 
  and styles. Added elevated 3-d look for the notes to make it more readable. 
- Added function in NoteManager.cs -  SearchContext(). This improves the searching in the mainwindow. 
- Added new functions for the search in NoteTemplateEditor 
- Select all text on notes title editing.
- Added theme for notes editor.
- Stopped on mainwindow private void OpenSelectedNote()) - working on settings of a previously opened note, if possible. We're saving the window pos and dim.All the notes are saved  in the settings manager using the NoteSettings class. 

---

## 2024-12-25 -> 1:08pm -> 11:43pm
- Merry Christmas, but I'm designing. 
- Large changes in SettingsMgr.cs to handle the note settings, individually. 
- WHile working in NoiteManager, it was revealed that removing a note, is done by locating a title. The need to move to NoteID is more suggested.
  Here's the TODO in NoteManager under the RemoveNote() func: TODO -> remove note by ID. Not by title. Create func: RemoveNoteByIdIndex() code 
  implementation example: noteManager.RemoveNoteByIdIndex(idIndexer.Value);
- Tested the new NoteSettings functionality for note POS DIM (position/dimension) configuration.
- Added notify tray icon for application to run in background. Relied on package: https://github.com/hardcodet/wpf-notifyicon
- Added settings for minimize to tray for fully exit
- Added mutex for single instance.

---

## 2024-12-29 -> 2:30am
- If note is selected, there needs to be a visual indicator. Added tag #NoteBlockChromeNoteSelected for default, light, and dark themes. 
  Added data trigger for NoteBlockChrome that if note is selected, color will change based on theme. Kept original tag
  `Tag #SelectedNoteNoNoteBlockChrome` in `defaultheme.xaml`since experimenting with the note selection.
- Cleaning up light and dark themes during mouse overs. 
- Kown "issue": Each opened note does not support themes yet. Main window is the only one. Total burnout!
- After working with themes, I have come to the conclusion that WPF styles in this project is super disorganized. Cleaning up 
  the styles will take a large amount of time. WPF can be a nightmare when it comes to this.
- Added version to settings box
- Fixed bug when starting Jotter to reset last selected theme
- Removed Custom theme from settings. Too difficult for that right now.
- Disabled some UI elements in settings that are not implemented yet.

---

## 2024-12-31 -> 3:05am
- Improved auto-build.cmd to fix hash calc and circleci build for release. 
- automated release notes via templates. 

---

## 2025-01-05 -> 3:475am
- Bug in versioninfo in settings. Still working on, but added logging since the compiled process could not attach for debugging
- small autobuild output changes.

---

## 2025-01-06 -> 7:20am
- Fixing default theme fallback, removing custom theme
- Fixing loading log settings. Saving has not been completed. 
- Connecting auto save intervals to settings. created in settings cb_AutoSaveValChanged() * STOPPED ON NoteTemaplteEditor.cs line 53

---

## 2025-01-011 -> 7:20am
- Continuing from above status. 
- Changing year in statuses to 2025.
- log settings are able to save and switch to new log from settings.
- light testing on loading of settings and saving for logfile.

---

## 2025-03-24 -> 7:19pm
 	- DataPath to class AppSettings() in SettingsMgr.cs. This will be used to store the data path for the notes. However, this will 
      also affect class SettingsMgr() -> SettingsFilePath and NoteSettingsFilePath because if the DataPath changes and the settings 
      reflect it, then SettingsFilePath and NoteSettingsFilePath will need to be updated (_they_ are hardcoded)
      I believe what needs to be done is the following:
        - In the path of Jotter, create a "default Jotter" folder and keep all the settings and notes in there. If the notes change,
        then create a new folder with the new set of notes and its settings. 
        - Have an option to "Open existing Notes" rather than using textbox to just modify the path. A textblock _should_ give a
        status of where the data is. 

---

## 2026-03-18 -> 7:49pm
- Wired the logging toggle end to end.

  The checkbox in Settings.xaml is now active, and Settings.xaml.cs saves/loads `AppSettings.IsKennyLoggings` through the existing `SettingsMgr` XML flow. On startup, MainWindow.xaml.cs now reads that setting and applies it to the shared logger, and also fixed the log-path checks to use `IsNullOrWhiteSpace` so the default path fallback works correctly when no custom path is stored.

  The logger itself now has a real enable switch in Utils/Logging/LogHandler.cs, and [WriteLog](Utils/Logging/LogHandler.cs) returns immediately when logging is off. That means existing `logger.LogInfo(...)` calls can stay as-is, but nothing gets written unless the setting is enabled.

  `dotnet build Jotter.sln` passes. There are still pre-existing nullable warnings in the project, but no new build errors from this change.
  
- Wired `IsDateTimeStamp` through the same settings flow.

  In Settings.xaml I named and enabled the checkbox as `chkDateTimeStamp`, and in Settings.xaml.cs it now loads, saves, and updates `settingsManager.Settings.IsDateTimeStamp` on checked/unchecked.

  On the main window side, the created-date text in MainWindow.xaml now binds its `Visibility` to a window property, and MainWindow.xaml.cs updates that property from `IsDateTimeStamp` during startup and again when Settings closes. So `off` collapses the date, and `on` shows it.

- Applied the deletion-confirmation setting end to end.

  In Settings.xaml the disabled checkbox is now active as `chkDeletionConfirm`, and Settings.xaml.cs now loads, saves, and updates `settingsManager.Settings.IsDeletionConfirm`.

  In MainWindow.xaml.cs `DeleteSelectedNote()` now checks that setting before removing a note. If confirmations are enabled, the app shows a Yes/No prompt; if disabled, deletion proceeds directly.

  `dotnet build Jotter.sln` passes. Existing nullable warnings are still there, but eh.. It'll do.


- Settings window opens at the Main window’s current size and position.

  In MainWindow.xaml.cs, `TransitionToSettings()` now copies the Main window’s current `Width`, `Height`, `Left`, and `Top` onto the Settings window before `ShowDialog()`. If the Main window is maximized, the Settings window now opens maximized too.   That should also let the Settings UI reflow naturally, since the window is larger before the XAML renders. 

  Also, changed  x:Name="TopBorderGrid" ...  HorizontalAlignment="Stretch" >. Previously it had a MaxWidth of 700, but this was causing the settings window to be small and not reflow correctly. 

- Data path settings, implemented the user-data relocation flow.

    `DataPath` still stores the full path to `JotterNotes.xml`, but the app now treats that as the anchor directory for both files. In SettingsMgr.cs, path helpers were added so `JotterNotes.xml` and `JotterNoteSettings.xml` always keep their fixed filenames in the same folder, and `RelocateDataFiles(...)` now creates the target directory if needed, copies both files into it, updates `Settings.DataPath`, and saves settings.

    In the UI, Settings.xaml now has a `Browse...` button at the end of the `User data` textbox, and Settings.xaml.cs now applies user-data path changes on `LostFocus` or browse selection. If the user types a directory, the app uses that directory. If they type a full `.xml` filename, the directory is used but the filename is normalized back to `JotterNotes.xml` so the paired `JotterNoteSettings.xml` stays reliable.

    Updated MainWindow.xaml.cs to read the current notes file path from `SettingsMgr`, so the new location is respected at runtime. 

    Testing...
    1. Open Settings and pick a new folder with `Browse...`. (fix that UI later)
    2. Confirm the textbox becomes `...\JotterNotes.xml`.
    3. Confirm both `JotterNotes.xml` and `JotterNoteSettings.xml` appear in that folder.
    4. Restart Jotter and verify notes still load normally.

    `DataPath` now defaults to `%LOCALAPPDATA%\Jotter`.

    Updated SettingsMgr.cs so a blank `DataPath` is initialized to the Jotter folder instead of the full `JotterNotes.xml` filename. It’s also backward-compatible now: if an older settings file still stores a full `.xml` path, the app will keep honoring it; if it stores a directory, the app resolves `JotterNotes.xml` and `JotterNoteSettings.xml` from that folder.

    Updated Settings.xaml.cs so the `User data` textbox shows the directory path, which matches the folder-browse behavior better.

    Manual check: open `Settings` on a clean/default run and confirm `User data` shows `%LOCALAPPDATA%\Jotter`.

## 2026-03-19 -> 10:07am

- CircleCI build failures. Fix script

  Rid ssh keys and configured CirclCI for automatic deploy keys.

  Three problems in `config.yml`: it was using `cmd.exe` syntax inside a PowerShell CircleCI step, it referenced `build\versioninfo.txt` even though the tracked file is actually `build\VersionInfo.txt`, and it was pushing with a token URL format that can fall back to an interactive auth prompt.

  The commit step now:
   reads the version with PowerShell-native `Get-Content`
   stages the correctly-cased `build\VersionInfo.txt`
   skips commit/push cleanly when nothing is staged
   pushes with `https://x-access-token:$env:GITHUB_TOKEN@github.com/...` to avoid hanging on credentials

## 2026-03-19 -> 11:30pm 

  - Version info finally fixed (well, locally). Fix was make publish also see the resolved version, moving the version-property calculation so it is available to both build and publish. the flow is the same:
  build still increments and writes build\VersionInfo.txt
  both build and publish now read the resolved version back from VersionInfo.txt
  AssemblyVersion, FileVersion, and InformationalVersion are stamped from that resolved value

## 2026-03-20 -> 12:01am

  - Immediate fix for settings displaying version info for application. 
When opening Settings Manager, application crashes. Logs  and event viewer 
points to getting version info. 

## 2026-03-22 -> 2:45pm

- Adding additiona theme components for the dropshadow of the notes. This is was a bit tricky. See   **Drop Shadow Ownership** in \Utils\Themes\Themes-README.md.
  Added a few things under Default,Light, Dark, and baked in code EarlKelly Themes: <Color x:Key="NoteShadowColor">#FF424242</Color> and changed the Color to {DynamicResource NoteShadowColor}. This way, the shadow color can be changed per theme.
  AI played a huge part of this due to a lack of notes in the XAML files (and they are hard to keep notes on)
- Settings.xaml - adjusted size for vertical scrollbar. She was THICK.


## 2026-03-30 -> 2:00am

  - Made MAIN note search more robust by normalizing whitespace and searching the full title or full note body text.
  - Updated main note search to run on `TextChanged` instead of Enter-only.
  - PreviewKeyDown added for note search keyboard shortcuts, `PerformSearch()` and `NoteMatchesSearch()` changed in the main window, and note-search match indexing now uses `searchMatches` plus `currentSearchMatchIndex`.
  - Updated note editor search so highlights persist when the search box loses focus and are only cleared with `Escape` or an empty-search clear action.
  - Added `F3` next-match navigation and `Shift+F3` previous-match navigation for the current highlighted note search results.
  - Improved note editor search placeholder behavior so typing into the search box clears the `Search...` watermark more reliably when returning to the field.
  - Made NOTE editor phrase search more robust by normalizing whitespace across the RichTextBox document before locating highlight ranges, helping queries like multi-word phrases search more like the main window search.
  - Added `Ctrl+F` in the note editor window so keyboard focus jumps directly to the note search textbox. Applied to MAIN window as well. 
  - Kept _existing_ NOTE search behavior intact, including Enter to search, `F3`/`Shift+F3` navigation, and `Escape` to clear.
  - Investigated note editor foreground behavior when opening from the main window. Tried localized activation-focused changes, but behavior remained effectively the same, so the issue is being deferred...

## 2026-03-31 -> 10:22pm

  - Improved circleci to put product at root of artifact directory. Before, you had to navigate through the "build" folder to get to the product.

  - Improved note activation and foreground behavior when opening from the main window.
    The note editor window was not reliably activating and coming to the foreground when opened from the main window, especially with the custom window style. The old method of setting `Topmost` to `true`, calling `Activate()`, and then setting `Topmost` back to `false` was not working consistently.
    The new method uses `Dispatcher.BeginInvoke` to set `Topmost` to `true` on the note editor window, which reliably brings it to the foreground. This is done both when opening a new note and when activating an already open note from the main window.

    Details:
    MainWindow and Notes are custom transparent windows (WindowStyle="None" and AllowsTransparency="True"). These kinds of windows are notoriously bad at responding to normal Activate() calls. WPF often ignores them or they end up behind the main window. This block is a reliable workaround that forces the note window to the foreground.

    Break it down:

    TL;DR: Wait until the current screen update is done, then very quickly flash the window as Topmost, activate it, give it focus, and then turn Topmost back off.

    `window.Dispatcher.BeginInvoke(...)` - This is the most important part. Instead of running the activation code immediately, it queues it to run on the UI thread after the current rendering pass finishes. This tiny delay is what makes it work reliably with transparent/custom windows. 
 
    DispatcherPriority.Render: Tells WPF: "Run this as soon as you’re done rendering the current frame."
    This is higher priority than ApplicationIdle but not so high that it fights with other UI updates. It’s the sweet spot for bringing windows forward.

    The Action() is the code that runs when the dispatcher gets to it. It does the following:
    { 
    window.Topmost = true;
    window.Activate();
    window.Focus();
    window.Topmost = false;
    }
    The last TopMost  = false is important - you don’t want the note to stay permanently on top of all windows (like over your browser or other apps). It only uses Topmost for a split second to steal focus, then behaves normally.

## 2026-04-04 -> 12:22am

  - Added Spotlight search highlights in the note editor. During note editing, if the user performs a search, the matching text in the note will be highlighted. This makes it easier to see where all search matches are in the note content. Also, by selecting text, if any matching text are present, they will be highlighted as well. This is similar to the search behavior in VSCode and other editors, where search matches are highlighted in the text.
  Things get goofy, throw a `return;` in QueueSelectionSpotlightUpdate() (a new helper func) to disable the spotlight search highlights 
  
  - Spotlight search text will be  using a new theme brush `NoteSearchHighlightBrush` for the highlight color. The code is there, but hardcoded to `Brushes.Yellow` until the brush is added to the theme system and defined in each theme.
  Below is the TODO for this:

    TODO: Finish adding NoteSearchHighlightBrush to the theme skinning set for all themes, then remove the Brushes.Yellow fallback. Code already in place in NoteTemplateEditor.xaml.cs to use NoteSearchHighlightBrush for the note search highlights, but the brush needs to be added to the theme system and defined in each theme before it will work.

    Implementations: register the brush as a theme-supported resource, then define it in each theme (the hard part)

    1. Add the brush name to the theme system
      - In ThemeSkinManager.cs, add NoteSearchHighlightBrush with skinnable brush keys listed. Add a short description describing the brush.
    2. Add the brush to shared/default theme resources
      - In SharedResources.xaml, add a safe default: `NoteSearchHighlightBrush` this gives a baseline even if a theme forgets to define it. (during development, chosen color was Yellow for HIGH visibility). Try to keep the color conservative and not bright.
    3. Add the brush to each concrete theme
      - In DefaultTheme.xaml, DarkTheme.xaml, and LightTheme.xaml -- define NoteSearchHighlightBrush with a color that works against that theme’s NoteEditBackgroundBrush and NoteEditForegroundBrush.  


## 2026-04-05 -> 11:58am
  - Added all new icons for the application. Forgot to add notes for 1st pass for 04.04 release notes. This is the 2nd and final pass. Add transparency around the icon to clean it up under light/dark themes under OS.

## 2026-04-07 -> 12:22am
  - Added an "Export Notes" operation to allow users to export their notes in Markdown format.
  Exporting notes can be done by right clicking on a note, individually, and selecting the "Export Note to Markdown" option from the context menu.
  Exporting notes can also be done in bulk by going  to the settings panel and choose export notes in the export card section. Choose a folder  where you want the exported files to be placed and all  the files will be placed there. 

  Decision was made that Markdown export is a good fit for this data model and most users write in markdown already. For many notes, export can be nearly lossless because users are already writing Markdown-ish content. No need a RichTextBox-to-Markdown converter for v1. The exporter should preserve body text as-is and avoid assisting transformations that might damage existing Markdown.

  - Updated UI in Settings panel for drop-down menu and buttons. Buttons and rop menu menus were stock UI elements and have now been updated to match the application’s visual style for a more consistent look and feel.

## 2026-04-11 -> 7:15pm
    - Added functionality to include images in notes! Supported images are jpg, png, tiff, gif, and bmp. To add an image, simply drag and drop the image file into the note editor. The image will be added to the mediastrip at top of note (Microsoft StickyNotes also does something similar). You can also remove images and in MainWindow, there's an image preview to signify the image is present in the note.
    - Small improvement for Spotlight search in note editor for non themed highlight. 
    - PREVIEW: Working on an installer for the application. Still in early stages, but the goal is to have a simple installer that can be used to install Jotter. 
    - Now each note supports the title of the note, rather than an empty windnow in the Windows taskbar.

## 2026-04-12 -> 2:10am
  - The file locking came back as an issue during deletion of images in notes. Noticed that when media gets deleted, then file locks remained and did not get removed in the media folder properly. I've discovered that the root cause is that the main window media preview strip is holding references to the image files in the note media folder. Even after the note media list is updated and the media item is removed from the note, the preview strip still has the image loaded in its `Image` control, which keeps a file handle open. This prevents the file from being deleted from disk until the preview strip releases the image. The fix will need to ensure that the preview strip releases any image references when media is removed from a note so that the underlying file can be deleted cleanly. Difficulty level: high. 
  - Image gallery viewer shows original images but you had to scroll to see the images. If they were large, it would be hard to see. Made adjustments so that the gallery viewer now scales images down to fit within the visible viewport so that large images can be seen without scrolling horizontally or vertically.
  - The Versioning is NOW stable. 
  - Creating a github action to help automate builds. The goal is to see if github actions can provide a similar level of automation for building the application, running tests, and producing artifacts that can be used for releases without having to rely on CircleCI for CI/CD. This will help determine if github actions can be a viable replacement for the current CircleCI setup.

## 2026-04-13 -> 11:27pm

  - A reproducible UI hang was found in the note editor while selecting text and then clicking elsewhere. Issue: https://github.com/sorrynofocus/Jotter/issues/17
  
    Using a debugger and thread dump analysis, it was determined that the hang occurred on the UI thread while the note editor was processing selection change events. Specifically, the call stack showed that `RchEditNote_SelectionChanged(...)` fired, which then called `ClearSelectionSpotlight()` and `ResetSpotlights(...)`. `ResetSpotlights(...)` walks the `RichTextBox` document and modifies formatting, which caused WPF to perform document tree updates while input/selection activity was still in progress. The UI thread became stuck inside WPF's internal `SplayTreeNode` and `TextRangeBase.EndChange(...)` calls, which are part of the document formatting and tree update logic. This confirmed that the hang was caused by selection/highlight maintenance logic blocking the UI thread during document updates.
