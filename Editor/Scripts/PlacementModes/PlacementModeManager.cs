using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Manages the placement mode toolbar and handles mode lifecycle events and transitions.
    /// </summary>
    public static class PlacementModeManager
    {
        static LineModeSettings LineSettings => Helpers.LoadOrCreateAsset<LineModeSettings>(PathDr.GetModeSettingsFolder, "LineModeSettings.asset",  out _);
        static SingleModeSettings SingleSettings => Helpers.LoadOrCreateAsset<SingleModeSettings>(PathDr.GetModeSettingsFolder, "SingleModeSettings.asset",  out _);
        
        public enum ModeName
        {
            Single,
            Line
        }

        static GUIContent[] toolbarButtons;
        static Dictionary<ModeName, IPlacementMode> modes;

        static PlacementModeManager()
        {
            InitialiseToolbarButtons();
            InitialisePlacementModes();

            // Set defualt mode here:
            CurrentModeName = ModeName.Single;
        }

        private static void InitialiseToolbarButtons()
        {
            // Add buttons to the toolbar here:
            // NOTE: ModeName enum and toolbarButtons must be in the same order.
            toolbarButtons = new GUIContent[]
            {
                new GUIContent(EditorGUIUtility.IconContent("d_MoveTool").image, "Single Mode"),
                new GUIContent(Resources.Load<Texture2D>("Imgs/LineIcon"), "Line Mode")
            };
        }

        private static void InitialisePlacementModes()
        {
            // Hook up the modes class with the mode enum:
            modes = new Dictionary<ModeName, IPlacementMode>()
            {
                { ModeName.Line, new LineDrawMode(LineSettings) },
                { ModeName.Single, new SinglePrefabMode(SingleSettings) },
            };
        }

        /// <summary>
        /// Gets the currently active placement mode instance.
        /// </summary>
        public static IPlacementMode CurrentMode
        {
            get
            {
                if (modes != null)
                    return modes[CurrentModeName];

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the currently selected mode.
        /// </summary>
        public static ModeName CurrentModeName { get; private set; }

        /// <summary>
        /// Renders the placement mode toolbar UI and handles mode switching.
        /// Calls OnExit on the old mode and OnEnter on the new mode when the selection changes.
        /// </summary>
        /// <param name="tool">The current tool context passed to mode lifecycle methods.</param>
        public static void ToolbarGUI(ToolContext tool)
        {
            int selectedIndex = (int)CurrentModeName;

            selectedIndex = GUILayout.Toolbar(selectedIndex, toolbarButtons, GUILayout.Height(30));
            ModeName asModeType = (ModeName)selectedIndex;

            if (asModeType != CurrentModeName)
            {
                modes[CurrentModeName].OnExit(tool);
                modes[asModeType].OnEnter(tool);
            }

            CurrentModeName = asModeType;
        }
    }
}
