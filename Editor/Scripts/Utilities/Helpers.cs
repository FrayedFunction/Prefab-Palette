using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrefabPalette
{
    public static class Helpers
    {
        public static void TitleText(string text, int fontSize = 20, float padding = 10)
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            GUILayout.Space(padding);
            EditorGUILayout.LabelField(text, titleStyle);
            GUILayout.Space(padding);
        }

        // "Label Grid" should be reworked as a generic to extend it for other types, but it'll do for now.
        public static float CalculateLabelGridWidth(string[] labels, int columns = 2, float padding = 10f)
        {
            GUIStyle labelStyle = GUI.skin.label;
            float maxLabelWidth = 0f;

            foreach (var label in labels)
            {
                Vector2 size = labelStyle.CalcSize(new GUIContent(label));
                if (size.x > maxLabelWidth)
                    maxLabelWidth = size.x;
            }

            return (maxLabelWidth + padding) * columns;
        }

        public static void DrawLabelGrid(string[] labels, int columns = 2, float padding = 10f)
        {
            int total = labels.Length;
            int rows = Mathf.CeilToInt((float)total / columns);

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(1, 1, 1, 1),
                padding = new RectOffset(4, 4, 4, 4),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            float maxLabelWidth = 0f;
            foreach (var label in labels)
            {
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(label));
                if (size.x > maxLabelWidth)
                    maxLabelWidth = size.x;
            }

            float cellWidth = maxLabelWidth + padding;
            float cellHeight = 30f;

            for (int row = 0; row < rows; row++)
            {
                GUILayout.BeginHorizontal();
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < total)
                    {
                        GUILayout.Box(labels[index], boxStyle, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                    }
                    else
                    {
                        GUILayout.Box("", boxStyle, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        public static Vector3 SnapToGrid(Vector3 position)
        {
            float gridSize = UnityEditor.EditorSnapSettings.move.x;
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;

            return position;
        }

        public static T LoadOrCreateAsset<T>(string folderPath, string assetName, out string assetPath) where T : ScriptableObject
        {
            // Find existing asset
            T asset = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .FirstOrDefault();

            if (asset != null)
            {
                assetPath = AssetDatabase.GetAssetPath(asset);
                return asset;
            }

            // Create new asset
            asset = ScriptableObject.CreateInstance<T>();
            assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{assetName}");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static void DrawLine(Color color, int thickness = 1, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        /// <summary>
        /// Ensures correct syntax for compatibility with enums.
        /// </summary>
        public static string SanitiseEnumName(string name)
        {
            // Remove invalid characters & replace spaces with underscores
            name = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");

            // Ensure it doesn't start with a number
            if (char.IsDigit(name[0]))
            {
                name = "_" + name;
            }

            return name;
        }
    }
}

