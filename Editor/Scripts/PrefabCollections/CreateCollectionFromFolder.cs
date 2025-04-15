using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PrefabPalette
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

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

                    Debug.Log($"PrefabPalette: Collection '{sanitizedName}' created after reload.");
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
                Debug.LogWarning("PrefabPalette: No prefabs found in selected assets or folders.");
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

        [MenuItem("Assets/Prefab Palette: Generate Prefab Collection", true)]
        private static bool ValidateDoSomethingWithFolder()
        {
            return Selection.objects.Length > 0 && (
                Selection.objects.All(o => AssetDatabase.GetAssetPath(o).EndsWith(".prefab")) ||
                GetSelectedFolderPath() != null
            );
        }

        private static string GetSelectedFolderPath()
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null)
                return null;

            string path = AssetDatabase.GetAssetPath(selected);

            return AssetDatabase.IsValidFolder(path) ? path : null;
        }

        [System.Serializable]
        public class PrefabListWrapper
        {
            public List<string> prefabPaths = new List<string>();
        }
    }

    [Serializable]
    public class PrefabListWrapper
    {
        public List<string> prefabPaths = new();
    }

}
