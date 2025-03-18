using UnityEngine;
using UnityEditor;

namespace PrefabPalette
{
    [InitializeOnLoad]
    public class VisualPlacer
    {
        private static Vector3 previewPosition;
        private static bool isActive = false;
        private static float targetRadius = 1.0f;

        private static Color color;

        static VisualPlacer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isActive) return;

            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                previewPosition = hit.point;
                DrawTargetIndicator(previewPosition, hit.normal);
            }

            sceneView.Repaint();
        }

        private static void DrawTargetIndicator(Vector3 position, Vector3 normal)
        {
            // Ignore colours alpha and force to 1.
            Handles.color = new Color(color.r, color.g, color.b, 1f);

            // Draw outer circle
            Handles.DrawWireDisc(position, normal, targetRadius);

            // Draw inner solid circle (30% of targetRadius)
            Handles.color = new Color(color.r, color.g, color.b, 0.25f);
            Handles.DrawSolidDisc(position, normal, targetRadius * 0.3f);
        }

/*        private static void DrawWireMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (wireMaterial == null)
            {
                wireMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                wireMaterial.hideFlags = HideFlags.HideAndDontSave;
                wireMaterial.SetInt("_ZWrite", 1);
                wireMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                wireMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            }

            wireMaterial.SetPass(0);
            wireMaterial.color = color;

            GL.wireframe = true;
            Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(position, rotation, scale));
            GL.wireframe = false;
        }

        public static void SetPreviewPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                StopPreview();
                return;
            }

            MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null)
            {
                previewMesh = meshFilter.sharedMesh;
                isActive = true;
            }
            else
            {
                Debug.LogWarning("Prefab does not contain a MeshFilter!");
                StopPreview();
            }
        }*/

        public static void Start()
        {
            isActive = true;
        }
        public static void Stop()
        {
            isActive = false;

            SceneView.RepaintAll();
        }

        public static void SetColor(Color color)
        {
            VisualPlacer.color = color;
            SceneView.RepaintAll();
        }

        public static void SetTargetRadius(float radius)
        {
            targetRadius = Mathf.Max(0.1f, radius); // Prevents zero or negative values
            SceneView.RepaintAll();
        }
    }
}
