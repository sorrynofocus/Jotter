 

### KNOWN ISSUES 
- Re-check timing for note auto-save. If there's no activity, there should be no reason to save or time check.
- ~~Opening a note should flow on top of main window, or to the side. User has to move the main window out of the way or click on note to have focus.~~


---
### TODOs

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
- In NoteManager.cs we added an indexer but the indexer isn't use during CRUD operations. Currently, everything is done by Title look up.
- ~~In NoteManager.cs, LoadNotes() need desperate try/catches~~ - COMPLETED see [2024.03.31](#2024-03-31) 
- ~~In NoteManager.cs, examine TODO in NoteEventArgs -> figure out which save is best and combine. -COMPLETED, saving with indexer~~

---


### FEATURE CREEPS AND TESTING REQUESTS
~~- Add date/time stamps. Can use this to see most recent notes (as an option in settings)~~
- Add tags. When user adds #{Tag} then let's find similar notes
- Add search in notes and ~~outside of notes (main window)~~
- Add coloring bar for notes. This surrounds the notes and sets them apart from others.
- Add a "favorites" button. 
- Add a label "grouping". If notes tend to float toward a specific thing, they can be labeled. This is similar to gmail labels in email.
- Option to either put in taskbar tray or fully exit. Currently, it fully exits. 
~~- In the note manager, over to the far right, top-corner, add a date to note creation.~~
- In the notes, there's no Context menu for copy/cut/paste. 
- In notes, there's a thin border for the window to adjust. A bottom bar should be added for font bold, italics, etc. The botton bar can be used to move the notes around.
~~- [settings] Order Notes by date/time, or by creation date~~
- [search/main window] Case sensitive doesn't find results. you can type for example "dog" to find all related items, but typing "Dog" doesn't produced results.
- [note template] Research image insertion either through tags or by config XML. Previous research tried embedding 64-base encoding. This did not work. 
  Idea:
   - insert the image (create an ID for it, store it in a folder if the ID of the current note.) in the data file, add it to the note's XML tag. Include position (possibly size) where it was on the RichEdit.



