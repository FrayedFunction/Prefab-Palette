using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class LineModeSettings : PlacementModeSettings
    {
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
        public PrefabCollection lineMode_altCollection => PrefabCollection.GetOrCreateCollection(lineMode_altObjsCollection);
        public GameObject lineMode_altObj;
        public bool lineMode_randomAltObjs = true;
        public float lineMode_altObjProbability = 0.5f;
        public int lineMode_altObjInterval = 4;
    }
}
