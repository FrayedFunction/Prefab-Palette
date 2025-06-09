using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public static class PlacementModeManager
    {
        public enum ModeName
        {
            Free,
            Line
        }

        static PlacementModeManager()
        {
            // Add buttons to the toolbar here:
            // NOTE: Mode enum and toolbarButtons must be in the same order.
            toolbarButtons = new GUIContent[]
            {
                new GUIContent(EditorGUIUtility.IconContent("d_MoveTool").image, "Free Mode"),
                new GUIContent(EditorGUIUtility.IconContent($"{PathDr.GetToolPath}/Imgs/LineIcon.png").image, "Line Mode")
            };

            // Hook up the modes class with the mode enum:
            modes = new Dictionary<ModeName, IPlacementMode>()
            {
                { ModeName.Line, new LineDrawMode() },
                { ModeName.Free, new SinglePrefabMode() },
            };

            // Set defualt mode here:
            CurrentModeName = ModeName.Free;
        }

        static GUIContent[] toolbarButtons;
        static Dictionary<ModeName, IPlacementMode> modes;

        public static IPlacementMode CurrentMode
        {
            get
            {
                if (modes != null)
                    return modes[CurrentModeName];

                return null;
            }
        }

        public static ModeName CurrentModeName { get; private set; }

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
