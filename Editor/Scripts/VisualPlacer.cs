using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    /// <summary>
    /// Scene GUI visual placer.
    /// </summary>
    [InitializeOnLoad]
    public class VisualPlacer
    {
        private static Vector3 previewPosition;
        private static bool isActive = false;
        private static float targetRadius = 1.0f;

        private static Color color;

        static VisualPlacer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isActive) return;

            previewPosition = SceneRaycastHelper.Position;
            DrawPlacer(previewPosition, SceneRaycastHelper.SurfaceNormal);

            sceneView.Repaint();
        }

        private static void DrawPlacer(Vector3 position, Vector3 normal)
        {
            // Ignore colours alpha and force to 1.
            Handles.color = new Color(color.r, color.g, color.b, 1f);

            // Draw outer circle
            Handles.DrawWireDisc(position, normal, targetRadius);

            // Draw inner solid circle (30% of targetRadius)
            Handles.color = new Color(color.r, color.g, color.b, 0.25f);
            Handles.DrawSolidDisc(position, normal, targetRadius * 0.3f);
        }

        /// <summary>
        /// Start rendering the placer
        /// </summary>
        public static void Show(Color color, float radius)
        {
            isActive = true;
            VisualPlacer.color = color;
            targetRadius = Mathf.Max(0.1f, radius);

        }

        /// <summary>
        /// Stop rendering the placer
        /// </summary>
        public static void Stop()
        {
            isActive = false;
        }
    }
}
