using System;
using System.Collections.Generic;
using System.Linq;
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
            private set => nameAsString = value.ToString();
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

        // This isn't really a property of a prefab collection, should live in a manager or utility script or something.
        /// <summary>
        /// Returns a list of all saved prefab collections.
        /// </summary>
        public static List<PrefabCollection> GetAllCollectionsInFolder =>
            AssetDatabase.FindAssets($"t:{nameof(PrefabCollection)}", new[] { PathDr.GetCollectionsFolder })
            .Select(guid => AssetDatabase.LoadAssetAtPath<PrefabCollection>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();

        /// <summary>
        /// Returns prefab collection object by name, creates it if it doesn't exist
        /// </summary>
        /// <remarks>
        /// Note: CollectionName.None returns null.
        /// </remarks>
        public static PrefabCollection GetOrCreateCollection(CollectionName name)
        {
            if (name == CollectionName.None)
                return null;

            foreach (var collection in GetAllCollectionsInFolder)
            {
                if (collection != null && collection.Name == name)
                {
                    return collection;
                }
            }

            return  CreateNewCollection(name);
        }
    }
}
