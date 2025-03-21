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
        private CollectionsList collectionsList;
        private PrefabPaletteTool tool;
        private Editor editorInstance;

        public static void OpenWindow(CollectionsList cl, PrefabPaletteTool t)
        {
            CollectionsListInspector window = GetWindow<CollectionsListInspector>("Collections Inspector");
            cl.SyncListWithEnum();
            window.collectionsList = cl;
            window.tool = t;
            window.editorInstance = Editor.CreateEditor(cl);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Collections", EditorStyles.whiteLargeLabel);

            if (editorInstance != null)
            {
                editorInstance.OnInspectorGUI();
            }

            // Disable button if AssetDatabase is reloading
            GUI.enabled = !EditorApplication.isCompiling && !EditorApplication.isUpdating;

            if (GUILayout.Button(GUI.enabled ? "Save" : "Saving..."))
            {
                collectionsList.GenerateEnum();
                CleanupCollectionsFolder(tool.GetAllCollectionsInFolder, collectionsList);
            }
        }

        /// <summary>
        /// Delete PrefabCollection objects no longer in the list.
        /// </summary>
        /// <param name="collectionsInFolder"></param>
        /// <param name="collectionsList"></param>
        private void CleanupCollectionsFolder(List<PrefabCollection> collectionsInFolder, CollectionsList collectionsList)
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
