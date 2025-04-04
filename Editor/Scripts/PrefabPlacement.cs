using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class PrefabPlacement
    {
        public static bool IsRotating => isRotating;

        static Vector3 lastSurfaceNormal;
        static GameObject currentPlacedObject;
        static bool isRotating = false;
        
        public static void HandleSinglePrefabPlacement(PrefabPaletteTool tool)
        {
            Event e = Event.current;

            // Place object on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                lastSurfaceNormal = SceneRaycastHelper.SurfaceNormal;

                currentPlacedObject = (GameObject)PrefabUtility.InstantiatePrefab(tool.selectedPrefab);
                currentPlacedObject.transform.SetPositionAndRotation(SceneRaycastHelper.Position + tool.Settings.placementOffset, tool.Settings.alignWithSurface ? Quaternion.FromToRotation(Vector3.up, lastSurfaceNormal) : Quaternion.identity);
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
    }
}
