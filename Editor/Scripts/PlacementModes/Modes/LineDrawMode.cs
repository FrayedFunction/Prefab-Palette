using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace PrefabPalette
{
    public class LineDrawMode : IPlacementMode
    {
        static List<Vector3> linePoints = new();
        static List<GameObject> spawnedObjects = new();
        static List<GameObject> previewObjects = new List<GameObject>();
        static GameObject brokenFencePrefab;
        static GameObject spawnedObjParent;
        static Dictionary<int, Vector3> cachedRotations = new();

        #region Placement Mode interface
        public void OnActive(PrefabPaletteTool tool)
        {
            Event e = Event.current;
            HandleInput(tool, e);

            if (linePoints.Count < 1) return;

            // Debug line (fit this in the line loop, why tf is it here?
            for (int i = 0; i < linePoints.Count; i++)
            {
                if (i != linePoints.Count - 1)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(linePoints[i], linePoints[i + 1], 4f);
                }
            }

            ClearPreviewObjects();

            Vector3 startPoint = linePoints.Last();

            if (tool.Settings.chainLines && linePoints.Count >= 2)
            {
                Vector3 segmentDir = (linePoints.Last() - linePoints[linePoints.Count - 2]).normalized;

                // Pick an arbitrary "up" that's not parallel to the segment
                Vector3 up = Vector3.up;
                if (Mathf.Abs(Vector3.Dot(up, segmentDir)) > 0.99f)
                    up = Vector3.forward;

                // Build a local coordinate frame from the segment direction
                Vector3 right = Vector3.Cross(up, segmentDir).normalized;
                Vector3 localUp = Vector3.Cross(segmentDir, right).normalized;

                // Offset the position using the segment-aligned local space
                Vector3 offset =
                    right * tool.Settings.segmentOffset.x +
                    localUp * tool.Settings.segmentOffset.y +
                    segmentDir * tool.Settings.segmentOffset.z;

                startPoint += offset;
            }

            Vector3 endPoint = SceneInteraction.Position;

            Vector3 line = endPoint - startPoint;
            float dist = line.magnitude;
            Vector3 dir = line.normalized;

            // Obj rotation
            Quaternion objRotation = Quaternion.LookRotation(dir, Vector3.up);
            objRotation.eulerAngles += tool.Settings.relativeRotation;

            // Parent Obj
            if (spawnedObjParent == null)
            {
                spawnedObjParent = new GameObject($"Line:{tool.SelectedPrefab.name}");
                spawnedObjParent.transform.position = startPoint;
            }

            // First Obj
            var firstObj = GameObject.Instantiate(tool.SelectedPrefab, spawnedObjParent.transform);
            firstObj.transform.SetPositionAndRotation(startPoint, objRotation);
            previewObjects.Add(firstObj);

            // Line
            float objectSpacing = tool.Settings.lineSpacing;
            Renderer renderer = tool.SelectedPrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                objectSpacing = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) + tool.Settings.lineSpacing;
            }

            int pointCount = Mathf.FloorToInt(dist / objectSpacing);

            if (pointCount < 1) 
                return;

            for (int i = 1; i <= pointCount; i++)
            {
                Vector3 currentPoint = startPoint + dir * objectSpacing * i;
                Handles.DrawSolidDisc(currentPoint, Vector3.up, 0.2f);

                // Needs to be re-calculated for each object to avoid cumulative results.
                objRotation = Quaternion.LookRotation(dir, Vector3.up);
                objRotation.eulerAngles += tool.Settings.relativeRotation;

                // variable rotation
                if (tool.Settings.lineObjRndRotation)
                {
                    if (!cachedRotations.TryGetValue(i, out var cachedRot))
                    {
                        float xRot = tool.Settings.rotateOnX ? UnityEngine.Random.Range(tool.Settings.lineObjRndRotationMin, tool.Settings.lineObjRndRotationMax) : 0;
                        float yRot = tool.Settings.rotateOnY ? UnityEngine.Random.Range(tool.Settings.lineObjRndRotationMin, tool.Settings.lineObjRndRotationMax) : 0;
                        float zRot = tool.Settings.rotateOnZ ? UnityEngine.Random.Range(tool.Settings.lineObjRndRotationMin, tool.Settings.lineObjRndRotationMax) : 0;

                        cachedRot = new(xRot, yRot, zRot);
                        cachedRotations[i] = cachedRot;
                    }

                    objRotation.eulerAngles += cachedRot;
                }

                var obj = GameObject.Instantiate(tool.SelectedPrefab, spawnedObjParent.transform);
                obj.transform.SetPositionAndRotation(currentPoint, objRotation);
                previewObjects.Add(obj);
            }
        }

        public void OnEnter(PrefabPaletteTool tool)
        {
        }

        public void OnExit(PrefabPaletteTool tool)
        {
            cachedRotations.Clear();
            linePoints.Clear();
            spawnedObjects.ForEach(p => GameObject.DestroyImmediate(p, false));
            spawnedObjects.Clear();
            ClearPreviewObjects();
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

                // Place the last lot of objects if there are any.
                if (linePoints.Count > 0)
                {
                    foreach (var preview in previewObjects)
                    {
                        GameObject placed = GameObject.Instantiate(tool.SelectedPrefab, spawnedObjParent.transform);
                        placed.transform.SetPositionAndRotation(preview.transform.position, preview.transform.rotation);
                        spawnedObjects.Add(placed);
                        cachedRotations.Clear();
                    }
                }

                if (!tool.Settings.chainLines && linePoints.Count >= 2)
                {
                    cachedRotations.Clear();
                    ClearPreviewObjects();
                    spawnedObjects.Clear();
                    linePoints.Clear();
                    spawnedObjParent = null;
                }

                e.Use();
            }

            // Confirm with Enter
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                cachedRotations.Clear();
                ClearPreviewObjects();
                linePoints.Clear();
                spawnedObjects.Clear();
                spawnedObjParent = null;
                e.Use();
            }

            // Cancel with Escape
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                cachedRotations.Clear();
                linePoints.Clear();
                spawnedObjects.ForEach(p => GameObject.DestroyImmediate(p, false));
                spawnedObjects.Clear();
                ClearPreviewObjects();
                GameObject.DestroyImmediate(spawnedObjParent);
                e.Use();
            }
        }

        #region GUI
        public void SettingsGUI(PrefabPaletteTool tool)
        {
            tool.Settings.lineSpacing = EditorGUILayout.FloatField("Spacing", tool.Settings.lineSpacing);
            tool.Settings.relativeRotation = EditorGUILayout.Vector3Field("Relative Rotation", tool.Settings.relativeRotation);

            tool.Settings.lineObjRndRotation = EditorGUILayout.Toggle("Variable Rotation?", tool.Settings.lineObjRndRotation);
            if (tool.Settings.lineObjRndRotation) 
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Range");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // --- Min, Slider, Max ---
                EditorGUILayout.BeginHorizontal();

                // Min float field
                tool.Settings.lineObjRndRotationMin = EditorGUILayout.FloatField(tool.Settings.lineObjRndRotationMin, GUILayout.Width(50));
                tool.Settings.lineObjRndRotationMin = Mathf.Clamp(tool.Settings.lineObjRndRotationMin, -360f, tool.Settings.lineObjRndRotationMax);

                // MinMax slider
                EditorGUILayout.MinMaxSlider(ref tool.Settings.lineObjRndRotationMin, ref tool.Settings.lineObjRndRotationMax, -360f, 360f);

                // Max float field
                tool.Settings.lineObjRndRotationMax = EditorGUILayout.FloatField(tool.Settings.lineObjRndRotationMax, GUILayout.Width(50));
                tool.Settings.lineObjRndRotationMax = Mathf.Clamp(tool.Settings.lineObjRndRotationMax, tool.Settings.lineObjRndRotationMin, 360f);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);


                // Row 2: Axis Toggle Row
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Axis");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label("X", GUILayout.Width(15));
                tool.Settings.rotateOnX = GUILayout.Toggle(tool.Settings.rotateOnX, GUIContent.none, GUILayout.Width(20));

                GUILayout.Space(10);
                GUILayout.Label("Y", GUILayout.Width(15));
                tool.Settings.rotateOnY = GUILayout.Toggle(tool.Settings.rotateOnY, GUIContent.none, GUILayout.Width(20));

                GUILayout.Space(10);
                GUILayout.Label("Z", GUILayout.Width(15));
                tool.Settings.rotateOnZ = GUILayout.Toggle(tool.Settings.rotateOnZ, GUIContent.none, GUILayout.Width(20));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                GUILayout.Space(25);

            }

            tool.Settings.chainLines = EditorGUILayout.Toggle("Chain Lines?", tool.Settings.chainLines);

            if (tool.Settings.chainLines)
            {
                EditorGUI.indentLevel++;
                tool.Settings.segmentOffset = EditorGUILayout.Vector3Field("Link Offset", tool.Settings.segmentOffset);
                EditorGUI.indentLevel--;
            }

            brokenFencePrefab = (GameObject)EditorGUILayout.ObjectField("Broken Fence Prefab", brokenFencePrefab, typeof(GameObject), false);

            if (brokenFencePrefab)
            {
                EditorGUI.indentLevel++;
                tool.Settings.randomAltObjs = EditorGUILayout.Toggle("Random Broken Fences?", tool.Settings.randomAltObjs);

                if (tool.Settings.randomAltObjs)
                    tool.Settings.altObjProbability = EditorGUILayout.Slider("Broken Probability", tool.Settings.altObjProbability, 0, 1);
                else
                    tool.Settings.altObjInterval = EditorGUILayout.IntField("Interval", tool.Settings.altObjInterval);
                EditorGUI.indentLevel--;
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
