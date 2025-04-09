using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace PrefabPalette
{
    public static class PlacementModeManager
    {
        static GUIContent[] toolbarButtons;
        static Dictionary<ModeType, IPlacementMode> modes;

        public static IPlacementMode CurrentMode
        {
            get
            {
                if (modes != null)
                    return modes[CurrentType];

                return null;
            }
        }

        static PlacementModeManager()
        {
            toolbarButtons = new GUIContent[]
            {
                new GUIContent(EditorGUIUtility.IconContent("d_MoveTool").image, "Free Mode"),
                new GUIContent(EditorGUIUtility.IconContent("SceneViewSnap").image, "Snapping Mode"),
                new GUIContent(EditorGUIUtility.IconContent($"{PathDr.GetToolPath}/Imgs/LineIcon.png").image, "Line Mode")
            };

            modes = new Dictionary<ModeType, IPlacementMode>()
            {
                { ModeType.Line, new LineMode() },
                { ModeType.Free, new PrefabPlacement() },
                { ModeType.Snap, new PrefabPlacement() },
            };

            CurrentType = ModeType.Free;
        }

        public enum ModeType
        {
            Free,
            Snap,
            Line
        }

        public static ModeType CurrentType { get; private set; }

        public static void ToolbarGUI(PrefabPaletteTool tool)
        {
            int selectedIndex = (int)CurrentType;

            selectedIndex = GUILayout.Toolbar(selectedIndex, toolbarButtons, GUILayout.Height(30));
            ModeType asModeType = (ModeType)selectedIndex;

            if (asModeType != CurrentType)
            {
                modes[CurrentType].OnExit(tool);
                modes[asModeType].OnEnter(tool);
            }

            CurrentType = asModeType;
        }
    }
}
