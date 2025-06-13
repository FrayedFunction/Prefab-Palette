#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace PrefabPalette
{
    /// <summary>
    /// Scene View overlay that provides UI controls for the tool.
    /// Allows users to toggle grid snapping, select placement modes, and configure mode-specific settings.
    /// </summary>
    [Overlay(typeof(SceneView), "Prefab Palette: Options")]
    public class PaletteOptionsOverlay : Overlay
    {
        private Vector2 _scrollPos;

        /// <summary>
        /// Creates the UI panel content shown in the Scene View overlay.
        /// Displays tool settings, placement mode controls, and provides access
        /// to the Palette Window if it is not already open.
        /// </summary>
        /// <returns>The root <see cref="VisualElement"/> containing the overlay UI.</returns>
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
                    GUILayout.Label(PlacementModeManager.CurrentModeName.ToString(), EditorStyles.boldLabel);
                    GUILayout.Space(2.5f);

                    string[] controls = PlacementModeManager.CurrentMode.ControlsHelpBox;
                    if (controls != null && controls.Length > 0)
                    {
                        if (GUILayout.Button("Controls"))
                        {
                            tool.Settings.overlay_showControlsHelpBox = !tool.Settings.overlay_showControlsHelpBox;
                        }

                        if (tool.Settings.overlay_showControlsHelpBox)
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);

                            float gridWidth = Helpers.CalculateLabelGridWidth(controls, 2);

                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            GUILayout.BeginVertical(GUILayout.Width(gridWidth));
                            Helpers.DrawLabelGrid(controls, 2);
                            GUILayout.EndVertical();

                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();

                            GUILayout.EndVertical();
                        }
                    }
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
                        PaletteWindow.OnShowToolWindow();
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
