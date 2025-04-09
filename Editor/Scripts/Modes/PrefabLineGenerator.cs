using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class PrefabLineGenerator : IPlacementMode
    {
        static List<Vector3> fencePoints = new List<Vector3>();

        static GameObject brokenFencePrefab;

        static GameObject fenceParentObject;

        private static List<GameObject> spawnedFences = new List<GameObject>();

        public void OnEnter(PrefabPaletteTool tool)
        {
        }

        public void OnActive(PrefabPaletteTool tool)
        {
            if (tool.selectedPrefab == null)
            {
                VisualPlacer.Stop();
                OnExit(tool);
                return;
            }

            VisualPlacer.ShowTarget(tool.Settings.placerColor, tool.Settings.placerRadius);
            Event e = Event.current;

            if (fenceParentObject == null)
                fenceParentObject = new GameObject("Fence");

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt) // Left click
            {
                fencePoints.Add(SceneInteraction.Position);

                // Whenever points change, re-spawn the fence segments
                ClearSpawnedFences();
                CreateFenceSegments(tool);
                SceneView.RepaintAll();

                e.Use();
            }

            // Draw the fence points and segments
            for (int i = 0; i < fencePoints.Count - 1; i++)
            {
                Handles.DrawLine(fencePoints[i], fencePoints[i + 1]);
            }
        }

        public void OnExit(PrefabPaletteTool tool)
        {
            fencePoints.Clear();
            spawnedFences.Clear();

            // Destroy empty if no fences spawned
            if (fenceParentObject != null && fenceParentObject.transform.childCount < 1)
                Editor.DestroyImmediate(fenceParentObject);

            fenceParentObject = null;
        }

        private static void CreateFenceSegments(PrefabPaletteTool tool)
        {
            // For each segment between fence points...
            for (int i = 0; i < fencePoints.Count - 1; i++)
            {
                Vector3 start = fencePoints[i];
                Vector3 end = fencePoints[i + 1];

                // Adjust endpoints at corners
                if (i > 0)
                {
                    Vector3 prev = fencePoints[i - 1];
                    start += GetCornerOffset(prev, start, end, tool.Settings.fenceCornerOffset);
                }
                if (i < fencePoints.Count - 2)
                {
                    Vector3 next = fencePoints[i + 2];
                    end -= GetCornerOffset(start, end, next, tool.Settings.fenceCornerOffset);
                }

                Vector3 direction = (end - start).normalized;
                float distance = Vector3.Distance(start, end);
                int numberOfFences = Mathf.FloorToInt(distance / tool.Settings.fenceSpacing);
                numberOfFences = Mathf.Max(1, numberOfFences);

                // Calculate perpendicular direction (rotate 90° about Y)
                Vector3 perpendicularDirection = new Vector3(direction.z, 0f, -direction.x);

                for (int j = 0; j < numberOfFences; j++)
                {
                    float t = (numberOfFences == 1) ? 0.5f : (float)j / (numberOfFences - 1);
                    Vector3 fencePosition = Vector3.Lerp(start, end, t);

                    // Determine if this fence should be "broken"
                    bool spawnBroken = false;
                    if (brokenFencePrefab != null)
                    {
                        if (tool.Settings.randomBrokenFences)
                        {
                            spawnBroken = (UnityEngine.Random.value < tool.Settings.brokenProbability);
                        }
                        else
                        {
                            // For set intervals, e.g. every brokenInterval-th fence is broken
                            spawnBroken = ((j + 1) % tool.Settings.brokenInterval == 0);
                        }
                    }

                    GameObject prefabToSpawn = tool.selectedPrefab;
                    // If broken fence is desired and we have a broken prefab, use it;
                    // if no broken prefab is assigned, use the fence prefab.
                    if (spawnBroken && brokenFencePrefab != null)
                    {
                        prefabToSpawn = brokenFencePrefab;
                    }

                    GameObject newFence = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);

                    newFence.transform.position = fencePosition;

                    if (Physics.Raycast(fencePosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
                    {
                        fencePosition.y = hit.point.y;
                    }

                    newFence.transform.rotation = Quaternion.LookRotation(perpendicularDirection, Vector3.up);


                    newFence.transform.SetParent(fenceParentObject.transform);
                    Undo.RegisterCreatedObjectUndo(newFence, "Created Fence Segment");
                    spawnedFences.Add(newFence);
                }
            }
        }

        // Returns an offset vector along the bisector for a corner point.
        private static Vector3 GetCornerOffset(Vector3 prevPoint, Vector3 cornerPoint, Vector3 nextPoint, float offset)
        {
            Vector3 dir1 = (cornerPoint - prevPoint).normalized;
            Vector3 dir2 = (nextPoint - cornerPoint).normalized;
            Vector3 bisector = (dir1 + dir2).normalized;
            // Optionally, you can factor in the angle between segments here for a more dynamic offset.
            return bisector * offset;
        }

        // Clears previously spawned fence segments.
        private static void ClearSpawnedFences()
        {
            foreach (GameObject fence in spawnedFences)
            {
                if (fence != null)
                {
                    Undo.DestroyObjectImmediate(fence);
                }
            }
            spawnedFences.Clear();
        }

        public void SettingsGUI(PrefabPaletteTool tool)
        {

            tool.Settings.showModeSettings = EditorGUILayout.Foldout(tool.Settings.showModeSettings, "Settings");
            if (tool.Settings.showModeSettings)
            {
                EditorGUI.indentLevel++;

                tool.Settings.fenceSpacing = EditorGUILayout.FloatField("Spacing", tool.Settings.fenceSpacing);
                tool.Settings.fenceCornerOffset = EditorGUILayout.FloatField("Corner Offset", tool.Settings.fenceCornerOffset);
                brokenFencePrefab = (GameObject)EditorGUILayout.ObjectField("Broken Fence Prefab", brokenFencePrefab, typeof(GameObject), false);

                if (brokenFencePrefab)
                {
                    tool.Settings.randomBrokenFences = EditorGUILayout.Toggle("Random Broken Fences?", tool.Settings.randomBrokenFences);

                    if (tool.Settings.randomBrokenFences)
                    {
                        tool.Settings.brokenProbability = EditorGUILayout.Slider("Broken Probability", tool.Settings.brokenProbability, 0, 1);
                    }
                    else
                    {
                        tool.Settings.brokenInterval = EditorGUILayout.IntField("Interval", tool.Settings.brokenInterval);
                    }
                }
                GUILayout.Space(15);
                
                EditorGUI.indentLevel--;
            }


            if (fencePoints.Count > 1)
            {
                if (GUILayout.Button("Place Line", GUILayout.Height(25)))
                {
                    OnExit(tool);
                }
            }
        }
    }
}
