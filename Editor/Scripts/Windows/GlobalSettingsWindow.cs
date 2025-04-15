using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace PrefabPalette
{
    public class GlobalSettingsWindow : EditorWindow
    {
        PrefabPaletteTool tool;

        public static void OpenWindow(PrefabPaletteTool tool)
        {
            var window = GetWindow<GlobalSettingsWindow>("Settings");
            window.tool = tool;
        }

        private void OnGUI() {

            // Palette Settings
            GUILayout.Label("Palette Settings");
            EditorGUI.indentLevel++;
            tool.Settings.gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Palette Columns", tool.Settings.gridColumns));
            tool.Settings.minPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Palette Scale", tool.Settings.minPaletteScale), 50f, tool.Settings.maxPaletteScale);
            tool.Settings.maxPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Palette Scale", tool.Settings.maxPaletteScale), tool.Settings.minPaletteScale, 500f);
            EditorGUI.indentLevel--;
            GUILayout.Space(2);

            // Placer Setttings
            GUILayout.Label("Placer Settings");
            EditorGUI.indentLevel++;
            tool.Settings.placerColor = EditorGUILayout.ColorField("Placer Color", tool.Settings.placerColor);
            tool.Settings.placerRadius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Placer Visual Radius", tool.Settings.placerRadius));
            EditorGUI.indentLevel-- ;
        }
    }
}
