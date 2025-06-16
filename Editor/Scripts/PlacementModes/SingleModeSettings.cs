using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class SingleModeSettings : ScriptableObject
    {
        public Vector3 freeMode_placementOffset = Vector3.zero;
        public float freeMode_rotationSpeed = 2f;

        // Marked dirty on disable so Unity knows to save it
        private void OnDisable()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
