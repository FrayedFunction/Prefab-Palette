# v1.1.2
* Fix prefab thumbnail flickering issues.
# v1.1.1
* Fix hierarchy tool throwing errors because of calls to obsolete methods in Unitys API. 
# v1.1.0
* Adds support for 2D mode:
  * Control placement depth from the overlay
  * Tool auto switches when editor is in 2D mode.
* Adds new palette overlay GUI.
* Adds thumbnail scale slider to both palette overlay and window.
* Adds per axis rotation selection to settings overlay.
* Adds object instance parenting field to mode settings overlay.
* Supports surface aligned rotation.
* Fixes not being able to place objects in empty space.
  * Fallback height is adjustable via settings window.
* Settings window: Section titles renamed for clarity.
* Add 'Open Palette Overlay' button to mode settings overlay start menu.
* Add hierarchy collapser tool to easily squash cascading hierarchy's with one button press.
* Updated README.

# v1.0.4
* Fixes a bug introduced in v1.0.3 that broke stacking and align with surface behaviour. **v1.0.3 has been withdrawn due to this issue. Please use v1.0.4 or greater**.

# v1.0.3 - *WITHDRAWN*
* *This release was withdrawn due to a bug affecting placement behaviour.
Do not use this version.*

**Tool**
* Placement now respects prefab stages and parents objects correctly.
* Reorganise PaletteOptionsOverlay UI.
* Remove SnapToGrid toggle.
* Optimise scene view raycast for performance.
* Support native grid and rotation angle snapping. 
* Expose mouse move threshold and max raycast distance in ToolSettings
* Add contact and links section to ToolSettings.
* Add indent block and link label helper methods.

**Docs**
* Fix broken links.
* Move repo level docs into new _NoShip folder.
* Update images.
* Tighten feature description.
* Remove references to custom snapping.

# v1.0.2
* Fixed bug generating empty collections when creating collection from folder.
* Improved tool state messaging when creating collections with manager window.
* New shortcut buttons added to options overlay when palette is closed.
* Per window scaling options added to tool settings menu.

# v1.0.1
* Resolved error being thrown by properly queuing calls to Asset Database. 
* Line mode spacing minimum now 0 instead of 1.
* Fixed issue with preexisting collections in folder not being auto recognised.

# v1.0.0
*  Initial Release
* Core features.
    * *Prefab Collections*:  
      Collections of prefabs can be created via the project windows context menu, or from the `Collections Manager Window`.
    
    * *Palette Window*:  
      Window to browse prefab collections and select prefabs via their thumbnails.
    
    * *Scene View Overlay:*  
      Overlay UI for changing global placement settings, switching Placement Modes and customising their settings directly in the Scene View.
    
    * *Visual Placer:*  
      When the tool is active, a target reticle will appear in the scene view, following your mouse to show where the prefab will be placed based on the current mode and settings.
    
    * *Placement Modes:*  
      Swap between unique placement behaviors with their own set of options, all in the scene view overlay.
    
    * *Configurable:*  
      Customise tool-wide settings in the Tool Settings window. Adjust the palettes’ scale, overlay size, placer colour, placer physics layer and more.
    
    * *Undo/Redo Integration:*  
      Instantiated prefabs are integrated into Unitys native Undo/Redo system. 
    
    * *Extendable:*  
      Built with a modular, state-based design, making it easy to add new placement modes. see [Developers.md](./Developers.md) for detailed guidance.

* Modes
    * *Single PrefabMode*
    * *Line Mode*
