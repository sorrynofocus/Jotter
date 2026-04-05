# Creating Windows Icons with Greenfish 

This guide documents the process used to generate high-quality Windows `.ico` files for Jotter using **Greenfish Icon Editor Pro**.

## File map 

Jotter-imgs-Fluent_2-dark-mode.png - The SOURCE image used to create the icon. 
> Note: The image was cropped, then copied as a new image. The left side (Fluent logo) was used and dark was not.

Jotter-app-splash-logo.png - This is a splash screen for Jotter. 
jotter-ico-pack.ico - The result from Greenfish editor
hamburger-menu-icon-png-white-11.jpg - The source image for hamburger menu. Other graphics will be used in the future like + and -, but Jotter currently uses CHARACTERS for those. TODO.
orig-gradiant_notebook.ico - The original logo used for Jotter containing the sized icons.


Update files:

`Jotter.csproj` :
```<ApplicationIcon>build\Icos\jotter-ico-pack.ico</ApplicationIcon>```

`MainWindow.xaml`:
```IconSource="pack://application:,,,/build/Icos/jotter-ico-pack.ico"```


Areas that reference the hamburger JPG:

`NoteTemplateEditor.xaml:79` 
```<ImageBrush x:Name="rscHambugerMenu" x:Key="rscHambugerMenu" ImageSource="/build/Icos/hamburger-menu-icon-png-white-11.jpg" />```

and `Utils/Themes/SharedResources.xaml:120`
```<ImageBrush x:Key="rscHambugerMenu" ImageSource="/build/Icos/hamburger-menu-icon-png-white-11.jpg" />```


---

## Tooling

* **Greenfish Icon Editor Pro** (Windows) - https://greenfishsoftware.org/
* Source image: Jotter logo (PNG)

---

##  Workflow

### 1. Prepare the Source Image

* Start with the highest resolution version of the logo
* Crop tightly around the subject (remove excess padding)
* Copy the cropped image

---

### 2. Generate Icon Layers

In Greenfish:

```
Icons → Generate Windows icon from image
```

This creates multiple icon sizes (layers), typically:

* 16×16
* 24×24
* 32×32
* 48×48
* 256×256

Each size is stored as a separate layer inside a single `.ico` file.

---

### 3. Validate Using Built-in Test

Use the test preview:

```
Icon → Test
```

Check the icon against:

* Dark background
* Light background
* Mid-tone backgrounds

Goal:

* Ensure visibility and contrast across all conditions

---

### 4. Optimize the 16×16 Icon (Critical Step)

The 16×16 icon is used in the Windows system tray and requires manual refinement.

#### Adjustments made:

* Simplified overall shape
* Increased size of key elements (pin, note)
* Removed unnecessary detail (backgrounds, extra layers)
* Ensured strong contrast and readability

> !!!  Do NOT rely on auto-scaling for 16×16. Treat it as a separate design.

---

### 5. Save as Multi-Resolution ICO

```
File → Save As → .ico
```

This saves **all icon layers into a single `.ico` file**.

Windows will automatically select the appropriate size depending on context.

---

## To knows...

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

---

## Tip

If the icon does not update in Windows:

```
taskkill /f /im explorer.exe
start explorer.exe
```

---

## 🎉 Outcome

This process produces a polished, production-ready Windows icon set suitable for desktop applications.
