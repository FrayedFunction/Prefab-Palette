using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Scriptable Object that holds a list of prefabs and its collection name.
    /// </summary>
    public class PrefabCollection : ScriptableObject
    {
        // Needs to be seralised otherwise it will not persist
        [SerializeField] private string nameAsString;

        /// <summary>
        /// User set name of collection.
        /// </summary>
        /// <returns>
        /// CollectionName.None if its name doesn't exist in the enum
        /// </returns>
        public CollectionName Name
        {
            get => Enum.TryParse(nameAsString, out CollectionName result) ? result : CollectionName.None;
            set => nameAsString = value.ToString();
        }

        /// <summary>
        /// List of prefabs in this collection.
        /// </summary>
        public List<GameObject> prefabList = new();

        public static PrefabCollection CreateNewCollection(CollectionName name)
        {
            // If no matching collection is found, create a new one
            PrefabCollection asset = ScriptableObject.CreateInstance<PrefabCollection>();
            asset.Name = name; // Assigns string-based enum reference

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{PathDr.GetCollectionsFolder}/{name}_PrefabCollection.asset");

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }
    }
}
