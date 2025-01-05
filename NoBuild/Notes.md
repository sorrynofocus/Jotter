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

## 2024-01-05 -> 3:475am
- Bug in versioninfo in settings. Still working on, but added logging since the compiled process could not attach for debugging
- small autobuild output changes.

---


