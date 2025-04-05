using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    /// <summary>
    /// Holds persistent tool settings.
    /// </summary>
    public class ToolSettings : ScriptableObject
    {
        public CollectionName collectionNameDropdown = CollectionName.None;
        public Vector3 placementOffset = Vector3.zero;
        public bool alignWithSurface = false;
        public float rotationSpeed = 2f;
        public float minPaletteScale = 50f;
        public float maxPaletteScale = 300f;
        public bool showPaletteSettings = false;
        public bool showPlacementSettings = false;
        public int gridColumns = 4;
        public Color placerColor = Color.white;
        public float placerRadius = 0.2f;
        public bool isNameDropdownActive = true;
        public bool showHeader = true;
        public float fenceSpacing = 1;
        public float fenceCornerOffset = 0.5f;
        public bool randomBrokenFences = true;
        public float brokenProbability = 0.5f;
        public int brokenInterval = 4;

        // Marked dirty on disable so Unity knows to save it
        private void OnDisable()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();  
            AssetDatabase.Refresh();     
        }
    }
}
