using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace PrefabPalette
{
    public static class SceneInteraction
    {
        static ToolSettings toolSettings;

        public static bool SnapToGrid { get; set; }

        public static void OnEnable(ToolSettings settings)
        {
            SceneView.duringSceneGui += UpdateRaycast;
            toolSettings = settings;
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
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, toolSettings.includeMask))
            {
                SurfaceNormal = hit.normal;

                Position = SnapToGrid ? Helpers.SnapToGrid(hit.point) : hit.point;  
            }
        }
    }
}
