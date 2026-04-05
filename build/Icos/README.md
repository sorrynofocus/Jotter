# Creating Windows Icons with Greenfish 

This guide documents the process used to generate high-quality Windows `.ico` files for Jotter using **Greenfish Icon Editor Pro**.

## File map 

- `Jotter-imgs-Fluent_2-dark-mode.png` - The SOURCE image used to create the icon. 
> Note: The image was cropped, then copied as a new image. The left side (Fluent logo) was used and dark was not. The new image is  `jotter-ico-logo-source.png` -see below.

- `jotter-ico-logo-source.png` - The cropped version of the original logo that was used to generate the icons. It can be useful for future edits or reference.

- `Jotter-app-splash-logo.png` - This is a splash screen for Jotter. 

- `jotter-ico-pack.ico` - The result from Greenfish editor

- `hamburger-menu-icon-png-white-11.jpg` - The source image for hamburger menu. Other graphics will be used in the future like + and -, but Jotter currently uses CHARACTERS for those. TODO.

- `orig-gradiant_notebook.ico` - The original logo used for Jotter containing the sized icons.


## Update Application Files:

**Jotter.csproj** :
```<ApplicationIcon>build\Icos\jotter-ico-pack.ico</ApplicationIcon>```

**MainWindow.xaml**:
```IconSource="pack://application:,,,/build/Icos/jotter-ico-pack.ico"```


Areas that reference the hamburger JPG:

**NoteTemplateEditor.xaml:79** 
```<ImageBrush x:Name="rscHambugerMenu" x:Key="rscHambugerMenu" ImageSource="/build/Icos/hamburger-menu-icon-png-white-11.jpg" />```

and **Utils/Themes/SharedResources.xaml:120**
```<ImageBrush x:Key="rscHambugerMenu" ImageSource="/build/Icos/hamburger-menu-icon-png-white-11.jpg" />```



## Tooling

* **Greenfish Icon Editor Pro** (Windows) - https://greenfishsoftware.org/
* Source image: Jotter logo (PNG)


##  Workflow

### 1. Prepare the Source Image

* Start with the highest resolution version of the logo. Load `Jotter-imgs-Fluent_2-dark-mode.png` into Greenfish Editor Pro. (Donate! this is a great tool!)
* Using the source file, we will need to create an icon pack with multiple sizes. The source image is large enough to allow for cropping and resizing without losing quality.
* Use rectangular selection to isolate the main logo element. Once selectred, you can use arrows on each of the corners to adjust the cropping area. Add zoom, for precision.
* Once cropped, choose Edit -> Copy, then Edit -> Paste as new document. 
* Transparency needs to be adjusted, so select WAND tool. In the toolbar *left hand side), set the tolerance to 50. Move mouse over the background area. Press and hold SHIFT (you will also see the cursor change with a "+" indicating an ADD selection). Press DELETE to remove the background. 
* Press CONTROL-SHIFT-T or click the beaker icon to TEST. You will see a background with black, grey, white. This will give an idea what the image would look like against a system with dark or light themes. Adjust the tolerance and repeat the background removal if needed.
* If everything looks great, then generate a Windows icon from the image. Choose Icons → Generate Windows icon from image. This will create multiple layers for different sizes. CONTROL-SHIFT-I will also mimic this action. A dialog will appear with the different sizes. You can select which sizes to include in the final .ico file. For Jotter, we will include 16×16, 24×24, 32×32, 48×48, and 256×256. Defaults settings can be used, but verify:
  - 32-bit color depth (this allows for full color and transparency) for all sizes: 16×16, 24×24, 32×32, 48×48, and 256×256.
  - Ensure "Pad with transparency to keep aspect ratio" is checked to maintain the original proportions of the image. 
  - Dither method: Floyd-Steinberg (this is a good default for reducing color depth while maintaining quality).
* Icons will generate each page layer in sizes. Press the beaker icon or CONTROL-SHIFT-T to test the icon. This will show the icon against different backgrounds (dark, light, mid-tone). Verify that the icon is visible and has good contrast in all conditions.
* The image can be further optimized in the 16x16 size. In this case, the "pin" needed sharper red tones. Running TEST again showed that the pin was not visible in the 16×16 size. This is a common issue with small icons, as details can get lost when scaled down. The 16x16 sizes are widely used for system tray icons or TNA applications (Taskbar Notification Area).
* once all has been tested, save the document by File -> Save As. Choose the .ico format. This will save all the layers (sizes) into a single .ico file. Windows will automatically select the appropriate size when displaying the icon in different contexts.
* Finally, saving the source we generated the icons from - a new file created: `jotter-ico-logo-source.png`. This is the cropped version of the original logo that was used to generate the icons. It can be useful for future edits or reference.


## Other

* `.ico` files are containers for multiple image sizes
* Windows dynamically selects the best size
* Small icons (16×16) must be **custom-designed**, not scaled
* Testing across backgrounds is essential for usability

---

## Result

* One `.ico` file
* Multiple embedded resolutions
* Optimized for:

  * System tray (16×16)
  * UI elements (32×32, 48×48)
  * High DPI displays (256×256)

## Tip

If the icon does not update in Windows:

```
taskkill /f /im explorer.exe
start explorer.exe
```

---

