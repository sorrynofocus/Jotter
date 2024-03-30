# Jotter
A quick note taking application for those who store useful data. As I chuckle writing this, I suppose the logline can be: _Jot it down, never forget_. 

I **really** liked Microsoft's modern [Sticky Notes][sticky_notes_url] implementation. This is a curt nod keeping a similar look and feel to this development. 

Jotter is an application *currently under development*.  I am learning WPF (very challenging compared to WinForms development) and the work hasn't reached alpha yet. Stay tuned.

---

The concept:

- Clean look
- Very easy to use
- Easy to use back-end database or user friendly data.
- Notes can be managed by a main window (note manager).
- Aimed at adding notes somewhere that can be easily brought back up when needed.
- Ability to _export_ the notes to a text file, markdown, html, etc. 
- A settings area to customize the application and its  functionality.
- Notes can be opened in separate windows (future settings may include sticky notes where notes can be _pinned_ as opened)
- this is a tool Mike will like (personal inside joke with friend)

---

![jotter-alpha-exhibit][jotter-alpha-exhibit-hr6uf]
_Screenshot exhibit to the Jotter application_

Jotter, a brief look:

- A standard _dark_ look with larger than life buttons.
- The main window (note manager at the left-most of the exhibit) contains a listing of the notes. I kept a _sticky notes_ look on each of the notes (yellow). 
- On the main window if you _right-click_ a context menu will appear to open the note or delete it. 
- Double-clicking the note (center and right-most of the exhibit), opens a window to edit the note and title.
- Many notes can be brought up to examine or update in their own separate window.
- You can change the title of the notes in the note manager.
- **NO** cloud migrations, syncing in this development. 
- Notes are stored in XML format under the %LOCALAPPDATA%\Jotter location.

---





[sticky_notes_url]: https://apps.microsoft.com/detail/9nblggh4qghw

[jotter-alpha-exhibit-hr6uf]:
./NoBuild/img/Jotter-alpha-screenshot.png