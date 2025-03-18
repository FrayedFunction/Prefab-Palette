using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Maintains valid paths to the tools root and collections folder.
    /// </summary>
    [InitializeOnLoad]
    public static class PathDr
    {
        // Keys for editor prefs
        private const string ToolRootPathKey = "PrefabPalette/PathDr_ToolRootPath";
        private const string CollectionsPathKey = "PrefabPalette/PathDr_CollectionsPath";

        private static string toolRootPath;
        private static string collectionsPath;

        static PathDr()
        {
            toolRootPath = EditorPrefs.GetString(ToolRootPathKey, string.Empty);
            collectionsPath = EditorPrefs.GetString(CollectionsPathKey, string.Empty);
        }

        /// <returns>
        /// Path to the tools root folder.
        /// </returns>
        public static string GetToolRootPath
        {
            get
            {
                // If the path for the tools root folder isn't set or the directory no longer
                // exists, look for it in th asset database.
                if (string.IsNullOrEmpty(toolRootPath) || !Directory.Exists(toolRootPath))
                {
                    toolRootPath = FindFolder("PrefabPalette");

                    // If the path's found, save it to editor prefs.
                    if (!string.IsNullOrEmpty(toolRootPath))
                    {
                        EditorPrefs.SetString(ToolRootPathKey, toolRootPath);
                    }
                }

                return toolRootPath;
            }
        }

        /// <summary>
        /// Returns the path to the folder where prefab collections are generated.
        /// </summary>
        /// <remarks>
        /// Note: Always assumed to be in the tools root folder,
        /// a new one will be created here if it's moved or deleted
        /// </remarks>
        public static string GetCollectionsFolder
        {
            get
            {
                // If the path to the Collections folder isn't set or the directory dosen't exists,
                // either find it or create a new one in the tools root directory.
                if (string.IsNullOrEmpty(collectionsPath) || !Directory.Exists(collectionsPath))
                {
                    string newPath = Path.Combine(GetToolRootPath, "Collections");

                    if (!Directory.Exists(newPath))
                    {
                        // Create the folder if it dosen't exist
                        Debug.Log($"PrefabPalette/{nameof(PathDr)}: Collections folder not found at {newPath}, creating...");
                        string newFolderGUID = AssetDatabase.CreateFolder(GetToolRootPath, "Collections");

                        if (string.IsNullOrEmpty(newFolderGUID))
                        {
                            // Creation failed.
                            Debug.LogError($"PrefabPalette/{nameof(PathDr)}: Could not create Collections folder at {newPath}!");
                            return string.Empty;
                        }

                        // Folder created.
                        collectionsPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);

                        Debug.Log($"PrefabPalette/{nameof(PathDr)}: Collections folder created successfully at {collectionsPath}. Refreshing AssetDatabase...");
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        // Folder found and exists.
                        Debug.Log($"PrefabPalette/{nameof(PathDr)}: Collections folder exists.");
                        collectionsPath = newPath;
                    }

                    EditorPrefs.SetString(CollectionsPathKey, collectionsPath);
                }

                return collectionsPath;
            }
        }


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
