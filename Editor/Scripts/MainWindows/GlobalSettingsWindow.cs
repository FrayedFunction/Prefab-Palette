using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    public class GlobalSettingsWindow : EditorWindow
    {
        ToolSettings Settings => ToolContext.Instance.Settings;
        Vector2 scrollPos;

        [MenuItem("Window/Prefab Palette/Settings")]
        public static void Open()
        {
            GetWindow<GlobalSettingsWindow>("Prefab Palette: Settings");
        }

        private void OnEnable()
        {
            Settings.settingsWindowScale.Resolve(Settings.globalMinWindowScale, Settings.globalMaxWindowScale, out Vector2 min, out Vector2 max);
            minSize = min;
            maxSize = max;
        }

        private void OnGUI() 
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("v1.1.2", EditorStyles.miniLabel);
            
            Helpers.TitleText("Prefab Palette: Settings");
            Helpers.DrawLine(Color.gray);

            // Palette Settings
            GUILayout.Label("Palette Overlay:", EditorStyles.whiteLargeLabel);
            Helpers.IndentBlock(1, () =>
            {
                Settings.palette_overlayScale = EditorGUILayout.Vector2Field("Palette Overlay Scale", Settings.palette_overlayScale);
            });

            Helpers.Line(Color.gray);

            // Placer Setttings
            GUILayout.Label("Placer:", EditorStyles.whiteLargeLabel);
            Helpers.IndentBlock(1, () =>
            {
                Settings.placer_includeMask = LayerMaskField("Include Layers", Settings.placer_includeMask);
                Settings.placer_color = EditorGUILayout.ColorField("Color", Settings.placer_color);
                Settings.placer_radius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Radius", Settings.placer_radius));
                Settings.placer_mouseMoveThreshold = (Mathf.Max(0.001f, EditorGUILayout.FloatField("Mouse Move Threshold", Settings.placer_mouseMoveThreshold)));
                Settings.placer_maxRaycastDistance = (Mathf.Max(1f, EditorGUILayout.FloatField("Raycast Distance", Settings.placer_maxRaycastDistance)));
                Settings.placer_3dFallbackHeight = EditorGUILayout.FloatField("Default Floor Height", Settings.placer_3dFallbackHeight);
            });

            Helpers.Line(Color.gray);

            // Overlay Settings
            GUILayout.Label("Mode Settings Overlay:", EditorStyles.whiteLargeLabel);
            Helpers.IndentBlock(1, () =>
            {
                Settings.overlay_autoSize = EditorGUILayout.Toggle("Auto Size?", Settings.overlay_autoSize);
                Settings.overlay_size = Settings.overlay_autoSize ? Vector2.zero : EditorGUILayout.Vector2Field("Size", Settings.overlay_size);
            });

            Helpers.Line(Color.gray);

            // Window Scale
            GUILayout.Label("Window Scale:", EditorStyles.whiteLargeLabel);
            Helpers.IndentBlock(1, () =>
            {
                Settings.globalMinWindowScale = EditorGUILayout.Vector2Field("Global min", Settings.globalMinWindowScale);
                // Only min needs to be clamped to a floor as max floor is already clamped by min.
                Settings.globalMinWindowScale = Vector2.Max(Settings.globalMinWindowScale, Vector2.one);
                Settings.globalMaxWindowScale = EditorGUILayout.Vector2Field("Global max", Settings.globalMaxWindowScale);
                Settings.globalMaxWindowScale = Vector2.Max(Settings.globalMaxWindowScale, Settings.globalMinWindowScale);
                GUILayout.Space(2);
                Helpers.IndentedLabel("Palette", 0, EditorStyles.whiteLabel);
                WindowScaleSettingsGUI.Draw(Settings.paletteWindowScale);
                GUILayout.Space(1);
                Helpers.IndentedLabel("Collections Manager", 0, EditorStyles.whiteLabel);
                WindowScaleSettingsGUI.Draw(Settings.collectionsManagerWindowScale);
                GUILayout.Space(1);
                Helpers.IndentedLabel("Settings", 0, EditorStyles.whiteLabel);
                WindowScaleSettingsGUI.Draw(Settings.settingsWindowScale);
            });

            Helpers.Line(Color.gray);

            // Links
            GUILayout.Label("Links:", EditorStyles.whiteLargeLabel);
            Helpers.IndentBlock(1, () => 
            { 
                Helpers.LinkLabel("GitHub", "https://github.com/FrayedFunction/Prefab-Palette");
                Helpers.LinkLabel("ReadMe", "https://github.com/FrayedFunction/Prefab-Palette/blob/main/Docs/README.md");
                Helpers.LinkLabel("Support", "https://github.com/FrayedFunction/Prefab-Palette/issues");
                Helpers.LinkLabel("License", "https://github.com/FrayedFunction/Prefab-Palette/blob/main/Docs/License.md");
                Helpers.LinkLabel("Contribute", "https://github.com/FrayedFunction/Prefab-Palette/blob/update/global-settings-window/Docs/ContributionsGuide.md");
                Helpers.LinkLabel("Donate", "https://ko-fi.com/frayedfunction");
            });
            Helpers.Line(Color.grey);

            // Contact
            GUILayout.Label("Contact:", EditorStyles.whiteLargeLabel);
            Helpers.IndentBlock(1, () =>
            {
                EditorGUILayout.SelectableLabel(
                    "reach@frayedfunction.com",
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)
                );
            });

            Helpers.Line(Color.gray);

            GUILayout.Space(10);
            GUILayout.EndScrollView();
        }

        private LayerMask LayerMaskField(string label, LayerMask selected)
        {
            // Get all layer names
            string[] layerNames = new string[32];
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                layerNames[i] = string.IsNullOrEmpty(layerName) ? $"Layer {i}" : layerName;
            }

            selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);
            return selected;
        }

    }
}
