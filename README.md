## Unity Package Author
This package allows you to easily create and manage your package.

### The window


### Usage
##### Creating a package
1. Open the window, by opening the tools menu and clicking the "Package Author" item.
2. Next you open in the Project window, the folder you want to create your package in. (It can be already be filled.)
   - Now when you click on the Package Author it should update accordingly and show you the current folder
3. Select the folders you want to create together with the package.json and enter your author, then click on the Create Package.
    - When you do that the first time on this unity editor, it will ask you for your package prefix (default: "com") and your company name. Both are important for the package idenifier.

The window should now update and show you the package editor. In the Project-window you should see a "package" item show up.

#### Editing a package
1. Open the window, by opening the tools menu and clicking the "Package Author" item.
2. Next you open in the Project window, the folder with your package.json inside, that you want to edit.
   - Now when you click on the Package Author it should update accordingly and show you the contents of the package file.

The editor is a direct representation of the [package manifest](https://docs.unity3d.com/Manual/upm-manifestPkg.html).

Your changes only get applied once you click on the "Apply"-button on the top.

### Weaknesses
- The tool will only work in folder inside the Assets-folder.