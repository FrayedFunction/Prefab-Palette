using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PrefabPalette;
using System.Linq;
using System;

namespace PrefabPalette
{
    /// <summary>
    /// Inspector window for the collections list object.
    /// </summary>
    public class CollectionsListInspector : EditorWindow
    {
        private PrefabCollectionList collectionsList;
        private PrefabPaletteTool tool;
        private Editor editorInstance;
        private Vector2 scrollPos;

        public static void OpenWindow(PrefabPaletteTool t)
        {
            CollectionsListInspector window = GetWindow<CollectionsListInspector>("Collections Inspector");
            window.collectionsList = PrefabCollectionList.Instance;
            window.collectionsList.SyncListWithEnum();
            window.tool = t;
            window.editorInstance = Editor.CreateEditor(window.collectionsList);
            window.Show();
        }

        private void OnGUI()
        {
            Helpers.TitleText("Prefab Collections", 15);
            Helpers.DrawLine(Color.grey);

            if (editorInstance != null)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                editorInstance.OnInspectorGUI();
                EditorGUILayout.EndScrollView();
            }

            // Disable button if AssetDatabase is reloading
            GUI.enabled = !EditorApplication.isCompiling && !EditorApplication.isUpdating;

            if (GUILayout.Button(GUI.enabled ? "Save" : "Saving..."))
            {
                collectionsList.GenerateEnum();
                CleanupCollectionsFolder(tool.GetAllCollectionsInFolder, collectionsList);
            }

            EditorGUILayout.Space(10f);
        }

        /// <summary>
        /// Delete PrefabCollection objects no longer in the list.
        /// </summary>
        /// <param name="collectionsInFolder"></param>
        /// <param name="collectionsList"></param>
        private void CleanupCollectionsFolder(List<PrefabCollection> collectionsInFolder, PrefabCollectionList collectionsList)
        {
            // Convert collectionNames to HashSet for quick lookup
            HashSet<string> validCollections = new HashSet<string>(
                collectionsList.collectionNames.Select(name => name.ToLower())
            );

            // Collect assets that need to be deleted
            List<PrefabCollection> toDelete = collectionsInFolder
                .Where(collection => !validCollections.Contains(collection.Name.ToString().ToLower()))
                .ToList();

            // Delete each asset
            foreach (var collection in toDelete)
            {
                string assetPath = AssetDatabase.GetAssetPath(collection);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    Debug.Log($"Deleted: {assetPath}");
                }
            }

            // Remove deleted items from the list
            collectionsInFolder.RemoveAll(toDelete.Contains);

            // Save the project after asset deletion
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
