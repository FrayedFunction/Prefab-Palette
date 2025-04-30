using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static UnityEngine.GridBrushBase;
using log4net.Util;
using System.IO.Pipes;

namespace PrefabPalette
{
    public class LineDrawMode : IPlacementMode
    {
        static List<Vector3> linePoints = new();
        static List<GameObject> spawnedObjects = new();
        static List<GameObject> previewObjects = new List<GameObject>();
        static GameObject brokenFencePrefab;

        #region Placement Mode interface
        public void OnActive(PrefabPaletteTool tool)
        {
            Event e = Event.current;
            HandleInput(tool, e);

            if (linePoints.Count < 1) return;

            // This is inefficient!
            ClearPreviewObjects();

            // Draw preview
            Vector3 startPoint = linePoints.Last();
            Vector3 endPoint = SceneInteraction.Position;

            Vector3 line = endPoint - startPoint;
            float dist = line.magnitude;
            Vector3 dir = line.normalized;

            int pointCount = Mathf.FloorToInt(dist / tool.Settings.lineSpacing);

            if (pointCount < 1) 
                return;

            float spacing = dist / (pointCount + 1);

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 point = startPoint + dir * spacing * (i + 1);
                var obj = GameObject.Instantiate(tool.SelectedPrefab);
                obj.transform.position = point;
                Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
                rotation.eulerAngles += tool.Settings.relativeRotation;
                obj.transform.rotation = rotation;

                previewObjects.Add(obj);
            }
        }

        public void OnEnter(PrefabPaletteTool tool)
        {
        }

        public void OnExit(PrefabPaletteTool tool)
        {
        }
        #endregion

        private void ClearPreviewObjects()
        {
            previewObjects.ForEach(obj => GameObject.DestroyImmediate(obj));
            previewObjects.Clear();
        }

        private void HandleInput(PrefabPaletteTool tool, Event e)
        {
            // Create line with left mouse click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                // Add a point for the lines start position
                linePoints.Add(SceneInteraction.Position);

                if (linePoints.Count > 0)
                {
                    foreach (var preview in previewObjects)
                    {
                        GameObject placed = GameObject.Instantiate(tool.SelectedPrefab);
                        placed.transform.SetPositionAndRotation(preview.transform.position, preview.transform.rotation);
                        spawnedObjects.Add(placed);
                    }
                }

                e.Use();
            }

            // Confirm with Enter
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                ClearPreviewObjects();
                linePoints.Clear();
                spawnedObjects.Clear();
                e.Use();
            }

            // Cancel with Escape
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                linePoints.Clear();
                spawnedObjects.ForEach(p => GameObject.DestroyImmediate(p, false));
                spawnedObjects.Clear();
                ClearPreviewObjects();
                e.Use();
            }
        }

        #region GUI
        public void SettingsGUI(PrefabPaletteTool tool)
        {
            tool.Settings.lineSpacing = EditorGUILayout.FloatField("Spacing", tool.Settings.lineSpacing);
            tool.Settings.lineCornerOffset = EditorGUILayout.FloatField("Corner Offset", tool.Settings.lineCornerOffset);
            tool.Settings.relativeRotation = EditorGUILayout.Vector3Field("Relative Rotation", tool.Settings.relativeRotation);
            brokenFencePrefab = (GameObject)EditorGUILayout.ObjectField("Broken Fence Prefab", brokenFencePrefab, typeof(GameObject), false);

            if (brokenFencePrefab)
            {
                tool.Settings.randomAltObjs = EditorGUILayout.Toggle("Random Broken Fences?", tool.Settings.randomAltObjs);

                if (tool.Settings.randomAltObjs)
                    tool.Settings.altObjProbability = EditorGUILayout.Slider("Broken Probability", tool.Settings.altObjProbability, 0, 1);
                else
                    tool.Settings.altObjInterval = EditorGUILayout.IntField("Interval", tool.Settings.altObjInterval);
            }

            if (linePoints.Count > 1)
            {
                if (GUILayout.Button("Place Line", GUILayout.Height(25)))
                    OnExit(tool);
            }
        }
        #endregion
    }
}
