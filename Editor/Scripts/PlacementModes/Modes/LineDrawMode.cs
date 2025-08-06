using Codice.Client.BaseCommands.BranchExplorer;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEditor;
using UnityEngine;

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
        static Dictionary<int, Vector3> rotationCache = new();

        public LineDrawMode(PlacementModeSettings settings)
        {
            this.settings = (LineModeSettings)settings;
        }

        #region Placement Mode Lifecycle
        public void OnActive(ToolContext context)
        {
            Event e = Event.current;
            HandleInput(context, e);

            if (linePoints.Count < 1) return;

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

            float totalDist = 0f;
            int i = 0;
            while (totalDist < dist)
            {
                // Obj rotation
                Quaternion objRotation = Quaternion.LookRotation(dir, Vector3.up);
                var rot = ObjectRotation(i);
                objRotation *= Quaternion.Euler(rot);

                var obj = settings.lineMode_useAltObjs ? SpawnAltObjectMaybe(context, i) : context.SelectedPrefab;
                // Spawn preview instance
                var objInstance = GameObject.Instantiate(obj, spawnedObjParent.transform);
                objInstance.transform.SetPositionAndRotation(startPoint + dir * totalDist, objRotation);
                previewObjects.Add(objInstance);

                float objectSpacing = CalcSpacing(objInstance, settings.lineMode_lineSpacing);

                if (totalDist + objectSpacing > dist)
                    break;

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

        private float CalcSpacing(GameObject obj, float userSpacing)
        {
            // Use bounds of the shared mesh on the filter instead of the renderer.
            // Renderer.bounds was causing issues because it's in world space, where as Mesh.bounds is local.
            var meshFilter = obj.GetComponent<MeshFilter>();

            // Fallback to user spacing if no mesh filter
            if (!meshFilter) 
            {
                Debug.LogWarning($"Obj: {obj.name} has no mesh filter... Using user spacing as fallback.");
                return userSpacing; 
            }
            
            
            Vector3 size = meshFilter.sharedMesh.bounds.size;
            float longestAxis = Mathf.Max(size.x, size.z);

            return longestAxis + userSpacing; 
        }

        private Vector3 ObjectRotation(int index)
        {
            if (rotationCache.ContainsKey(index))
            {
                return rotationCache[index];
            }

            var newRot = settings.linemode_ObjRndRotation ? settings.lineMode_relativeRotation + CalcRotationVariation() : settings.lineMode_relativeRotation;
            rotationCache.Add(index, newRot);
            return newRot;
        }

        private Vector3 CalcRotationVariation()
        {
            return new Vector3(
                settings.lineMode_rotateOnX ? Random.Range(settings.lineMode_ObjRndRotationMin, settings.lineMode_ObjRndRotationMax) : 0,
                settings.lineMode_rotateOnY ? Random.Range(settings.lineMode_ObjRndRotationMin, settings.lineMode_ObjRndRotationMax) : 0,
                settings.lineMode_rotateOnZ ? Random.Range(settings.lineMode_ObjRndRotationMin, settings.lineMode_ObjRndRotationMax) : 0);
        }

        private GameObject SpawnAltObjectMaybe(ToolContext context, int currentIndex)
        {
            bool shouldSpawnAlt = settings.lineMode_randomAltObjs ? 
                Random.value < settings.lineMode_altObjProbability : 
                currentIndex % settings.lineMode_altObjInterval == 0;

            if (!shouldSpawnAlt) return context.SelectedPrefab;

            if (settings.lineMode_useCollection && settings.lineMode_altCollectionName != CollectionName.None)
            {
                var prefabList = settings.lineMode_altCollection.prefabList;
                int prefabCount = prefabList.Count;
                return prefabCount > 0 ? prefabList[Random.Range(0, prefabCount)] : context.SelectedPrefab;
            }
            
            return settings.lineMode_altObj ? settings.lineMode_altObj : context.SelectedPrefab;
        }

        private void AddLineToUndoStack()
        {
            if (spawnedObjParent) 
                Undo.RegisterCreatedObjectUndo(spawnedObjParent, "Create Line");

            foreach (var obj in spawnedObjects)
            {
                Undo.RegisterCreatedObjectUndo(obj, "Create Line");
            }
        }

        private void PlaceLine()
        {
            AddLineToUndoStack();
            Reset();
        }

        private void Reset()
        {
            ClearPreviewObjects();
            spawnedObjects.Clear();
            linePoints.Clear();
            spawnedObjParent = null;
            rotationCache.Clear();
        }

        private void ClearPreviewObjects()
        {
            previewObjects.ForEach(obj => GameObject.DestroyImmediate(obj));
            previewObjects.Clear();
        }

        private void HandleInput(ToolContext context, Event e)
        {
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
                }

                if (!settings.lineMode_chainLines && linePoints.Count >= 2)
                {
                    PlaceLine();
                }

                e.Use();
            }

            if (e.type == EventType.KeyDown)
            {
                // Confirm with Enter
                if (e.keyCode == KeyCode.Return)
                {
                    PlaceLine();

                    e.Use();
                }

                // Cancel with Escape
                if (e.keyCode == KeyCode.Escape)
                {
                    spawnedObjects.ForEach(p => GameObject.DestroyImmediate(p, false));
                    GameObject.DestroyImmediate(spawnedObjParent);
                    Reset();

                    e.Use();
                }
            }
        }

        #region GUI
        public void SettingsOverlayGUI(ToolContext tool)
        {
            settings.lineMode_lineSpacing = EditorGUILayout.FloatField("Spacing", Mathf.Clamp(settings.lineMode_lineSpacing, 1, 1000));
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
                    settings.lineMode_altCollectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection",settings.lineMode_altCollectionName);
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
