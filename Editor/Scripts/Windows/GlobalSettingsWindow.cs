using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace PrefabPalette
{
    public class GlobalSettingsWindow : EditorWindow
    {
        PrefabPaletteTool tool;

        [MenuItem("Window/Prefab Palette/Settings")]
        public static void OpenWindow()
        {
            var window = GetWindow<GlobalSettingsWindow>("Prefab Palette: Settings");
            window.tool = PrefabPaletteTool.Instance;
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
            tool.Settings.gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Palette Columns", tool.Settings.gridColumns));
            tool.Settings.minPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Palette Scale", tool.Settings.minPaletteScale), 50f, tool.Settings.maxPaletteScale);
            tool.Settings.maxPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Palette Scale", tool.Settings.maxPaletteScale), tool.Settings.minPaletteScale, 500f);
            EditorGUI.indentLevel--;
            
            GUILayout.Space(2);

            // Placer Setttings
            GUILayout.Label("Placer:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            tool.Settings.includeMask = LayerMaskField("Include Layers", tool.Settings.includeMask);
            tool.Settings.placerColor = EditorGUILayout.ColorField("Placer Color", tool.Settings.placerColor);
            tool.Settings.placerRadius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Placer Visual Radius", tool.Settings.placerRadius));
            EditorGUI.indentLevel-- ;

            GUILayout.Space(2);

            // Overlay Settings
            GUILayout.Label("Overlay:", EditorStyles.whiteLargeLabel);
            EditorGUI.indentLevel++;
            tool.Settings.autoOverlaySize = EditorGUILayout.Toggle("Auto Size?", tool.Settings.autoOverlaySize);
            tool.Settings.overlaySize = tool.Settings.autoOverlaySize ? Vector2.zero : EditorGUILayout.Vector2Field("Overlay Size", tool.Settings.overlaySize);
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
