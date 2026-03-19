# DevOnboarding.md

# Jotter Developer Onboarding

Welcome to Jotter.

This document is the fast path for contributors who want to understand the project without reverse-engineering the whole codebase on day one.

---

## 1. What Jotter Is

Jotter is a local-first WPF note-taking application built with C# on .NET 8.

It supports:

- note management from a main window
- rich text note editing
- multiple note windows open at the same time
- XML-based storage under `%LOCALAPPDATA%\Jotter`
- theming
- tray support
- search
- drag-and-drop note creation helpers

It does **not** currently depend on cloud services.

---

## 2. The Current Shape of the App

```text
single project
WPF
code-behind
XML persistence
Windows-first
```

This matters because it tells you how to make safe changes:

- do not force MVVM in casual edits
- do not split projects unless the task explicitly calls for it
- do not replace XML just because another format looks shinier

---

## 3. Read These First

Before changing behavior, read:

```text
AGENTS.md
README.md
NoBuild/Notes.md
NoBuild/Feature-Requests-Known-Issues.md
NoBuild/release-notes.md
RepoMap.md
Architecture.md
CodeIntelligenceMap.md
```

These files give you project intent, historical context, and code ownership clues.

---

## 4. Development Environment

Primary environment:

```text
Windows 11
Visual Studio 2022
.NET 8 SDK
```

Shell guidance:

- for Windows examples, prefer `cmd.exe`
- for Linux examples, prefer `bash`
- do not default to PowerShell unless specifically requested

---

## 5. How to Build and Run

Build the solution:

```cmd
dotnet build Jotter.sln
```

Run the project:

```cmd
dotnet run --project Jotter.csproj
```

Other helpful files:

```text
auto-build.cmd - the buildrunner script for Windows
auto-build.sh - the buildrunner script for Linux/macOS (bash)
```

---

## 6. The Most Important Files

### Startup and instance control

```text
App.xaml.cs
```

### Main note manager UI

```text
MainWindow.xaml
MainWindow.xaml.cs
```

### Note model and persistence

```text
NoteManager.cs
```

### Note editor UI and autosave and search behavior

```text
NoteTemplateEditor.xaml
NoteTemplateEditor.xaml.cs
```

### Settings and note window persistence

```text
Settings.xaml
Settings.xaml.cs
SettingsMgr.cs
```

### Logging

```text
Utils/Logging/LogHandler.cs
```

### Themes

```text
Utils/Themes/
```

---

## 7. First Things to Understand in the Code

A new contributor should answer these questions early:

1. where is the note file loaded?
    %LOCALAPPDATA%\Jotter\JotterNotes.xml (default)

2. where is the note file saved?
    %LOCALAPPDATA%\Jotter\JotterNotes.xml (default)

3. how does the editor signal note updates back to the manager
    The NoteTemplateEditor raises events when a note is updated. The MainWindow subscribes to these events and updates the NoteManager accordingly. The NoteManager then handles saving the updated note data back to the XML file on disk.

4. how is autosave triggered
    Time  based saved

5. how are settings loaded and saved
    Settings Manager handles loading and saving settings to %LOCALAPPDATA%\Jotter\JotterSettings.xml by default, using XML serialization. Settings are loaded at app startup and saved when changed or on app exit.

6. how are note window positions persisted
   These are saved as part of the note's XML data under the NoteManager's persistence logic. When a note is opened, its window position is read from the XML and applied to the NoteTemplateEditor instance. When the note is closed or moved, the new position is saved back to the XML file on disk. These are storeds in %LOCALAPPDATA%\Jotter\JotterNotesSettings.xml 

7. how does tray behavior interact with close and minimize
    The tray behavior is controlled in MainWindow.xaml.cs. When the user clicks the close button, the app checks if the "minimize to tray" setting is enabled. If it is, instead of closing, the app minimizes to the system tray. The user can then restore the app by clicking the tray icon. If "minimize to tray" is not enabled, the app will close normally when the close button is clicked. The minimize button always minimizes to the taskbar regardless of settings.

8. how does drag and drop become a new note
    The drag-and-drop behavior is implemented in MainWindow.xaml.cs. When a file is dragged over the main window, the app checks if it's a supported file type (like .txt or .md). If it is, the app shows a visual cue that the file can be dropped. When the file is dropped, the app creates a new note with the content of the file and opens it in a new NoteTemplateEditor window.

If you can answer those, then, you're good to go.

---

## 8. Safe Change Rules

When editing:

- preserve naming
- preserve comments
- preserve logging style
- prefer the smallest correct change
- avoid touching serialization format unless required
- avoid touching tray behavior casually
- avoid changing file paths without approval
- avoid turning one task into a full architecture migration

---

## 9. Manual Smoke Test Checklist

After a change, at minimum verify:

- app launches
- existing notes load
- add note works
- open note works
- edit note title works
- edit note content works
- autosave still works
- delete note still works
- settings window opens
- theme switch works if touched
- tray behavior works if touched

If you changed serialization, also verify:

- old notes still load
- settings still load
- per-note settings still load

---

## 10. Common Risk Zones

Be extra careful in these places:

- `NoteManager.cs` XML serialization logic
- rich text persistence in `NoteTemplateEditor.xaml.cs`
- settings file writes in `SettingsMgr.cs`
- tray and close behavior in `MainWindow.xaml.cs`
- theme switching code in `Settings.xaml.cs` and `MainWindow.xaml.cs`

---

## 11. Good Starter Tasks

Good first contributions include:

- documentation improvements
- readme refreshes
- adding validation comments or guardrails
- improving manual test notes
- small search UX fixes
- export design work

Less safe starter tasks include:

- changing serialization formats
- redesigning the editor
- moving to MVVM
- introducing encryption without a migration plan

---

## 12. Product Direction

The project clearly points toward these future ideas:

- export to text, markdown, and HTML
- optional encryption
- markdown support
- better handling for images in markdown contexts
- eventual portability ideas for Linux and macOS

Treat these as design topics first, not casual drop-in features.

---

## 13. Contributor Mindset

Jotter should remain:

```text
fast
local
practical
understandable
```

When in doubt, choose the path that keeps the app easier to maintain and safer for existing notes.
