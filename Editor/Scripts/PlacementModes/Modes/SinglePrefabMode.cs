using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Allows placing and rotating a single prefab in the scene using mouse input.
    /// </summary>
    public class SinglePrefabMode : IPlacementMode
    {
        SingleModeSettings Settings;
        GameObject currentPlacedObject;
        Vector3 lastSurfaceNormal;

        public SinglePrefabMode(SingleModeSettings settings)
        {
            this.Settings = settings;
        }

        /// <summary>
        /// Called every frame while the mode is active. Handles prefab placement and rotation based on mouse input.
        /// </summary>
        /// <param name="context">The current tool context.</param>
        public void OnActive(ToolContext context)
        {
            Event e = Event.current;

            if (context.SelectedPrefab == null) 
                return;

            // Place object on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                VisualPlacer.Stop();

                lastSurfaceNormal = SceneInteraction.SurfaceNormal;

                currentPlacedObject = (GameObject)PrefabUtility.InstantiatePrefab(context.SelectedPrefab);
                currentPlacedObject.transform.SetPositionAndRotation(SceneInteraction.Position + Settings.freeMode_placementOffset, context.Settings.placer_alignWithSurface ? Quaternion.FromToRotation(Vector3.up, lastSurfaceNormal) : Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(currentPlacedObject, "Placed Prop");

                e.Use();
            }

            // Rotate while holding the mouse button
            if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && currentPlacedObject != null)
            {
                float angle = e.delta.x * Settings.freeMode_rotationSpeed;
                Vector3 axis = context.Settings.placer_alignWithSurface ? lastSurfaceNormal : Vector3.up;
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

        public void OnEnter(ToolContext context)
        {
        }

        public void OnExit(ToolContext context)
        {
        }

        /// <summary>
        /// Draws the settings GUI for this placement mode in the overlay.
        /// Allows configuration of rotation speed, placement offset, and alignment.
        /// </summary>
        /// <param name="context">The current tool context.</param>
        public void SettingsOverlayGUI(ToolContext context)
        {
            Settings.freeMode_rotationSpeed = EditorGUILayout.Slider("Rotation Speed", Settings.freeMode_rotationSpeed, 0.1f, 5);
            Settings.freeMode_placementOffset = EditorGUILayout.Vector3Field("Placement Offset", Settings.freeMode_placementOffset);
        }

        public string[] ControlsHelpBox => new string[]
        {
                "LMB", "Place Single Prefab"
        };
    }
}
