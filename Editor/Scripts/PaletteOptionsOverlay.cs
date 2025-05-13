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
        private Vector2 _scrollPos;

        public override VisualElement CreatePanelContent()
        {
            var container = new IMGUIContainer(() =>
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(420), GUILayout.Height(250));

                if (EditorWindow.HasOpenInstances<PaletteWindow>())
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

                    GUILayout.Space(4);
                    PlacementModeManager.ToolbarGUI(tool);
                    GUILayout.Space(2.5f);
                    GUILayout.Label(PlacementModeManager.CurrentType.ToString(), EditorStyles.boldLabel);
                    GUILayout.Space(5f);

                    PlacementModeManager.CurrentMode.SettingsGUI(tool);

                    GUILayout.Space(10);
                    Helpers.DrawLine(Color.grey);
                    GUILayout.EndVertical();
                }
                else
                {
                    if (GUILayout.Button("Open Palette"))
                    {
                        PaletteWindow.OnShowToolWindow(PrefabPaletteTool.Instance);
                        SceneView.RepaintAll(); // Attempt to refresh view
                    }
                }

                GUILayout.EndScrollView();
            });

            return container;
        }
    }
}
#endif
