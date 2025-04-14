using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Maintains valid paths to the tools root and collections folder, Saves current path to Editor Prefs.
    /// </summary>
    [InitializeOnLoad]
    public static class PathDr
    {
        // Keys for editor prefs
        private const string ToolPathKey = "PrefabPalette/PathDr_ToolPath";
        private const string CollectionsPathKey = "PrefabPalette/PathDr_CollectionsPath";

        private static string toolPath;
        private static string generatedFolderPath;
        private static string collectionsPath;

        static PathDr()
        {
            Init();
        }

        static void Init()
        {
            toolPath = EditorPrefs.GetString(ToolPathKey, string.Empty);
            collectionsPath = EditorPrefs.GetString(CollectionsPathKey, string.Empty);

            // If the path for the tools root folder isn't set or the directory no longer
            // exists, look for it in th asset database.
            if (string.IsNullOrEmpty(toolPath) || !Directory.Exists(toolPath))
            {
                var root = FindFolder("PrefabPalette");
                toolPath = Path.Combine(root, "Editor");
                // If the path's found, save it to editor prefs.
                if (!string.IsNullOrEmpty(toolPath))
                {
                    EditorPrefs.SetString(ToolPathKey, toolPath);
                }
                else
                {
                    Debug.LogError($"PrefabPalette/{nameof(PathDr)}Can't find editor folder");
                }
            }

            generatedFolderPath = Path.Combine(GetToolPath, "Generated");

            if (!Directory.Exists(generatedFolderPath))
            {
                string newGUID = AssetDatabase.CreateFolder(GetToolPath, "Generated");

                if (string.IsNullOrEmpty(newGUID))
                {
                    // Creation failed
                    Debug.LogError($"PrefabPalette/{nameof(PathDr)}: Failed to create Generated folder!");
                    return;
                }

                // Folder Created
                generatedFolderPath = AssetDatabase.GUIDToAssetPath(newGUID);

                Debug.Log($"PrefabPalette/{nameof(PathDr)}: Collections folder created successfully at {generatedFolderPath}. Refreshing AssetDatabase...");
                AssetDatabase.Refresh();
            }

            collectionsPath = Path.Combine(GetGeneratedFolderPath, "Collections");

            if (!Directory.Exists(collectionsPath))
            {
                string newGUID = AssetDatabase.CreateFolder(GetGeneratedFolderPath, "Collections");

                if (string.IsNullOrEmpty(newGUID))
                {
                    Debug.LogError($"PrefabPalette/{nameof(PathDr)}: Failed to create Collections folder!");
                    return;
                }

                // Folder Created
                collectionsPath = AssetDatabase.GUIDToAssetPath(newGUID);

                Debug.Log($"PrefabPalette/{nameof(PathDr)}: Collections folder created successfully at {collectionsPath}. Refreshing AssetDatabase...");
                AssetDatabase.Refresh();
            }
        }

        /// <returns>
        /// Path to /PrefabPalette/Editor
        /// </returns>
        public static string GetToolPath => toolPath;

        public static string GetGeneratedFolderPath => generatedFolderPath;

        /// <summary>
        /// Returns the path to the folder where prefab collections are generated.
        /// </summary>
        /// <remarks>
        /// Note: Always assumed to be in the tools root folder,
        /// a new one will be created here if it's moved or deleted
        /// </remarks>
        public static string GetCollectionsFolder => collectionsPath;

        /// <returns>
        /// GUID of <paramref name="folderName"/> from asset database
        /// </returns>
        private static string FindFolder(string folderName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Folder {folderName}");
            return guids.Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(path => path.EndsWith(folderName));
        }
    }
}
