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
                var tool = ToolContext.Instance;
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(tool.Settings.overlay_size.x), GUILayout.Height(tool.Settings.overlay_size.y));

                if (EditorWindow.HasOpenInstances<PaletteWindow>())
                {
                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10f);
                    SceneInteraction.SnapToGrid = GUILayout.Toggle(
                        SceneInteraction.SnapToGrid,
                        EditorGUIUtility.IconContent("SceneViewSnap").image,
                        "Button",
                        GUILayout.ExpandWidth(true),
                        GUILayout.Height(40)
                    );
                    GUILayout.Space(10f);
                    GUILayout.EndHorizontal();

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
                        PaletteWindow.OnShowToolWindow(ToolContext.Instance);
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
