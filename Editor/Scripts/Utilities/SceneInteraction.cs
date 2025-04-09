using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace PrefabPalette
{
    public static class SceneInteraction
    {
        public static bool Snap { get; set; }

        public static void OnEnable()
        {
            SceneView.duringSceneGui += UpdateRaycast;
        }

        public static void OnDisable()
        {
            SceneView.duringSceneGui -= UpdateRaycast;
        }

        public static Vector3 Position { get; private set; }
        public static Vector3 SurfaceNormal { get; private set; }

        public static void UpdateRaycast(SceneView sceneView)
        {
            Event e = Event.current;
            if (e == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SurfaceNormal = hit.normal;

                Position = Snap ? Helpers.SnapToGrid(hit.point) : hit.point;  
            }
        }
    }
}
