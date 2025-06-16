using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace PrefabPalette
{
    /// <summary>
    /// Implements a mode for placing prefabs along a defined line.
    /// It allows users to dynamically draw lines and automatically distributes instances of a selected
    /// prefab (or alternative prefabs) along it, offering controls for spacing, rotation,
    /// and chaining multiple line segments.
    /// </summary>
    public class LineDrawMode : IPlacementMode
    {
        static List<Vector3> linePoints = new();
        static List<GameObject> spawnedObjects = new();
        static List<GameObject> previewObjects = new List<GameObject>();
        static GameObject spawnedObjParent;
        static Dictionary<int, Vector3> cachedRotations = new();
        static Dictionary<int, GameObject> cachedObjects = new();

        #region Placement Mode interface
        public void OnActive(ToolContext tool)
        {
            Event e = Event.current;
            HandleInput(tool, e);

            if (linePoints.Count < 1) return;

            // Debug line (fit this in the line loop, why tf is it here?)
            for (int j = 0; j < linePoints.Count; j++)
            {
                if (j != linePoints.Count - 1)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(linePoints[j], linePoints[j + 1], 4f);
                }
            }

            ClearPreviewObjects();

            Vector3 startPoint = linePoints.Last();

            if (tool.Settings.lineMode_chainLines && linePoints.Count >= 2)
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
                    right * tool.Settings.lineMode_segmentOffset.x +
                    localUp * tool.Settings.lineMode_segmentOffset.y +
                    segmentDir * tool.Settings.lineMode_segmentOffset.z;

                startPoint += offset;
            }

            // Parent Obj
            if (spawnedObjParent == null)
            {
                spawnedObjParent = new GameObject($"Line:{tool.SelectedPrefab.name}");
                spawnedObjParent.transform.position = startPoint;
            }

            Vector3 endPoint = SceneInteraction.Position;

            Vector3 line = endPoint - startPoint;
            float dist = line.magnitude;
            Vector3 dir = line.normalized;

            // Obj rotation
            Quaternion objRotation = Quaternion.LookRotation(dir, Vector3.up);
            objRotation.eulerAngles += tool.Settings.lineMode_relativeRotation;

/*            // First Obj
            var firstObj = GameObject.Instantiate(tool.SelectedPrefab, spawnedObjParent.transform);
            firstObj.transform.SetPositionAndRotation(startPoint, objRotation);
            previewObjects.Add(firstObj);*/

            int i = 1;
            float totalDist = 0f;

            while (totalDist < dist)
            {
                if (!cachedObjects.TryGetValue(i, out GameObject objToSpawn))
                {
                    bool spawnAlt = false;

                    if (tool.Settings.lineMode_useAltObjs)
                    {
                        if (tool.Settings.lineMode_randomAltObjs)
                        {
                            spawnAlt = Random.value <= tool.Settings.lineMode_altObjProbability;
                        }
                        else if (tool.Settings.lineMode_altObjInterval > 0 && i % tool.Settings.lineMode_altObjInterval == 0)
                        {
                            spawnAlt = true;
                        }
                    }

                    if (spawnAlt)
                    {
                        if (tool.Settings.lineMode_useCollection)
                        {
                            if (tool.Settings.lineMode_altObjsCollection != CollectionName.None)
                            {
                                int rndIndex = UnityEngine.Random.Range(0, tool.Settings.lineMode_altCollection.prefabList.Count);
                                objToSpawn = tool.Settings.lineMode_altCollection.prefabList[rndIndex];
                            }
                            else
                            {
                                objToSpawn = tool.SelectedPrefab;
                            }
                        }
                        else
                        {

                            objToSpawn = tool.Settings.lineMode_altObj ? tool.Settings.lineMode_altObj : tool.SelectedPrefab;
                        }
                    }
                    else
                    {
                        objToSpawn = tool.SelectedPrefab;
                    }

                    cachedObjects[i] = objToSpawn;
                }

                if (tool.Settings.linemode_ObjRndRotation)
                {
                    if (!cachedRotations.TryGetValue(i, out Vector3 cachedRot))
                    {
                        float xRot = tool.Settings.lineMode_rotateOnX ? UnityEngine.Random.Range(tool.Settings.lineMode_ObjRndRotationMin, tool.Settings.lineMode_ObjRndRotationMax) : 0;
                        float yRot = tool.Settings.lineMode_rotateOnY ? UnityEngine.Random.Range(tool.Settings.lineMode_ObjRndRotationMin, tool.Settings.lineMode_ObjRndRotationMax) : 0;
                        float zRot = tool.Settings.lineMode_rotateOnZ ? UnityEngine.Random.Range(tool.Settings.lineMode_ObjRndRotationMin, tool.Settings.lineMode_ObjRndRotationMax) : 0;

                        cachedRot = new Vector3(xRot, yRot, zRot);
                        cachedRotations[i] = cachedRot;
                    }

                    objRotation.eulerAngles += cachedRot;
                }

                // Calculate object-specific spacing
                float objectSpacing = tool.Settings.lineMode_lineSpacing;
                Renderer renderer = objToSpawn.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds bounds = renderer.bounds;

                    // Get the rotation-aware size vector
                    Vector3 size = bounds.size;

                    // Project size onto the line direction using the object's transform
                    Vector3 localDir = objToSpawn.transform.InverseTransformDirection(dir.normalized);
                    Vector3 extents = bounds.extents * 2f; // bounds.size is extents * 2, but we want to use directional info
                    Vector3 worldSizeDir = objToSpawn.transform.TransformDirection(Vector3.Scale(extents, localDir.normalized));

                    float projectedSize = Mathf.Abs(Vector3.Dot(worldSizeDir, dir.normalized));
                    objectSpacing += projectedSize;
                }


                // If too far, break early
                if (totalDist + objectSpacing > dist)
                    break;

                Vector3 currentPoint = startPoint + dir * totalDist;
                Handles.DrawSolidDisc(currentPoint, Vector3.up, 0.2f);

                // Instantiate
                var obj = GameObject.Instantiate(objToSpawn, spawnedObjParent.transform);
                obj.transform.SetPositionAndRotation(currentPoint, objRotation);
                previewObjects.Add(obj);

                totalDist += objectSpacing;
                i++;
            }
        }

        public void OnEnter(ToolContext tool)
        {
        }

        public void OnExit(ToolContext tool)
        {
            cachedObjects.Clear();
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

        private void HandleInput(ToolContext tool, Event e)
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
                        GameObject placed = GameObject.Instantiate(preview, spawnedObjParent.transform);
                        placed.transform.SetPositionAndRotation(preview.transform.position, preview.transform.rotation);
                        spawnedObjects.Add(placed);
                        cachedRotations.Clear();
                        cachedObjects.Clear();
                    }
                }

                if (!tool.Settings.lineMode_chainLines && linePoints.Count >= 2)
                {
                    cachedObjects.Clear();
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
                cachedObjects.Clear();
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
                cachedObjects.Clear();
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
        public void SettingsGUI(ToolContext tool)
        {
            tool.Settings.lineMode_lineSpacing = EditorGUILayout.FloatField("Spacing", tool.Settings.lineMode_lineSpacing);
            tool.Settings.lineMode_relativeRotation = EditorGUILayout.Vector3Field("Relative Rotation", tool.Settings.lineMode_relativeRotation);

            tool.Settings.linemode_ObjRndRotation = EditorGUILayout.Toggle("Variable Rotation?", tool.Settings.linemode_ObjRndRotation);
            if (tool.Settings.linemode_ObjRndRotation) 
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
                tool.Settings.lineMode_ObjRndRotationMin = EditorGUILayout.FloatField(tool.Settings.lineMode_ObjRndRotationMin, GUILayout.Width(50));
                tool.Settings.lineMode_ObjRndRotationMin = Mathf.Clamp(tool.Settings.lineMode_ObjRndRotationMin, -360f, tool.Settings.lineMode_ObjRndRotationMax);

                // MinMax slider
                EditorGUILayout.MinMaxSlider(ref tool.Settings.lineMode_ObjRndRotationMin, ref tool.Settings.lineMode_ObjRndRotationMax, -360f, 360f);

                // Max float field
                tool.Settings.lineMode_ObjRndRotationMax = EditorGUILayout.FloatField(tool.Settings.lineMode_ObjRndRotationMax, GUILayout.Width(50));
                tool.Settings.lineMode_ObjRndRotationMax = Mathf.Clamp(tool.Settings.lineMode_ObjRndRotationMax, tool.Settings.lineMode_ObjRndRotationMin, 360f);

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
                tool.Settings.lineMode_rotateOnX = GUILayout.Toggle(tool.Settings.lineMode_rotateOnX, GUIContent.none, GUILayout.Width(20));

                GUILayout.Space(10);
                GUILayout.Label("Y", GUILayout.Width(15));
                tool.Settings.lineMode_rotateOnY = GUILayout.Toggle(tool.Settings.lineMode_rotateOnY, GUIContent.none, GUILayout.Width(20));

                GUILayout.Space(10);
                GUILayout.Label("Z", GUILayout.Width(15));
                tool.Settings.lineMode_rotateOnZ = GUILayout.Toggle(tool.Settings.lineMode_rotateOnZ, GUIContent.none, GUILayout.Width(20));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                GUILayout.Space(25);

            }

            tool.Settings.lineMode_chainLines = EditorGUILayout.Toggle("Chain Lines?", tool.Settings.lineMode_chainLines);

            if (tool.Settings.lineMode_chainLines)
            {
                EditorGUI.indentLevel++;
                tool.Settings.lineMode_segmentOffset = EditorGUILayout.Vector3Field("Link Offset", tool.Settings.lineMode_segmentOffset);
                EditorGUI.indentLevel--;
            }

            tool.Settings.lineMode_useAltObjs = EditorGUILayout.Toggle("Use Alt Objs?", tool.Settings.lineMode_useAltObjs);
            EditorGUI.indentLevel++;
            if (tool.Settings.lineMode_useAltObjs)
            {
                tool.Settings.lineMode_useCollection = EditorGUILayout.Toggle("Use Prefab Alt Collection?", tool.Settings.lineMode_useCollection);
                
                if (tool.Settings.lineMode_useCollection)
                {
                    tool.Settings.lineMode_altObjsCollection = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", tool.Settings.lineMode_altObjsCollection);
                    GUILayout.Space(5);
                }
                else
                {
                    tool.Settings.lineMode_altObj = (GameObject)EditorGUILayout.ObjectField("Alt Object Prefab", tool.Settings.lineMode_altObj, typeof(GameObject), false);
                }

                tool.Settings.lineMode_randomAltObjs = EditorGUILayout.Toggle("Random?", tool.Settings.lineMode_randomAltObjs);

                if (tool.Settings.lineMode_randomAltObjs)
                    tool.Settings.lineMode_altObjProbability = EditorGUILayout.Slider("Probability", tool.Settings.lineMode_altObjProbability, 0, 1);
                else
                    tool.Settings.lineMode_altObjInterval = EditorGUILayout.IntField("Interval", tool.Settings.lineMode_altObjInterval);
            }
            EditorGUI.indentLevel--;
        }

        public string[] ControlsHelpBox => new string[] 
        { 
            "LMB", "Place Point",
            "Escape", "Cancel Drawing Line"
        };
        #endregion
}
}
