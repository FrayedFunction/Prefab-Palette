using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    [InitializeOnLoad]
    public class SceneRaycastHelper
    {
        static SceneRaycastHelper()
        {
            SceneView.duringSceneGui += UpdateRaycast;
        }

        public enum PlacementMode
        {
            Free,
            Snap,
            Line,
        }

        public static PlacementMode CurrentPlacementMode {  get; set; }

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
                
                switch (CurrentPlacementMode)
                {
                    case PlacementMode.Free:
                        Position = hit.point;
                    break;
                    case PlacementMode.Snap:
                        Position = SnapToGrid(hit.point);
                    break;
                }

                
            }
        }

        private static Vector3 SnapToGrid(Vector3 position)
        {
            float gridSize = UnityEditor.EditorSnapSettings.move.x; // Use Unity's snap settings
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;
            return position;
        }
    }
}
