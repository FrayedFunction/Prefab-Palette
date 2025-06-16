using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Allows placing and rotating a single prefab in the scene using mouse input.
    /// </summary>
    public class SinglePrefabMode : IPlacementMode
    {
        GameObject currentPlacedObject;
        Vector3 lastSurfaceNormal;

        /// <summary>
        /// Called every frame while the mode is active. Handles prefab placement and rotation based on mouse input.
        /// </summary>
        /// <param name="tool">The current tool context.</param>
        public void OnActive(ToolContext tool)
        {
            Event e = Event.current;

            if (tool.SelectedPrefab == null) 
                return;

            // Place object on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                VisualPlacer.Stop();

                lastSurfaceNormal = SceneInteraction.SurfaceNormal;

                currentPlacedObject = (GameObject)PrefabUtility.InstantiatePrefab(tool.SelectedPrefab);
                currentPlacedObject.transform.SetPositionAndRotation(SceneInteraction.Position + tool.Settings.freeMode_placementOffset, tool.Settings.freeMode_alignWithSurface ? Quaternion.FromToRotation(Vector3.up, lastSurfaceNormal) : Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(currentPlacedObject, "Placed Prop");

                e.Use();
            }

            // Rotate while holding the mouse button
            if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && currentPlacedObject != null)
            {
                float angle = e.delta.x * tool.Settings.freeMode_rotationSpeed;
                Vector3 axis = tool.Settings.freeMode_alignWithSurface ? lastSurfaceNormal : Vector3.up;
                currentPlacedObject.transform.Rotate(axis, angle, Space.World);
                e.Use();
            }

            // Stop rotating on mouse release
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                VisualPlacer.ShowTarget();
                currentPlacedObject = null;
                e.Use();
            }
        }

        public void OnEnter(ToolContext tool)
        {
        }

        public void OnExit(ToolContext tool)
        {
        }

        /// <summary>
        /// Draws the settings GUI for this placement mode in the overlay.
        /// Allows configuration of rotation speed, placement offset, and alignment.
        /// </summary>
        /// <param name="tool">The current tool context.</param>
        public void SettingsOverlayGUI(ToolContext tool)
        {
            tool.Settings.freeMode_rotationSpeed = EditorGUILayout.Slider("Rotation Speed", tool.Settings.freeMode_rotationSpeed, 0.1f, 5);
            tool.Settings.freeMode_placementOffset = EditorGUILayout.Vector3Field("Placement Offset", tool.Settings.freeMode_placementOffset);
            tool.Settings.freeMode_alignWithSurface = EditorGUILayout.Toggle("Align with surface?", tool.Settings.freeMode_alignWithSurface);
        }

        public string[] ControlsHelpBox => new string[]
        {
                "LMB", "Place Single Prefab"
        };
    }
}
