using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static UnityEngine.GridBrushBase;

namespace PrefabPalette
{
    public class PrefabPaletteTool
    {
        private static PrefabPaletteTool instance;

        public static PrefabPaletteTool Instance => instance ??= new();

        public ToolSettings Settings { get; private set; }

        public PrefabCollection CurrentPrefabCollection { get; set; }

        public GameObject SelectedPrefab { get; set; }

        /// <summary>
        /// Returns a list of all saved prefab collections.
        /// </summary>
        public List<PrefabCollection> GetAllCollectionsInFolder =>
            AssetDatabase.FindAssets($"t:{nameof(PrefabCollection)}", new[] { PathDr.GetCollectionsFolder })
            .Select(guid => AssetDatabase.LoadAssetAtPath<PrefabCollection>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();

        /// <summary>
        /// Returns prefab collection object by name, creates it if it doesn't exist
        /// </summary>
        /// <remarks>
        /// Note: CollectionName.None returns null.
        /// </remarks>
        public PrefabCollection GetPrefabCollection(CollectionName name)
        {
            if (name == CollectionName.None)
                return null;
            if (CurrentPrefabCollection != null && CurrentPrefabCollection.Name == name)
                return CurrentPrefabCollection;
            SelectedPrefab = null;

            foreach (var collection in GetAllCollectionsInFolder)
            {
                if (collection != null && collection.Name == name)
                {
                    return collection;
                }
            }

            return PrefabCollection.CreateNewCollection(name);
        }

        PrefabPaletteTool()
        {
            Settings = Helpers.LoadOrCreateAsset<ToolSettings>(PathDr.GetGeneratedFolderPath, "ToolSettings.asset", out _);
        }
    }
}
