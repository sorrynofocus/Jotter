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

- TODO:
[Universal]
- Find all TODOs in code, complete those.
- Clean up code. It's messy.
- Move all subscriptions to another file
- move all utiltiies to another file
- move all data types; classes, data, etc to another file. 
- Add a "settings" to the application. See [Settings] below.
- Change the buttons from text-based ("+" is for add, for example) to image based
- Test the icons. Icon test in explorer is good. Create final icon in future releases
- more comments in code. For beginners looking into this, it would help greatly. 
  
 
 
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

TODO 
- Examine previous TODOs. Mark them complete or turn into issues.
- In NoteManager.cs we added an indexer but the indexer isn't use during CRUD operations. Currently, everything is done by Title look up.
- In NoteManager.cs, LoadNotes() need desperate try/catches - COMPLETED see [2024.03.31](#2024-03-31) 
- In NoteManager.cs, examine TODO in NoteEventArgs -> figure out which save is best and combine. -COMPLETED, saving with indexer


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




--- 

### BUG AND ANNOYANCES REPORT
- Re-check timing for note auto-save. If there's no activity, there should be no reason to save or time check.
- Opening a note should flow on top of main window, or to the side. User has to move the main window out of the way or click on note to have focus.

### FEATURE CREEPS AND TESTING REQUESTS
- Add date/time stamps. Can use this to see most recent notes (as an option in settings)
- Add tags. When user adds #{Tag} then let's find similar notes
- Add search in notes and outside of notes (main window)
- Add coloring bar for notes. This surrounds the notes and sets them apart from others.
- Add a "favorites" button. 
- Add a label "grouping". If notes tend to float toward a specific thing, they can be labeled. This is similar to gmail labels in email.
- Option to either put in taskbar tray or fully exit. Currently, it fully exits. 
- In the note manager, over to the far right, top-corner, add a date to note creation.
- In the notes, there's no Context menu for copy/cut/paste. 
- On notes, pressing enter may give double return lines.
- In notes, there's a thin border for the window to adjust. A bottom bar should be added for font bold, italics, etc. The botton bar can be used to move the notes around.


