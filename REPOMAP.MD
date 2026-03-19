# RepoMap.md

# Jotter Repository Map

This file is an exact-minded repository guide for contributors and AI agents working in Jotter.

---

## 1. Top-Level Repository Layout

```text
Jotter-NT/
|
|-- .circleci/
|   `-- config.yml
|
|-- AGENTS.MD
|-- README.md
|-- Jotter.sln
|-- Jotter.csproj
|-- Directory.Build.props
|-- Jotter.csproj.user
|-- auto-build.cmd
|-- auto-build.sh
|
|-- App.xaml
|-- App.xaml.cs
|-- MainWindow.xaml
|-- MainWindow.xaml.cs
|-- NoteManager.cs
|-- NoteTemplateEditor.xaml
|-- NoteTemplateEditor.xaml.cs
|-- Settings.xaml
|-- Settings.xaml.cs
|-- SettingsMgr.cs
|
|-- Utils/
|   |-- Logging/
|   |   `-- LogHandler.cs
|   `-- Themes/
|       |-- DarkTheme.xaml
|       |-- DefaultTheme.xaml
|       |-- LightTheme.xaml
|       `-- SharedResources.xaml
|
|-- build/
|   |-- build-info.md
|   |-- Version.props
|   |-- VersionInfo.txt
|   `-- Icos/
|       |-- gradiant_notebook.ico
|       `-- hamburger-menu-icon-png-white-11.jpg
|
|-- NoBuild/
|   |-- DataFile-SettingsMgr.xml
|   |-- DataFile-Structure.xml
|   |-- Feature-Requests-Known-Issues.md
|   |-- Notes.md
|   |-- release-notes.md
|   |-- img/
|   |   `-- Jotter-alpha-screenshot.png
|   `-- sandbox/
|       `-- Window-sandbox.xaml
|
`-- Properties/
    |-- Resources.Designer.cs
    |-- Resources.resx
    `-- PublishProfiles/
        `-- FolderSelfContained.pubxml
```

---

## 2. File Ownership by Responsibility

### Application Startup

Files:

```text
App.xaml
App.xaml.cs
```

Ownership:

- app entry behavior
- single-instance enforcement
- foreground activation of existing instance

### Main Note Manager Window

Files:

```text
MainWindow.xaml
MainWindow.xaml.cs
```

Ownership:

- primary note list UI
- search in manager
- create, open, delete note flows
- drag and drop import
- settings transition
- tray icon behavior
- sorting and selection behavior

### Note Model and Persistence

File:

```text
NoteManager.cs
```

Ownership:

- `Note` model
- note collection management
- XML load and save
- note ID indexing
- note updates and search helpers
- backup creation
- note-related event args

### Note Editor

Files:

```text
NoteTemplateEditor.xaml
NoteTemplateEditor.xaml.cs
```

Ownership:

- note content editing
- note title editing
- autosave timer behavior
- note update events
- in-note search spotlighting

### Settings UI and Persistence

Files:

```text
Settings.xaml
Settings.xaml.cs
SettingsMgr.cs
```

Ownership:

- settings window UI
- app settings serialization
- per-note window settings serialization
- theme selection and persistence
- tray and autosave preferences

### Logging

File:

```text
Utils/Logging/LogHandler.cs
```

Ownership:

- custom logger class
- log file writing
- log events
- info and error formatting

### Theme Resources

Files:

```text
Utils/Themes/DarkTheme.xaml
Utils/Themes/DefaultTheme.xaml
Utils/Themes/LightTheme.xaml
Utils/Themes/SharedResources.xaml
```

Ownership:

- visual theme dictionaries
- reusable app visual resources

### Build and Version Metadata

Files:

```text
build/Version.props
build/VersionInfo.txt
build/build-info.md
auto-build.cmd
auto-build.sh
Properties/PublishProfiles/FolderSelfContained.pubxml
```

Ownership:

- build metadata
- icon assets
- publish configuration
- local build helpers

### Historical Notes and Samples

Files:

```text
NoBuild/Notes.md
NoBuild/Feature-Requests-Known-Issues.md
NoBuild/release-notes.md
NoBuild/DataFile-Structure.xml
NoBuild/DataFile-SettingsMgr.xml
```

Ownership:

- development history
- backlog and issue notes
- release chronology
- XML reference samples

---

## 3. Key Types and Their Home Files

### `Note`

Defined in:

```text
NoteManager.cs
```

Key fields and properties:

```text
IdIndexer
Title
Text
IsDeleted
CreatedDate
```

### `NoteManager`

Defined in:

```text
NoteManager.cs
```

Key responsibilities:

```text
LoadNotes
AddUpdateNote
RemoveNote
SaveNotes
GenerateID
UpdateNoteByIdIndexer
SearchContext
```

### `NoteEventArgs`

Defined in:

```text
NoteManager.cs
```

Used for:

- editor-to-main-window note update notifications

### `AppSettings`

Defined in:

```text
SettingsMgr.cs
```

### `NoteSettings`

Defined in:

```text
SettingsMgr.cs
```

### `SettingsMgr`

Defined in:

```text
SettingsMgr.cs
```

### `Logger`

Defined in:

```text
Utils/Logging/LogHandler.cs
```

---

## 4. Important Hard-Wired Paths and Names

### Notes File

Application note path:

```text
%LOCALAPPDATA%\Jotter\userdata.xml
```

### Settings File

Global settings path:

```text
%LOCALAPPDATA%\Jotter\JotterSettings.xml
```

### Per-Note Settings File

Per-note settings path:

```text
%LOCALAPPDATA%\Jotter\JotterNoteSettings.xml
```

### Theme Resource Path Pattern

```text
/Utils/Themes/{themeName}.xaml
```

---

## 5. Search Guide for New Contributors

If you need to find where something happens, start here.

### Find note loading and saving

Look in:

```text
NoteManager.cs
MainWindow.xaml.cs
```

### Find note editing behavior

Look in:

```text
NoteTemplateEditor.xaml.cs
```

### Find settings load and save behavior

Look in:

```text
SettingsMgr.cs
Settings.xaml.cs
MainWindow.xaml.cs
```

### Find theme switching behavior

Look in:

```text
Settings.xaml.cs
MainWindow.xaml.cs
Utils/Themes/
```

### Find tray behavior

Look in:

```text
MainWindow.xaml.cs
Jotter.csproj
```

### Find logging behavior

Look in:

```text
Utils/Logging/LogHandler.cs
MainWindow.xaml.cs
NoteManager.cs
Settings.xaml.cs
```

### Find import and drag-drop behavior

Look in:

```text
MainWindow.xaml.cs
```

---

## 6. Areas That Should Be Touched Carefully

These parts are high-risk because they affect user data or core UX:

- note XML serialization
- settings XML serialization
- tray close and minimize behavior
- theme loading and fallback
- rich text save and load behavior
- note identity handling via `IdIndexer`
- drag and drop import path

---

## 7. Areas That Are Good First Tasks

These are safer places for contributors to start:

- documentation updates
- small settings UI improvements
- search UX improvements
- export design notes
- non-invasive logging additions
- readme cleanup

---

## 8. Update Policy

Whenever the repo structure changes in a meaningful way, update this map so future contributors do not have to spelunk through the project like cave divers with flashlights.

