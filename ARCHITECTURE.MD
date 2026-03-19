# Architecture.md

# Jotter Architecture Notes

This document describes the actual runtime structure of the current Jotter repository.

---

## 1. System Overview

Jotter is a single-project WPF desktop application with code-behind, XML persistence, rich text note editing, theme resources, settings persistence, tray integration, and single-instance enforcement.

High-level architecture:

```text
+--------------------------------------------------------------+
|                        Jotter Application                     |
+--------------------------------------------------------------+
|                                                              |
|   +-------------------+         +-------------------------+  |
|   |    MainWindow     |<--->|   NoteTemplateEditor    |  |
|   |   Note Manager    |         |     Note Editor         |  |
|   +---------+---------+         +------------+------------+  |
|             |                                |               |
|             |                                |               |
|             +---------------+----------------+               |
|                             |                                |
|                    +--------v--------+                       |
|                    |   NoteManager   |                       |
|                    | note model/load |                       |
|                    | save/update     |                       |
|                    +--------+--------+                       |
|                             |                                |
|                   +---------v----------+                     |
|                   |    XML Note File   |                     |
|                   | userdata.xml       |                     |
|                   +--------------------+                     |
|                                                              |
|   +-------------------+         +-------------------------+  |
|   |     Settings      |<------->|      SettingsMgr        |  |
|   |   Settings UI     |         | app + note config XML   |  |
|   +---------+---------+         +------------+------------+  |
|             |                                |               |
|             |                                |               |
|             |                  +-------------v-------------+ |
|             |                  | JotterSettings.xml        | |
|             |                  | JotterNoteSettings.xml    | |
|             |                  +---------------------------+ |
|             |                                                |
|   +---------v---------+                                      |
|   | Theme Resources   |                                      |
|   | Utils/Themes      |                                      |
|   +-------------------+                                      |
|                                                              |
|   +-------------------+                                      |
|   | Logger            |                                      |
|   | LogHandler.cs     |                                      |
|   +-------------------+                                      |
|                                                              |
+--------------------------------------------------------------+
```

```MERMAID
graph TD
    A[Jotter Application]
    A --> B[MainWindow]
    A --> C[NoteTemplateEditor]
    B --> D[Note Manager]
    C --> E[Note Editor]
    D --> F[NoteManager]
    F --> G[XML Note File]
    A --> H[Settings]
    A --> I[SettingsMgr]
    H --> J[JotterSettings.xml]
    H --> K[JotterNoteSettings.xml]
    A --> L[Theme Resources]
    A --> M[Logger]
    M --> N[LogHandler.cs]
    ```

---

## 2. Startup Architecture

Startup flow:

```text
App starts
-> App.xaml.cs OnStartup runs
-> mutex is created using current user name
-> if another instance exists, foreground existing app and exit new instance
-> otherwise continue normal startup
```

Single-instance enforcement is therefore part of the application architecture and not optional incidental behavior.

---

## 3. Note Data Architecture

### 3.1 Note Model

The `Note` class lives in `NoteManager.cs` and includes:

```text
IdIndexer
Title
Text
IsDeleted
CreatedDate
```

The note model implements `INotifyPropertyChanged`, which helps the UI stay synchronized with changes.

### 3.2 Note Collection

The application uses:

```text
ObservableCollection<Note>
```

This collection is the binding-friendly in-memory representation of the current note set.

### 3.3 Fast Lookup

`NoteManager` also keeps:

```text
Dictionary<Guid, Note> idIndexerNoteMap
```

This acts as a fast lookup table by note ID.

---

## 4. Notes Persistence Architecture

Notes are stored in:

```text
%LOCALAPPDATA%\Jotter\userdata.xml
```

Reference structure:

```xml
<ArrayOfNote>
  <Note>
    <CreatedDate>...</CreatedDate>
    <IdIndexer>...</IdIndexer>
    <Title>...</Title>
    <Text>...</Text>
    <IsDeleted>false</IsDeleted>
  </Note>
</ArrayOfNote>
```

Persistence characteristics:

- many notes in one XML file
- XML serialization via `XmlSerializer`
- backup creation is present in `NoteManager`
- note save and load is centralized rather than spread everywhere

---

## 5. Settings Architecture

`SettingsMgr.cs` owns two related persistence areas.

### 5.1 Global App Settings

File:

```text
%LOCALAPPDATA%\Jotter\JotterSettings.xml
```

Stores items such as:

```text
window size
window position
save interval
log path
tray behavior
theme
delete confirmation
logging flags
```

### 5.2 Per-Note Window Settings

File:

```text
%LOCALAPPDATA%\Jotter\JotterNoteSettings.xml
```

Stores note-specific window placement and sizing using:

```text
Position
Dimension
NoteSettings
NotesConfiguration
```

---

## 6. Main Window Architecture

`MainWindow.xaml.cs` is a large coordination hub. It currently owns several areas of behavior:

- app shell UI
- note list management
- note creation and deletion
- opening note editors
- updating list view
- searching
- sorting
- drag and drop import
- settings transition
- theme application
- tray icon and minimize behavior

This means `MainWindow.xaml.cs` is both UI logic and application orchestration.

Any edits here should be surgical.

---

## 7. Note Editor Architecture

`NoteTemplateEditor.xaml.cs` owns the per-note editing experience.

It is responsible for:

- loading note title and content
- responding to text changes
- saving note changes
- timer-based autosave
- searching inside note content
- raising note update events

Key design point:

The editor communicates note changes back to the main flow rather than becoming an isolated data island.

---

## 8. Search Architecture

Jotter has two distinct search experiences.

### 8.1 Note Manager Search

Owned by `MainWindow.xaml.cs`.

Purpose:

- search across note list and note content contexts
- update visible manager state

### 8.2 In-Note Search

Owned by `NoteTemplateEditor.xaml.cs`.

Purpose:

- spotlight matching content inside the current note editor

This split means search improvements should preserve the distinction between manager-level and editor-level behavior.

---

## 9. Import Architecture

`MainWindow.xaml.cs` contains drag and drop behavior.

Observed responsibilities:

- drag enter handling
- drag over handling
- drop handling
- binary file detection
- HTML file detection
- HTML-to-text conversion
- creating new notes from imported text

This effectively makes the main window a lightweight import surface.

---

## 10. Theme Architecture

Themes are implemented through WPF resource dictionaries located in `Utils/Themes/`.

Flow:

```text
settings UI selects theme
-> resource dictionaries are swapped
-> fallback to default theme on failure
-> selected theme is saved to settings
```

Theme handling exists in both `Settings.xaml.cs` and `MainWindow.xaml.cs`, so edits should keep them consistent.

---

## 11. Tray Architecture

Jotter includes tray support using `Hardcodet.NotifyIcon.Wpf`.

Main lifecycle concerns:

- whether close means exit or minimize to tray
- restoring the application from the tray
- explicit exit behavior from tray menu

Tray behavior is user-facing and easy to accidentally break. Treat it like glassware.

---

## 12. Logging Architecture

Logging is implemented by a custom `Logger` in `Utils/Logging/LogHandler.cs`.

Characteristics:

- custom info and error methods
- event-driven logging hooks
- file output
- formatting behavior controlled inside the logger

When adding logs, use the current logger style rather than introducing a parallel logging path.

---

## 13. Runtime Flow Diagrams

### 13.1 Load Flow

```text
Application starts
-> Main window initializes settings manager and note manager
-> settings load from XML
-> notes load from userdata.xml
-> note collection is bound and displayed
-> UI becomes interactive
```

### 13.2 Open Note Flow

```text
User selects a note
-> MainWindow resolves selected note
-> NoteTemplateEditor is created for that note
-> note title and content load into controls
-> editor hooks events and autosave timer
```

### 13.3 Save Note Flow

```text
User edits title or content
-> editor updates note model
-> save timer or explicit save path runs
-> NoteManager persists notes XML
-> main list can refresh or update
```

### 13.4 Delete Flow

```text
User requests delete
-> MainWindow or NoteManager removes note
-> notes XML is saved
-> UI list updates
-> related note window state may be cleaned up
```

### 13.5 Settings Flow

```text
User opens settings
-> Settings window loads current AppSettings
-> user changes theme, tray, autosave, or log options
-> SettingsMgr saves XML
-> UI reflects updated state
```

---

## 14. Architectural Constraints

These are current realities, not suggestions:

- code-behind is the current architecture
- XML is the current persistence format
- one project is the current deployment unit
- Windows is the current real target
- note compatibility matters more than elegance refactors

---

## 15. Future-Friendly Boundaries

If the application grows later, a possible future decomposition might be:

```text
Jotter.UI
Jotter.Core
Jotter.Storage
```

But that is a future design option, not current guidance.

The priority today is:

```text
preserve behavior
protect stored data
improve incrementally
```
