using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class SinglePrefabMode : IPlacementMode
    {
        static Vector3 lastSurfaceNormal;
        static GameObject currentPlacedObject;

        public void OnActive(PrefabPaletteTool tool)
        {
            Event e = Event.current;

            if (tool.selectedPrefab == null) 
                return;

            // Place object on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                VisualPlacer.Stop();

                lastSurfaceNormal = SceneInteraction.SurfaceNormal;

                currentPlacedObject = (GameObject)PrefabUtility.InstantiatePrefab(tool.selectedPrefab);
                currentPlacedObject.transform.SetPositionAndRotation(SceneInteraction.Position + tool.Settings.placementOffset, tool.Settings.alignWithSurface ? Quaternion.FromToRotation(Vector3.up, lastSurfaceNormal) : Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(currentPlacedObject, "Placed Prop");

                e.Use();
            }

            // Rotate while holding the mouse button
            if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && currentPlacedObject != null)
            {

                float angle = e.delta.x * tool.Settings.rotationSpeed;
                Vector3 axis = tool.Settings.alignWithSurface ? lastSurfaceNormal : Vector3.up;
                currentPlacedObject.transform.Rotate(axis, angle, Space.World);
                e.Use();
            }

            // Stop rotating on mouse release
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                VisualPlacer.ShowTarget(tool.Settings.placerColor, tool.Settings.placerRadius);
                currentPlacedObject = null;
            }
        }

        public void OnEnter(PrefabPaletteTool tool)
        {
        }

        public void OnExit(PrefabPaletteTool tool)
        {
        }

        public void SettingsGUI(PrefabPaletteTool tool)
        {
            tool.Settings.rotationSpeed = EditorGUILayout.Slider("Rotation Speed", tool.Settings.rotationSpeed, 0.1f, 5);
            tool.Settings.placementOffset = EditorGUILayout.Vector3Field("Placement Offset", tool.Settings.placementOffset);
            tool.Settings.alignWithSurface = EditorGUILayout.Toggle("Align with surface?", tool.Settings.alignWithSurface);
        }
    }
}
