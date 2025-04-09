using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace PrefabPalette
{
    public static class SceneInteraction
    {
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

                Position = PlacementModeManager.CurrentType == PlacementModeManager.ModeType.Snap ? SnapToGrid(hit.point) : hit.point;  
            }
        }

        private static Vector3 SnapToGrid(Vector3 position)
        {
            // Use unitys built in scene grid
            float gridSize = UnityEditor.EditorSnapSettings.move.x;
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;
            return position;
        }
    }
}
