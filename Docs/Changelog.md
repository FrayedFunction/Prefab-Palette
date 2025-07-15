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