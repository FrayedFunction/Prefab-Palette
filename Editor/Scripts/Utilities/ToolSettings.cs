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

        // Free Mode
        public Vector3 freeMode_placementOffset = Vector3.zero;
        public bool freeMode_alignWithSurface = false;
        public float freeMode_rotationSpeed = 2f;

        // Line Mode
        public float lineMode_ObjRndRotationMin;
        public float lineMode_ObjRndRotationMax;
        public bool linemode_ObjRndRotation;
        public bool lineMode_rotateOnX, lineMode_rotateOnY, lineMode_rotateOnZ;
        public bool lineMode_chainLines;
        public Vector3 lineMode_segmentOffset;
        public Vector3 lineMode_relativeRotation;
        public float lineMode_lineSpacing = 1;
        
        public bool lineMode_useAltObjs;
        public bool lineMode_useCollection;
        public CollectionName lineMode_altObjsCollection;
        public PrefabCollection lineMode_altCollection => PrefabCollection.GetCollectionByName(lineMode_altObjsCollection);
        public GameObject lineMode_altObj;
        public bool lineMode_randomAltObjs = true;
        public float lineMode_altObjProbability = 0.5f;
        public int lineMode_altObjInterval = 4;

        // Marked dirty on disable so Unity knows to save it
        private void OnDisable()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();  
            AssetDatabase.Refresh();     
        }
    }
}
