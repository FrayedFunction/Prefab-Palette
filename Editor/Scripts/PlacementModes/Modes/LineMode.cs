using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class LineMode : IPlacementMode
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
            Event e = Event.current;

            if (fenceParentObject == null)
                fenceParentObject = new GameObject("Fence");

            Vector3 currentMousePosition = SceneInteraction.Position;

            // Add point on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                fencePoints.Add(currentMousePosition);
                e.Use();
            }

            // Confirm with Enter
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                CreateFenceSegments(tool, fencePoints);
                OnExit(tool);
                e.Use();
            }

            // Cancel with Escape
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                fencePoints.Clear();
                e.Use();
            }

            List<Vector3> previewPoints = new List<Vector3>(fencePoints);
            previewPoints.Add(currentMousePosition);
            CreateFenceSegments(tool, previewPoints);

            // Draw lines
            List<Vector3> drawPoints = new List<Vector3>(fencePoints);
            if (fencePoints.Count > 0)
                drawPoints.Add(currentMousePosition);

            for (int i = 0; i < drawPoints.Count - 1; i++)
            {
                Handles.DrawLine(drawPoints[i], drawPoints[i + 1]);
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

        private static void CreateFenceSegments(PrefabPaletteTool tool, List<Vector3> points)
        {
            List<GameObject> pool = spawnedFences;
            EnsureFencePool(pool, points, tool);

            int poolIndex = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[i + 1];

                if (i > 0)
                {
                    Vector3 prev = points[i - 1];
                    start += GetCornerOffset(prev, start, end, tool.Settings.fenceCornerOffset);
                }

                if (i < points.Count - 2)
                {
                    Vector3 next = points[i + 2];
                    end -= GetCornerOffset(start, end, next, tool.Settings.fenceCornerOffset);
                }

                Vector3 direction = (end - start).normalized;
                float distance = Vector3.Distance(start, end);
                int numberOfFences = Mathf.FloorToInt(distance / tool.Settings.fenceSpacing);
                numberOfFences = Mathf.Max(1, numberOfFences);

                Vector3 perp = new Vector3(direction.z, 0f, -direction.x);

                for (int j = 0; j < numberOfFences; j++)
                {
                    float t = (numberOfFences == 1) ? 0.5f : (float)j / (numberOfFences - 1);
                    Vector3 pos = Vector3.Lerp(start, end, t);

                    if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
                        pos.y = hit.point.y;

                    GameObject fence = pool[poolIndex++];

                    if (brokenFencePrefab != null)
                    {
                        bool spawnBroken = tool.Settings.randomBrokenFences
                            ? UnityEngine.Random.value < tool.Settings.brokenProbability
                            : ((j + 1) % tool.Settings.brokenInterval == 0);

                        if (spawnBroken)
                            fence = brokenFencePrefab;
                    }

                    fence.transform.position = pos;
                    fence.transform.rotation = Quaternion.LookRotation(perp, Vector3.up);
                    fence.transform.SetParent(fenceParentObject.transform, false);
                    fence.SetActive(true);

                    Undo.RegisterCreatedObjectUndo(fence, "Created Fence Segment");
                }
            }
        }

        private static void EnsureFencePool(List<GameObject> pool, List<Vector3> points, PrefabPaletteTool tool)
        {
            int totalFenceCount = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[i + 1];
                float distance = Vector3.Distance(start, end);
                totalFenceCount += Mathf.Max(1, Mathf.FloorToInt(distance / tool.Settings.fenceSpacing));
            }

            while (pool.Count < totalFenceCount)
            {
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(tool.SelectedPrefab);
                obj.SetActive(false);
                spawnedFences.Add(obj);
            }
        }

        private static Vector3 GetCornerOffset(Vector3 prevPoint, Vector3 cornerPoint, Vector3 nextPoint, float offset)
        {
            Vector3 dir1 = (cornerPoint - prevPoint).normalized;
            Vector3 dir2 = (nextPoint - cornerPoint).normalized;
            Vector3 bisector = (dir1 + dir2).normalized;
            return bisector * offset;
        }

        public void SettingsGUI(PrefabPaletteTool tool)
        {
            tool.Settings.fenceSpacing = EditorGUILayout.FloatField("Spacing", tool.Settings.fenceSpacing);
            tool.Settings.fenceCornerOffset = EditorGUILayout.FloatField("Corner Offset", tool.Settings.fenceCornerOffset);
            brokenFencePrefab = (GameObject)EditorGUILayout.ObjectField("Broken Fence Prefab", brokenFencePrefab, typeof(GameObject), false);

            if (brokenFencePrefab)
            {
                tool.Settings.randomBrokenFences = EditorGUILayout.Toggle("Random Broken Fences?", tool.Settings.randomBrokenFences);

                if (tool.Settings.randomBrokenFences)
                    tool.Settings.brokenProbability = EditorGUILayout.Slider("Broken Probability", tool.Settings.brokenProbability, 0, 1);
                else
                    tool.Settings.brokenInterval = EditorGUILayout.IntField("Interval", tool.Settings.brokenInterval);
            }

            if (fencePoints.Count > 1)
            {
                if (GUILayout.Button("Place Line", GUILayout.Height(25)))
                    OnExit(tool);
            }
        }
    }
}
