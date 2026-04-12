![jotter-logo][jotter-logo]

A quick note taking application for those who store useful data. As I chuckle writing this, I suppose the logline can be: _Jot it down, never forget_. 

I **really** liked Microsoft's modern [Sticky Notes][sticky_notes_url] implementation. This is a curt nod keeping a similar look and feel to this development. 

Jotter is an application *currently under development*.  I am learning WPF (very challenging compared to WinForms development) and the work hasn't reached alpha yet. Stay tuned.

---

The concept:

- Clean look
- Very easy to use
- Easy to use back-end database or user friendly data.
- Notes can be managed by a main window (note manager).
- Notes placement on the screen is remembered and multiple notes can be opened at the same time.
- Ability to _export_ the notes to markdown. 
- A settings area to customize the application and its functionality.
- Notes can be opened in separate windows (future settings may include sticky notes where notes can be _pinned_ as opened)


---


_Screen exhibit of the main Jotter window showing the note manager and multiple notes opened for editing_

![jotter-screen001][jotter-screen001]


Jotter, a brief look:

- A standard _dark_ look with larger than life buttons.
- The main window (note manager at the left-most of the exhibit) contains a listing of the notes. I kept a _sticky notes_ look on each of the notes (yellow). Therer are _theme skins_ you can apply! There are _three_ themes currently available to switch between with more on the way!
- On the main window,  _right-clicking_ will produce a context menu that allows you to open the note or delete it. 
- Double-clicking the note (center and right-most of the exhibit), opens a window to edit the note and title.
- Many notes can be brought up to examine or update in their own separate window.
- You can change the title of the notes in the note manager.
- You can import images into notes to visually enhance them.
- You can customize settings to adjust date/time stamp of the notes, logging, and other application behaviors.
- **NO** cloud migrations, syncing in this development. 
- Notes are stored in XML format under the %LOCALAPPDATA%\Jotter location.

<BR>

_Recording of theme switching in Jotter_

![jotter-theme-recording][jotter-theme-recording]

_Screen exhibit to the Jotter application_

![jotter-alpha-exhibit][jotter-alpha-exhibit-hr6uf]


_Screen exhibit showing another view of the Jotter application with image gallery opened within a note_

![jotter-screen002][jotter-screen002]

_Screen exhibit of the settings manager_

![jotter-screen003][jotter-screen003]


---

## Build Jotter
To build Jotter from source, ensure you have the following prerequisites installed:

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload.
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or later.

Once the prerequisites are installed:

Clone the repository:
   ```bash
   git clone https://github.com/sorrynofocus/Jotter.git
   cd Jotter
   ```

`auto-build.cmd` will build everything- dedbug and release and packages for publishing.

To quickly build the project without opening Visual Studio 2022 and output build will be placed in the default `bin` and `obj` folders under each project directory, enter:
   ```bash
   dotnet build Jotter.sln
   ```

---




[sticky_notes_url]: https://apps.microsoft.com/detail/9nblggh4qghw

[jotter-alpha-exhibit-hr6uf]:
./NoBuild/img/Jotter-alpha-screenshot.png

[jotter-logo]:
./build/icos/Jotter-logo.png

[jotter-screen001]:
./NoBuild/img/Jotter-screen-001.png

[jotter-screen002]:
./NoBuild/img/Jotter-screen-002.png

[jotter-screen003]:
./NoBuild/img/Jotter-screen-003.png

[jotter-theme-recording]:
./NoBuild/img/Jotter-theme-Recording.gif
