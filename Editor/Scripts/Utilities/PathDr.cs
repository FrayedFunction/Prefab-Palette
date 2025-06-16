using System.IO;
using System.Linq;
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
        private static string modeSettingsPath;

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
                    Debug.LogError($"PrefabPalette/{nameof(PathDr)}Can't find editor folder!");
                    return;
                }
            }

            generatedFolderPath = Path.Combine(toolPath, "Generated");
            ValidateFolderPath(generatedFolderPath);

            collectionsPath = Path.Combine(generatedFolderPath, "Collections");
            ValidateFolderPath(collectionsPath);

            modeSettingsPath = Path.Combine(generatedFolderPath, "Mode Settings");
            ValidateFolderPath(modeSettingsPath);
        }

        public static bool ValidateFolderPath(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                // Get parent folder and new folder name
                string parent = Path.GetDirectoryName(fullPath);
                string folderName = Path.GetFileName(fullPath);

                if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
                {
                    Debug.LogError($"PrefabPalette/{nameof(ValidateFolderPath)}: Invalid path '{fullPath}'");
                    return false;
                }

                string newGUID = AssetDatabase.CreateFolder(parent, folderName);

                if (string.IsNullOrEmpty(newGUID))
                {
                    Debug.LogError($"PrefabPalette/{nameof(ValidateFolderPath)}: Failed to create folder '{folderName}' in '{parent}'");
                    return false;
                }

                string createdPath = AssetDatabase.GUIDToAssetPath(newGUID);

                Debug.Log($"PrefabPalette/{nameof(ValidateFolderPath)}: Folder '{folderName}' created successfully at '{createdPath}'. Refreshing AssetDatabase...");
                AssetDatabase.Refresh();
            }

            return true;
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

        public static string GetModeSettingsFolder => modeSettingsPath;
        
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
