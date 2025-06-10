using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PrefabPalette
{
    /// <summary>
    /// Handles creating a PrefabCollection asset from selected folders or prefabs in the Project Window.
    /// This class uses EditorPrefs to store data across domain reloads so the process can continue smoothly.
    /// It supports generating a collection from selected prefabs or all prefabs inside selected folders.
    /// </summary>
    [InitializeOnLoad]
    public static class CreateCollectionFromFolder
    {
        static CreateCollectionFromFolder()
        {
            EditorApplication.delayCall += CreateAndPopulateCollection;
        }

        private static void CreateAndPopulateCollection()
        {
            if (EditorPrefs.HasKey("PendingPrefabCollectionName"))
            {
                string sanitizedName = EditorPrefs.GetString("PendingPrefabCollectionName");
                EditorPrefs.DeleteKey("PendingPrefabCollectionName");

                if (System.Enum.TryParse<CollectionName>(sanitizedName, out var enumValue))
                {
                    var collection = PrefabCollection.CreateNewCollection(enumValue);

                    if (EditorPrefs.HasKey("PendingPrefabList"))
                    {
                        string json = EditorPrefs.GetString("PendingPrefabList");

                        var wrapper = JsonUtility.FromJson<PrefabListWrapper>(json);
                        EditorPrefs.DeleteKey("PendingPrefabList");

                        List<GameObject> prefabList = wrapper.prefabPaths
                            .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
                            .Where(go => go != null)
                            .ToList();

                        collection.prefabList = prefabList;
                        EditorUtility.SetDirty(collection);
                        AssetDatabase.SaveAssets();
                    }

                    EditorUtility.DisplayDialog(
                        "Successfully Created Colelction!",
                        $"Collection '{sanitizedName}' was created successfully after reload!",
                        "OK"
                    );
                }
                else
                {
                    Debug.LogError($"PrefabPalette: Could not parse '{sanitizedName}' after reload!");
                }
            }
        }

        [MenuItem("Assets/Prefab Palette: Generate Prefab Collection", false, 2000)]
        private static void Generate()
        {
            var prefabPaths = new List<string>();

            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (string.IsNullOrEmpty(path))
                    continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    prefabPaths.AddRange(GetPrefabPathsFromFolder(path));
                }
                else if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    string relativePath = path.Replace('\\', '/'); // Ensure forward slashes
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);

                    if (prefab != null)
                    {
                        Debug.Log($" - {prefab.name} ({relativePath})");
                        prefabPaths.Add(relativePath);
                    }
                    else
                    {
                        Debug.LogError($"PrefabPalette: Failed to load prefab at {relativePath}.");
                    }
                }
            }
            

            // prevent duplicate prefabs
            prefabPaths = prefabPaths.Distinct().ToList();

            if (prefabPaths.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Prefabs Found",
                    "No prefabs were found in the selected assets or folders.",
                    "OK"
                );
                return;
            }

            var wrapper = new PrefabListWrapper { prefabPaths = prefabPaths };
            string json = JsonUtility.ToJson(wrapper);
            EditorPrefs.SetString("PendingPrefabList", json);

            CreateCollectionWindow.Show(collectionName =>
            {
                EditorPrefs.SetString("PendingPrefabCollectionName", collectionName);

                PrefabCollectionList.Instance.collectionNames.Add(collectionName);
                EditorUtility.SetDirty(PrefabCollectionList.Instance);
                AssetDatabase.SaveAssets();
                PrefabCollectionList.Instance.GenerateEnum();
            });
        }

        public static List<string> GetPrefabPathsFromFolder(string folderPath)
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
            string[] prefabFiles = Directory.GetFiles(absolutePath, "*.prefab", SearchOption.TopDirectoryOnly);

            if (prefabFiles.Length == 0)
            {
                Debug.LogWarning($"PrefabPalette: No prefabs found in {absolutePath}.");
                return new List<string>();
            }

            Debug.Log($"PrefabPalette: Found {prefabFiles.Length} prefabs in folder:");

            List<string> prefabPaths = new List<string>();

            foreach (string fullPath in prefabFiles)
            {
                string relativePath = "Assets" + fullPath.Substring(Application.dataPath.Length);
                relativePath = relativePath.Replace('\\', '/'); // Ensure forward slashes

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);

                if (prefab != null)
                {
                    Debug.Log($" - {prefab.name} ({relativePath})");
                    prefabPaths.Add(relativePath);
                }
                else
                {
                    Debug.LogError($"Failed to load prefab at {relativePath}.");
                }
            }

            return prefabPaths;
        }

        /// <summary>
        /// Validates that the selected object is either a folder or prefab/s.
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Prefab Palette: Generate Prefab Collection", true)]
        private static bool ValidateSelected()
        {
            return Selection.objects.Length > 0 && (
                Selection.objects.All(o => AssetDatabase.GetAssetPath(o).EndsWith(".prefab")) ||
                GetSelectedFolderPath() != null
            );
        }

        private static string GetSelectedFolderPath()
        {
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path))
                    return path;
            }

            return null;
        }

        [System.Serializable]
        public class PrefabListWrapper
        {
            public List<string> prefabPaths = new List<string>();
        }
    }
}
