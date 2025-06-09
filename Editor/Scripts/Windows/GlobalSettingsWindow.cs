using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace PrefabPalette
{
    public class GlobalSettingsWindow : EditorWindow
    {
        ToolContext tool;

        [MenuItem("Window/Prefab Palette/Settings")]
        public static void OpenWindow()
        {
            var window = GetWindow<GlobalSettingsWindow>("Prefab Palette: Settings");
            window.tool = ToolContext.Instance;
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
            tool.Settings.palette_gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Palette Columns", tool.Settings.palette_gridColumns));
            tool.Settings.palette_minScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Palette Scale", tool.Settings.palette_minScale), 50f, tool.Settings.palette_maxScale);
            tool.Settings.palette_maxScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Palette Scale", tool.Settings.palette_maxScale), tool.Settings.palette_minScale, 500f);
            EditorGUI.indentLevel--;
            
            GUILayout.Space(2);

            // Placer Setttings
            GUILayout.Label("Placer:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            tool.Settings.placer_includeMask = LayerMaskField("Include Layers", tool.Settings.placer_includeMask);
            tool.Settings.placer_color = EditorGUILayout.ColorField("Placer Color", tool.Settings.placer_color);
            tool.Settings.placer_radius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Placer Visual Radius", tool.Settings.placer_radius));
            EditorGUI.indentLevel-- ;

            GUILayout.Space(2);

            // Overlay Settings
            GUILayout.Label("Overlay:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            tool.Settings.overlay_autoSize = EditorGUILayout.Toggle("Auto Size?", tool.Settings.overlay_autoSize);
            tool.Settings.overlay_size = tool.Settings.overlay_autoSize ? Vector2.zero : EditorGUILayout.Vector2Field("Overlay Size", tool.Settings.overlay_size);
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
