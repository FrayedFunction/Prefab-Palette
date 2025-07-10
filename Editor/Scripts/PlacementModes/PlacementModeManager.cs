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
                { ModeName.Line, CreateModeInstance<LineModeSettings, LineDrawMode>("LineModeSettings.asset") },
                { ModeName.Single, CreateModeInstance<SingleModeSettings, SinglePrefabMode>("SingleModeSettings.asset") },
            };
        }

        /// <summary>
        /// Creates an instance of a placement mode using the specified settings asset.
        /// Loads or creates the settings asset of type <typeparamref name="TSettings"/>,
        /// then uses reflection to instantiate a <typeparamref name="TMode"/> that has a
        /// constructor accepting a <see cref="PlacementModeSettings"/>.
        /// Throws <see cref="InvalidOperationException"/> if <typeparamref name="TMode"/> does not have a suitable constructor.
        /// </summary>
        /// <typeparam name="TSettings">The type of settings to load, must inherit from <see cref="PlacementModeSettings"/>.</typeparam>
        /// <typeparam name="TMode">The type of placement mode to instantiate, must implement <see cref="IPlacementMode"/> and have a constructor that accepts a <see cref="PlacementModeSettings"/>.</typeparam>
        /// <param name="settingsAssetName">The name of the settings asset to load or create.</param>
        /// <returns>An instance of <typeparamref name="TMode"/> initialized with the loaded settings.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static TMode CreateModeInstance<TSettings, TMode>(string settingsAssetName)
           where TSettings : PlacementModeSettings
           where TMode : IPlacementMode
        {
            TSettings settings = Helpers.LoadOrCreateAsset<TSettings>(PathDr.GetModeSettingsFolder, settingsAssetName, out _);
            
            var constructor = typeof(TMode).GetConstructor(new[] { typeof(PlacementModeSettings) });

            if (constructor == null)
            {
                throw new InvalidOperationException($"{typeof(TMode)} must have a constructor with one argument of type PlacementModeSettings.");
            }

            return (TMode)constructor.Invoke(new object[] { settings });
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
