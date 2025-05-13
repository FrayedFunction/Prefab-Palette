using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    /// <summary>
    /// Holds persistent tool settings.
    /// </summary>
    public class ToolSettings : ScriptableObject
    {
        public CollectionName collectionName = CollectionName.None;
        public Vector3 placementOffset = Vector3.zero;
        public bool alignWithSurface = false;
        public float rotationSpeed = 2f;
        public float minPaletteScale = 50f;
        public float maxPaletteScale = 300f;
        public bool showPaletteSettings = false;
        public bool showPlacerSettings = false;
        public int gridColumns = 4;
        public Color placerColor = Color.white;
        public float placerRadius = 0.2f;
        public bool showHeader = true;
        public float lineSpacing = 1;
        public bool randomAltObjs = true;
        public float altObjProbability = 0.5f;
        public int altObjInterval = 4;
        public bool showModeSettings;
        public LayerMask includeMask = ~0; // masks to be included in scene interaction raycast. Default is everything.
        public Vector3 relativeRotation;
        public bool chainLines;
        public Vector3 segmentOffset;
        public Vector2 overlaySize = new(420, 250);
        public bool autoOverlaySize;

        // Marked dirty on disable so Unity knows to save it
        private void OnDisable()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();  
            AssetDatabase.Refresh();     
        }
    }
}
