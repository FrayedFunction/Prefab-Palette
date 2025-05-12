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
                normal = { textColor = Color.white } // Adjust for dark/light themes as needed
            };

            GUILayout.Space(padding);
            EditorGUILayout.LabelField(text, titleStyle);
            GUILayout.Space(padding);
        }
        public static Vector3 SnapToGrid(Vector3 position)
        {
            // Use unitys built in scene grid
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

