using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PrefabPalette
{
    public class FencePlacerOG : Editor
    {
        GameObject fencePrefab;
        List<Vector3> fencePoints = new List<Vector3>();
        bool isPlacingFence = false;
        float fenceSpacing = 1;
        float fenceCornerOffset = 0.5f;
        GameObject brokenFencePrefab;
        bool randomBrokenFences = true;
        float brokenProbability = 0.5f;
        int brokenInterval = 4;
        GameObject fenceParentObject;
        private List<GameObject> spawnedFences = new List<GameObject>();

        void FencePlacerGUI()
        {
            GUILayout.Label("Fence Placer - BETA", EditorStyles.boldLabel);
            fencePrefab = (GameObject)EditorGUILayout.ObjectField("Fence Prefab", fencePrefab, typeof(GameObject), false);
            fenceSpacing = EditorGUILayout.FloatField("Spacing", fenceSpacing);
            fenceCornerOffset = EditorGUILayout.FloatField("Corner Offset", fenceCornerOffset);
            brokenFencePrefab = (GameObject)EditorGUILayout.ObjectField("Broken Fence Prefab", brokenFencePrefab, typeof(GameObject), false);

            if (brokenFencePrefab)
            {
                randomBrokenFences = EditorGUILayout.Toggle("Random Broken Fences?", randomBrokenFences);

                if (randomBrokenFences)
                {
                    brokenProbability = EditorGUILayout.Slider("Broken Probability", brokenProbability, 0, 1);
                }
                else
                {
                    brokenInterval = EditorGUILayout.IntField("Interval", brokenInterval);
                }
            }

            if (GUILayout.Button(isPlacingFence ? "Stop Placing Fence" : "Start Placing Fence"))
            {
                isPlacingFence = !isPlacingFence;
                if (!isPlacingFence)
                {
                    fencePoints.Clear();
                    spawnedFences.Clear();

                    // Destroy empty if no fences spawned
                    if (fenceParentObject != null && fenceParentObject.transform.childCount < 1)
                        DestroyImmediate(fenceParentObject);

                    fenceParentObject = null;
                }
                else
                {
                    fenceParentObject = new GameObject("Fence");
                }
            }

            if (isPlacingFence)
            {
                GUILayout.Label("Click to add fence points");
            }
        }
        private void HandleFencePlacement()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt) // Left click
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    fencePoints.Add(hit.point);
                    // Whenever points change, re-spawn the fence segments
                    ClearSpawnedFences();
                    CreateFenceSegments();
                    SceneView.RepaintAll();
                }

                e.Use();
            }

            // Draw the fence points and segments
            for (int i = 0; i < fencePoints.Count - 1; i++)
            {
                Handles.DrawLine(fencePoints[i], fencePoints[i + 1]);
            }
        }
        private void CreateFenceSegments()
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
                    start += GetCornerOffset(prev, start, end, fenceCornerOffset);
                }
                if (i < fencePoints.Count - 2)
                {
                    Vector3 next = fencePoints[i + 2];
                    end -= GetCornerOffset(start, end, next, fenceCornerOffset);
                }

                Vector3 direction = (end - start).normalized;
                float distance = Vector3.Distance(start, end);
                int numberOfFences = Mathf.FloorToInt(distance / fenceSpacing);
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
                        if (randomBrokenFences)
                        {
                            spawnBroken = (UnityEngine.Random.value < brokenProbability);
                        }
                        else
                        {
                            // For set intervals, e.g. every brokenInterval-th fence is broken
                            spawnBroken = ((j + 1) % brokenInterval == 0);
                        }
                    }

                    GameObject prefabToSpawn = fencePrefab;
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
        private Vector3 GetCornerOffset(Vector3 prevPoint, Vector3 cornerPoint, Vector3 nextPoint, float offset)
        {
            Vector3 dir1 = (cornerPoint - prevPoint).normalized;
            Vector3 dir2 = (nextPoint - cornerPoint).normalized;
            Vector3 bisector = (dir1 + dir2).normalized;
            // Optionally, you can factor in the angle between segments here for a more dynamic offset.
            return bisector * offset;
        }

        // Clears previously spawned fence segments.
        private void ClearSpawnedFences()
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

        private void OnSceneGUI()
        {
            if (isPlacingFence && fencePrefab != null)
            {
                HandleFencePlacement();
            }
        }
    }
}
