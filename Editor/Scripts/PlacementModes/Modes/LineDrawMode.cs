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
        LineModeSettings settings;
        static List<Vector3> linePoints = new();
        static List<GameObject> spawnedObjects = new();
        static List<GameObject> previewObjects = new List<GameObject>();
        static GameObject spawnedObjParent;
        static Dictionary<int, Vector3> cachedRotations = new();
        static Dictionary<int, GameObject> cachedObjects = new();

        public LineDrawMode(PlacementModeSettings settings)
        {
            this.settings = (LineModeSettings)settings;
        }

        #region Placement Mode interface
        public void OnActive(ToolContext context)
        {
            Event e = Event.current;
            HandleInput(context, e);

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

            if (settings.lineMode_chainLines && linePoints.Count >= 2)
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
                    right * settings.lineMode_segmentOffset.x +
                    localUp * settings.lineMode_segmentOffset.y +
                    segmentDir * settings.lineMode_segmentOffset.z;

                startPoint += offset;
            }

            // Parent Obj
            if (spawnedObjParent == null)
            {
                spawnedObjParent = new GameObject($"Line:{context.SelectedPrefab.name}");
                spawnedObjParent.transform.position = startPoint;
            }

            Vector3 endPoint = SceneInteraction.Position;

            Vector3 line = endPoint - startPoint;
            float dist = line.magnitude;
            Vector3 dir = line.normalized;

            // Obj rotation
            Quaternion objRotation = Quaternion.LookRotation(dir, Vector3.up);
            objRotation.eulerAngles += settings.lineMode_relativeRotation;

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

                    if (settings.lineMode_useAltObjs)
                    {
                        if (settings.lineMode_randomAltObjs)
                        {
                            spawnAlt = Random.value <= settings.lineMode_altObjProbability;
                        }
                        else if (settings.lineMode_altObjInterval > 0 && i % settings.lineMode_altObjInterval == 0)
                        {
                            spawnAlt = true;
                        }
                    }

                    if (spawnAlt)
                    {
                        if (settings.lineMode_useCollection)
                        {
                            if (settings.lineMode_altObjsCollection != CollectionName.None)
                            {
                                int rndIndex = UnityEngine.Random.Range(0, settings.lineMode_altCollection.prefabList.Count);
                                objToSpawn = settings.lineMode_altCollection.prefabList[rndIndex];
                            }
                            else
                            {
                                objToSpawn = context.SelectedPrefab;
                            }
                        }
                        else
                        {

                            objToSpawn = settings.lineMode_altObj ? settings.lineMode_altObj : context.SelectedPrefab;
                        }
                    }
                    else
                    {
                        objToSpawn = context.SelectedPrefab;
                    }

                    cachedObjects[i] = objToSpawn;
                }

                if (settings.linemode_ObjRndRotation)
                {
                    if (!cachedRotations.TryGetValue(i, out Vector3 cachedRot))
                    {
                        float xRot = settings.lineMode_rotateOnX ? UnityEngine.Random.Range(settings.lineMode_ObjRndRotationMin, settings.lineMode_ObjRndRotationMax) : 0;
                        float yRot = settings.lineMode_rotateOnY ? UnityEngine.Random.Range(settings.lineMode_ObjRndRotationMin, settings.lineMode_ObjRndRotationMax) : 0;
                        float zRot = settings.lineMode_rotateOnZ ? UnityEngine.Random.Range(settings.lineMode_ObjRndRotationMin, settings.lineMode_ObjRndRotationMax) : 0;

                        cachedRot = new Vector3(xRot, yRot, zRot);
                        cachedRotations[i] = cachedRot;
                    }

                    objRotation.eulerAngles += cachedRot;
                }

                // Calculate object-specific spacing
                float objectSpacing = settings.lineMode_lineSpacing;
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

        public void OnEnter(ToolContext context)
        {
            
        }

        public void OnExit(ToolContext tool)
        {
            spawnedObjects.ForEach(p => GameObject.DestroyImmediate(p, false));
            Reset();
        }
        #endregion


        private void AddLineToUndoStack()
        {
            Undo.RegisterCreatedObjectUndo(spawnedObjParent, "Create Line");
            foreach (var obj in spawnedObjects)
            {
                Undo.RegisterCreatedObjectUndo(obj, "Create Line");
            }
        }

        private void Reset()
        {
            cachedObjects.Clear();
            cachedRotations.Clear();
            ClearPreviewObjects();
            spawnedObjects.Clear();
            linePoints.Clear();
        }

        private void ClearPreviewObjects()
        {
            previewObjects.ForEach(obj => GameObject.DestroyImmediate(obj));
            previewObjects.Clear();
        }

        private void HandleInput(ToolContext context, Event e)
        {
             // Switch might be more appropriate here.
            // Create line point with left mouse click
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
                    }

                    cachedRotations.Clear();
                    cachedObjects.Clear();
                }

                if (!settings.lineMode_chainLines && linePoints.Count >= 2)
                {
                    spawnedObjParent = null;
                    Reset();
                }

                e.Use();
            }

            // Confirm with Enter
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                AddLineToUndoStack();
                spawnedObjParent = null;
                Reset();

                e.Use();
            }

            // Cancel with Escape
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                spawnedObjects.ForEach(p => GameObject.DestroyImmediate(p, false));
                GameObject.DestroyImmediate(spawnedObjParent);
                Reset();

                e.Use();
            }
        }

        #region GUI
        public void SettingsOverlayGUI(ToolContext tool)
        {
            settings.lineMode_lineSpacing = EditorGUILayout.FloatField("Spacing", settings.lineMode_lineSpacing);
            settings.lineMode_relativeRotation = EditorGUILayout.Vector3Field("Relative Rotation", settings.lineMode_relativeRotation);

            settings.linemode_ObjRndRotation = EditorGUILayout.Toggle("Variable Rotation?", settings.linemode_ObjRndRotation);
            if (settings.linemode_ObjRndRotation) 
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
                settings.lineMode_ObjRndRotationMin = EditorGUILayout.FloatField(settings.lineMode_ObjRndRotationMin, GUILayout.Width(50));
                settings.lineMode_ObjRndRotationMin = Mathf.Clamp(settings.lineMode_ObjRndRotationMin, -360f, settings.lineMode_ObjRndRotationMax);

                // MinMax slider
                EditorGUILayout.MinMaxSlider(ref settings.lineMode_ObjRndRotationMin, ref settings.lineMode_ObjRndRotationMax, -360f, 360f);

                // Max float field
                settings.lineMode_ObjRndRotationMax = EditorGUILayout.FloatField(settings.lineMode_ObjRndRotationMax, GUILayout.Width(50));
                settings.lineMode_ObjRndRotationMax = Mathf.Clamp(settings.lineMode_ObjRndRotationMax, settings.lineMode_ObjRndRotationMin, 360f);

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
                settings.lineMode_rotateOnX = GUILayout.Toggle(settings.lineMode_rotateOnX, GUIContent.none, GUILayout.Width(20));

                GUILayout.Space(10);
                GUILayout.Label("Y", GUILayout.Width(15));
                settings.lineMode_rotateOnY = GUILayout.Toggle(settings.lineMode_rotateOnY, GUIContent.none, GUILayout.Width(20));

                GUILayout.Space(10);
                GUILayout.Label("Z", GUILayout.Width(15));
                settings.lineMode_rotateOnZ = GUILayout.Toggle(settings.lineMode_rotateOnZ, GUIContent.none, GUILayout.Width(20));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                GUILayout.Space(25);

            }

            settings.lineMode_chainLines = EditorGUILayout.Toggle("Chain Lines?", settings.lineMode_chainLines);

            if (settings.lineMode_chainLines)
            {
                EditorGUI.indentLevel++;
               settings.lineMode_segmentOffset = EditorGUILayout.Vector3Field("Link Offset",settings.lineMode_segmentOffset);
                EditorGUI.indentLevel--;
            }

           settings.lineMode_useAltObjs = EditorGUILayout.Toggle("Use Alt Objs?",settings.lineMode_useAltObjs);
            EditorGUI.indentLevel++;
            if (settings.lineMode_useAltObjs)
            {
               settings.lineMode_useCollection = EditorGUILayout.Toggle("Use Prefab Alt Collection?",settings.lineMode_useCollection);
                
                if (settings.lineMode_useCollection)
                {
                    settings.lineMode_altObjsCollection = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection",settings.lineMode_altObjsCollection);
                    GUILayout.Space(5);
                }
                else
                {
                   settings.lineMode_altObj = (GameObject)EditorGUILayout.ObjectField("Alt Object Prefab",settings.lineMode_altObj, typeof(GameObject), false);
                }

               settings.lineMode_randomAltObjs = EditorGUILayout.Toggle("Random?",settings.lineMode_randomAltObjs);

                if (settings.lineMode_randomAltObjs)
                    settings.lineMode_altObjProbability = EditorGUILayout.Slider("Probability", settings.lineMode_altObjProbability, 0, 1);
                else
                   settings.lineMode_altObjInterval = EditorGUILayout.IntField("Interval",settings.lineMode_altObjInterval);
            }
            EditorGUI.indentLevel--;
        }

        public string[] ControlsHelpBox => new string[] 
        { 
            "LMB", "Place Point",
            "Enter", "Confirm Line",
            "Escape", "Cancel Drawing Line"
        };
        #endregion
    }
}
