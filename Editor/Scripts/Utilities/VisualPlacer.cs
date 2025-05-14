using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    /// <summary>
    /// Scene GUI visual placer.
    /// </summary>
    public static class VisualPlacer
    {
        private static Vector3 previewPosition;
        private static bool isActive = false;
        private static float targetRadius = 1.0f;
        private static Vector3 lastPreviewPosition;
        private static Color color;
        static ToolSettings settings;

        public static void OnEnable(ToolSettings settings)
        {            
            SceneView.duringSceneGui += OnSceneGUI;
            VisualPlacer.settings = settings;
        }

        public static void OnDisable()
        {
            Stop();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isActive) return;

            // Get the new position and update only if it's different
            Vector3 newPosition = SceneInteraction.Position;
            if (newPosition != lastPreviewPosition)
            {
                previewPosition = newPosition;
                lastPreviewPosition = newPosition;

                // Force a repaint when position changes
                sceneView.Repaint();
            }
            var normal = settings.freeMode_alignWithSurface ? SceneInteraction.SurfaceNormal : Vector3.up;

            // Draw the visual placer
            DrawPlacer(previewPosition, normal);
        }

        private static void DrawPlacer(Vector3 position, Vector3 normal)
        {
            // Outer circle - full opacity
            Handles.color = new Color(color.r, color.g, color.b, 1f);
            Handles.DrawWireDisc(position, normal, targetRadius);

            // Inner circle - semi-transparent
            Handles.color = new Color(color.r, color.g, color.b, 0.25f);
            Handles.DrawSolidDisc(position, normal, targetRadius * 0.3f);
        }

        /// <summary>
        /// Start rendering the visual placer
        /// </summary>
        public static void ShowTarget()
        {
            color = settings.placer_color;
            targetRadius = Mathf.Max(0.1f, settings.placer_radius);

            if (isActive)
                return;

            isActive = true;

            // Clear previous position to prevent old data from interfering
            lastPreviewPosition = Vector3.zero;

            // Force SceneView to repaint immediately when enabling
            SceneView.RepaintAll();
        }

        /// <summary>
        /// </summary>
        public static void Stop()
        {
            if (!isActive) return;

            // Disable the visual placer
            isActive = false;

            // Clear position data when stopping
            lastPreviewPosition = Vector3.zero;

            // Force SceneView to repaint immediately when disabling
            SceneView.RepaintAll();
        }
    }

}
