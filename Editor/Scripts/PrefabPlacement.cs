using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class PrefabPlacement : PlacementMode
    {
        public static bool IsRotating => isRotating;

        static Vector3 lastSurfaceNormal;
        static GameObject currentPlacedObject;
        static bool isRotating = false;

        public override void OnEnter()
        {
        }

        public override void OnActive(PrefabPaletteTool tool)
        {
            Event e = Event.current;
            
            if (tool.selectedPrefab == null || isRotating)
            {
                VisualPlacer.Stop();
            }

            if (tool.selectedPrefab == null) 
                return;

            VisualPlacer.ShowTarget(tool.Settings.placerColor, tool.Settings.placerRadius);

            // Place object on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                lastSurfaceNormal = SceneInteraction.SurfaceNormal;

                currentPlacedObject = (GameObject)PrefabUtility.InstantiatePrefab(tool.selectedPrefab);
                currentPlacedObject.transform.SetPositionAndRotation(SceneInteraction.Position + tool.Settings.placementOffset, tool.Settings.alignWithSurface ? Quaternion.FromToRotation(Vector3.up, lastSurfaceNormal) : Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(currentPlacedObject, "Placed Prop");

                e.Use();
            }

            // Rotate while holding the mouse button
            if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && currentPlacedObject != null)
            {
                if (!isRotating)
                {
                    isRotating = true;
                }

                float angle = e.delta.x * tool.Settings.rotationSpeed;
                Vector3 axis = tool.Settings.alignWithSurface ? lastSurfaceNormal : Vector3.up;
                currentPlacedObject.transform.Rotate(axis, angle, Space.World);
                e.Use();
            }

            // Stop rotating on mouse release
            if (e.type == EventType.MouseUp && e.button == 0 && isRotating)
            {
                isRotating = false;
                currentPlacedObject = null;
            }
        }

        public override void OnExit()
        {
        }

        public override void SettingsGUI()
        {
        }
    }
}
