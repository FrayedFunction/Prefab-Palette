#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace PrefabPalette
{
    [Overlay(typeof(SceneView), "Prefab Palette: Options")]
    public class PaletteOptionsOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var container = new IMGUIContainer(() =>
            {
                var tool = PrefabPaletteTool.Instance;

                GUILayout.BeginVertical();
                
                SceneInteraction.SnapToGrid = GUILayout.Toggle(
                    SceneInteraction.SnapToGrid,
                    EditorGUIUtility.IconContent("SceneViewSnap").image,
                    "Button",
                    GUILayout.Width(400),
                    GUILayout.Height(40)
                );
                Helpers.DrawLine(Color.grey);
                // Placement mode
                GUILayout.Space(4);
                PlacementModeManager.ToolbarGUI(tool);
                GUILayout.Space(2.5f);
                GUILayout.Label(PlacementModeManager.CurrentType.ToString(), EditorStyles.boldLabel);
                GUILayout.Space(5f);
                Helpers.DrawLine(Color.grey);
                PlacementModeManager.CurrentMode.SettingsGUI(tool);
                GUILayout.Space(10);
                Helpers.DrawLine(Color.grey);
                GUILayout.EndVertical();
            });

            return container;
        }
    }
}
#endif
