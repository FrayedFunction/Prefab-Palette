using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    public class GlobalSettingsWindow : EditorWindow
    {
        ToolSettings Settings => ToolContext.Instance.Settings;

        [MenuItem("Window/Prefab Palette/Settings")]
        public static void OpenWindow()
        {
            var window = GetWindow<GlobalSettingsWindow>("Prefab Palette: Settings");
        }

        private void OnEnable()
        {
            minSize = new Vector2(300, 500);
            maxSize = new Vector2(350, 550);
        }

        private void OnGUI() 
        {
            Helpers.TitleText("Prefab Palette: Settings");
            Helpers.DrawLine(Color.gray);

            // Palette Settings
            GUILayout.Label("Palette:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            Settings.palette_gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Columns", Settings.palette_gridColumns));
            Settings.palette_minScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Scale", Settings.palette_minScale), 50f, Settings.palette_maxScale);
            Settings.palette_maxScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Scale", Settings.palette_maxScale), Settings.palette_minScale, 500f);
            EditorGUI.indentLevel--;
            
            GUILayout.Space(2);

            // Placer Setttings
            GUILayout.Label("Placer:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            Settings.placer_includeMask = LayerMaskField("Include Layers", Settings.placer_includeMask);
            Settings.placer_color = EditorGUILayout.ColorField("Color", Settings.placer_color);
            Settings.placer_radius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Radius", Settings.placer_radius));
            EditorGUI.indentLevel-- ;

            GUILayout.Space(2);

            // Overlay Settings
            GUILayout.Label("Overlay:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            Settings.overlay_autoSize = EditorGUILayout.Toggle("Auto Size?", Settings.overlay_autoSize);
            Settings.overlay_size = Settings.overlay_autoSize ? Vector2.zero : EditorGUILayout.Vector2Field("Size", Settings.overlay_size);
            EditorGUI.indentLevel--;
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
