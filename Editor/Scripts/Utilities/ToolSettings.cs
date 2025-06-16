using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    /// <summary>
    /// Holds persistent tool settings.
    /// </summary>
    public class ToolSettings : ScriptableObject
    {
        public CollectionName currentCollectionName = CollectionName.None;
        public PrefabCollection CurrentPrefabCollection => PrefabCollection.GetCollectionByName(currentCollectionName);

        // Palette
        public float palette_minScale = 50f;
        public float palette_maxScale = 300f;
        public int palette_gridColumns = 4;

        // Placer
        public Color placer_color = Color.white;
        public float placer_radius = 0.2f;
        public LayerMask placer_includeMask = ~0; // masks to be included in scene interaction raycast. Default is everything.

        // Overlay
        public Vector2 overlay_size = new(420, 250);
        public bool overlay_autoSize;
        public bool overlay_showControlsHelpBox = true;

        public bool freeMode_alignWithSurface = false;

        // Marked dirty on disable so Unity knows to save it
        private void OnDisable()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();  
            AssetDatabase.Refresh();     
        }
    }
}
