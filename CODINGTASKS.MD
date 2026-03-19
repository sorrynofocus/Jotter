# CodingTasks.md

# Jotter Project Comprehensive Task List

This document tracks implementation status, active gaps, and future work for the Jotter project based on the current repository.

---

## 1. Repository and Environment Setup

- [x] Maintain a single-project WPF solution.
- [x] Target `.NET 8` using `net8.0-windows`.
- [x] Keep Windows as the primary runtime environment.
- [x] Maintain both `auto-build.cmd` and `auto-build.sh` helper scripts.
- [ ] Refresh `README.md` with current build and run instructions.
- [ ] Add a release checklist tied to the current build and version files.

---

## 2. Architecture and Documentation

- [x] Keep a root `AGENTS.md` for AI and contributor guidance.
- [x] Keep development history in `NoBuild/Notes.md`.
- [x] Keep feature requests and issues in `NoBuild/Feature-Requests-Known-Issues.md`.
- [x] Keep release notes in `NoBuild/release-notes.md`.
- [x] Keep XML reference samples in `NoBuild/DataFile-Structure.xml` and `NoBuild/DataFile-SettingsMgr.xml`.
- [ ] Add exact repo documentation files:
  - [ ] `RepoMap.md`
  - [ ] `Architecture.md`
  - [ ] `CodeIntelligenceMap.md`
  - [ ] `DevOnboarding.md`
- [ ] Update docs when core file ownership changes.

---

## 3. Note Model and NoteManager

- [x] Define a `Note` model in `NoteManager.cs`.
- [x] Support `INotifyPropertyChanged` for note properties.
- [x] Persist note identity through `IdIndexer`.
- [x] Store notes in `ObservableCollection<Note>`.
- [x] Maintain an ID-to-note dictionary for fast lookup.
- [x] Load notes from XML.
- [x] Save notes to XML.
- [x] Generate IDs for new notes.
- [x] Support add and update logic.
- [x] Support delete logic.
- [x] Create a backup before overwrite during save.
- [ ] Review soft delete design around `IsDeleted` and confirm intended long-term behavior.
- [ ] Normalize any internal casing inconsistencies only when necessary and with care.
- [ ] Add tests or manual validation docs for `LoadNotes`, `SaveNotes`, and `UpdateNoteByIdIndexer`.

---

## 4. Notes XML Storage

- [x] Store many notes in a single XML file.
- [x] Use `ArrayOfNote` as the XML root for note data.
- [x] Persist note `CreatedDate`, `IdIndexer`, `Title`, `Text`, and `IsDeleted`.
- [x] Keep note data under `%LOCALAPPDATA%\Jotter`.
- [ ] Confirm whether `userdata.xml` should remain the long-term canonical note file name.
- [ ] Add corruption-handling strategy when note XML is unreadable.
- [ ] Add safer recovery messaging for note load failures.
- [ ] Consider temp-file write then replace pattern if not already sufficient.
- [ ] Document exact compatibility guarantees for old note files.

---

## 5. Main Window and Note Manager UI

- [x] Display note list in `MainWindow`.
- [x] Open note editor windows from the manager.
- [x] Add note from the manager.
- [x] Delete note from the manager.
- [x] Support context menu note actions.
- [x] Support double-click to open note.
- [x] Support title editing from the main view.
- [x] Support search in the manager.
- [x] Support sorting behavior.
- [x] Support drag-and-drop input flow.
- [ ] Improve empty-state messaging when no notes exist.
- [ ] Review list virtualization or performance if note count grows large.
- [ ] Review keyboard accessibility and shortcuts in the manager window.
- [ ] Add manual validation checklist for add, delete, open, and search flows.

---

## 6. Note Editor Window

- [x] Open multiple note editor windows at once.
- [x] Load note title into the editor.
- [x] Load note rich text content into the editor.
- [x] Update note content during editor use.
- [x] Update note title during editor use.
- [x] Support autosave timer behavior.
- [x] Support in-note search spotlight behavior.
- [ ] Confirm exact rich text round-trip behavior for edge cases.
- [ ] Validate special characters and encoded content handling.
- [ ] Review editor save timing versus user expectations.
- [ ] Add explicit unsaved-state UX only if it fits the current autosave design.

---

## 7. Settings and Settings Manager

- [x] Define `AppSettings` in `SettingsMgr.cs`.
- [x] Define note window settings via `NoteSettings` and `NotesConfiguration`.
- [x] Load global settings from XML.
- [x] Save global settings to XML.
- [x] Load per-note window settings from XML.
- [x] Save per-note window settings to XML.
- [x] Support autosave interval setting.
- [x] Support theme setting.
- [x] Support tray behavior setting.
- [x] Support log path setting.
- [x] Support deletion confirmation setting.
- [ ] Review the purpose and usage of `DataPath` since notes still appear tied to the current default location.
- [ ] Add stronger validation for user-editable settings values.
- [ ] Document every setting shown in the settings UI.

---

## 8. Theme System

- [x] Maintain theme resource dictionaries.
- [x] Support `DarkTheme`, `LightTheme`, and `DefaultTheme` resources.
- [x] Switch themes from the settings window.
- [x] Fall back to default theme on theme application failure.
- [ ] Review theme naming consistency between saved values and displayed values.
- [ ] Add screenshots or design notes for each theme.
- [ ] Audit theme resources for missing keys or accidental drift.

---

## 9. Logging and Diagnostics

- [x] Maintain custom logger in `Utils/Logging/LogHandler.cs`.
- [x] Support info and error logging.
- [x] Support log event handling.
- [x] Support configurable log file path.
- [ ] Review where logs are most useful during note load and save failures.
- [ ] Consider adding targeted debug logs around drag and drop import and rich text persistence.
- [ ] Document log file expectations and default location behavior.

---

## 10. Search Features

- [x] Support manager-level search in `MainWindow.xaml.cs`.
- [x] Support in-note spotlight search in `NoteTemplateEditor.xaml.cs`.
- [ ] Review search matching rules for case sensitivity and partial matching.
- [ ] Add documentation describing search behavior.
- [ ] Consider search result UX improvements in the manager.

---

## 11. Drag and Drop and Import Behavior

- [x] Support drag enter, over, and drop handlers in the main window.
- [x] Detect binary files.
- [x] Detect HTML files.
- [x] Convert HTML to text for note creation.
- [x] Add new notes from incoming text.
- [ ] Document exactly which file types are supported today.
- [ ] Add validation for large files and malformed HTML.
- [ ] Review import behavior for plain text, HTML, and unsupported types.

---

## 12. Tray and Window Lifecycle

- [x] Support minimize-to-tray behavior.
- [x] Support notify icon initialization.
- [x] Support show application action.
- [x] Support exit application action.
- [x] Persist tray-related settings.
- [ ] Add a manual test checklist for tray behavior on startup, close, and restore.
- [ ] Confirm single-instance behavior plus tray restore flow remain reliable.

---

## 13. Export System

- [ ] Design export ownership.
- [ ] Decide whether export logic lives in `NoteManager.cs` or a new helper or service file.
- [ ] Implement plain text export.
- [ ] Implement markdown export.
- [ ] Implement HTML export.
- [ ] Decide whether export is note-level, selection-level, or all-notes.
- [ ] Decide how rich text maps into markdown and HTML.
- [ ] Add export UI entry points.
- [ ] Add export validation docs or tests.

---

## 14. Markdown Support

- [ ] Decide whether markdown is an export-only format or a true editor mode.
- [ ] Define how markdown coexists with current rich text storage.
- [ ] Plan image support strategy for markdown.
- [ ] Decide whether images are referenced, embedded, copied to an assets folder, or deferred.
- [ ] Prototype a minimal markdown flow without breaking current note files.

---

## 15. Encryption

- [ ] Design optional encryption for note data.
- [ ] Decide whether encryption is whole-file or per-note.
- [ ] Decide on key or passphrase handling.
- [ ] Define compatibility path for old unencrypted notes.
- [ ] Define recovery behavior for wrong passphrase or corrupt encrypted content.
- [ ] Keep encryption design local-first and offline-first.

---

## 16. Reliability and Recovery

- [ ] Improve note XML load failure messaging.
- [ ] Improve settings XML load failure messaging.
- [ ] Add a clearer recovery plan when settings files are malformed.
- [ ] Document backup and restore expectations.
- [ ] Review save atomicity for note and settings writes.

---

## 17. Testing Strategy

- [ ] Add a manual smoke-test document for launch, load, edit, save, and close.
- [ ] Add serialization tests for note XML if test infrastructure is introduced.
- [ ] Add settings serialization tests if test infrastructure is introduced.
- [ ] Add regression validation for theme switching.
- [ ] Add regression validation for tray behavior.
- [ ] Add regression validation for drag and drop import.

---

## 18. Packaging and Release

- [x] Maintain build metadata in `build/Version.props` and `build/VersionInfo.txt`.
- [x] Keep publish profile assets under `Properties/PublishProfiles/`.
- [ ] Document the intended release packaging flow.
- [ ] Decide whether self-contained publish is the default distribution model.
- [ ] Add versioning notes to `README.md` or release docs.

---

## 19. Portability Exploration

- [ ] Keep Linux and macOS portability as a future design track.
- [ ] Do not start portability refactors inside routine bug fixes.
- [ ] If portability is pursued, evaluate a shared core plus alternate UI approach.
- [ ] Research the smallest viable path before rewriting the UI layer.

---

## 20. Human Review Items

These items need explicit human direction before major implementation work:

- whether `userdata.xml` remains the canonical note file name
- whether `DataPath` should become active or remain dormant
- long-term soft delete behavior
- markdown image strategy
- encryption design
- portability plan
- export UX and format priorities
