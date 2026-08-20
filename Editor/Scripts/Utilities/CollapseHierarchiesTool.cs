using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace PrefabPalette
{
    /// <summary>
    /// Adds buttons for collapsing hierarchies.
    /// </summary>
    [InitializeOnLoad]
    public class CollapseHierarchiesTool
    {
        // Cached reflection references
        private static System.Type hierarchyType;
        private static MethodInfo setExpandedRecursive;
        private static EditorWindow hierarchyWindow;

        static CollapseHierarchiesTool() { }

        private static bool EnsureReflectionReady()
        {
            if (setExpandedRecursive != null && hierarchyWindow != null)
                return true;

            if (hierarchyType == null)
                hierarchyType = typeof(EditorWindow).Assembly
                    .GetType("UnityEditor.SceneHierarchyWindow");

            if (hierarchyType == null)
            {
                Debug.LogError("Could not find SceneHierarchyWindow type.");
                return false;
            }

            if (setExpandedRecursive == null)
                setExpandedRecursive = hierarchyType.GetMethod(
                    "SetExpandedRecursive",
                    BindingFlags.Public | BindingFlags.Instance);

            if (setExpandedRecursive == null)
            {
                Debug.LogError("Could not find SetExpandedRecursive method.");
                return false;
            }

            // Find the hierarchy window without focusing it every call
            if (hierarchyWindow == null)
            {
                var windows = Resources.FindObjectsOfTypeAll(hierarchyType);
                if (windows.Length > 0)
                    hierarchyWindow = windows[0] as EditorWindow;
            }

            // Last resort: open it once if it isn't already open
            if (hierarchyWindow == null)
            {
                EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                hierarchyWindow = EditorWindow.focusedWindow;
            }

            return hierarchyWindow != null;
        }

        // Batch expand / collapse helpers

        /// <summary>
        /// Collect every InstanceID in the subtree, then call SetExpandedRecursive
        /// once per root rather than once per node.
        /// </summary>
        private static void ApplyToHierarchy(Transform root, bool expand)
        {
            if (!EnsureReflectionReady()) return;

            // SetExpandedRecursive already walks the full subtree internally,
            // so one call per root is sufficient — no manual recursion needed.
            setExpandedRecursive.Invoke(
                hierarchyWindow,
                new object[] { GetObjectId(root.gameObject), expand });
        }

        private static void ApplyToObjects(IEnumerable<GameObject> roots, bool expand)
        {
            if (!EnsureReflectionReady()) return;

            foreach (var root in roots)
                setExpandedRecursive.Invoke(
                    hierarchyWindow,
                    new object[] { GetObjectId(root), expand });

            // Single repaint after all changes
            hierarchyWindow.Repaint();
        }

#if UNITY_6000_4_OR_NEWER
        private static EntityId GetObjectId(GameObject gameObject)
        {
            return gameObject.GetEntityId();
        }
#else
        private static int GetObjectId(GameObject gameObject)
        {
            return gameObject.GetInstanceID();
        }
#endif
        // Menu Items

        [MenuItem("Window/Prefab Palette/Hierarchy Cleaner/Collapse Selected", false, 300)]
        [MenuItem("GameObject/Collapse Hierarchy", false, 0)]
        public static void CollapseSelectedHierarchy()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) { Debug.LogWarning("No GameObject selected."); return; }

            ApplyToHierarchy(selected.transform, false);
            Debug.Log($"Collapsed hierarchy for: {selected.name}");
        }

        [MenuItem("GameObject/Collapse Hierarchy", true)]
        private static bool ValidateCollapseSelectedHierarchy() =>
            Selection.activeGameObject != null;

        [MenuItem("Window/Prefab Palette/Hierarchy Cleaner/Collapse All", false, 300)]
        public static void CollapseAllHierarchies()
        {
            if (!EnsureReflectionReady()) return;

            int sceneCount = SceneManager.sceneCount;
            int totalRoots = 0;

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                GameObject[] roots = scene.GetRootGameObjects();
                ApplyToObjects(roots, false);
                totalRoots += roots.Length;
            }

            hierarchyWindow?.Repaint();
            Debug.Log($"Collapsed all hierarchies across {sceneCount} scene(s). Total root objects: {totalRoots}");
        }

        [MenuItem("Window/Prefab Palette/Hierarchy Cleaner/Collapse All - Current Scene", false, 300)]
        public static void CollapseCurrentSceneOnly()
        {
            Scene active = SceneManager.GetActiveScene();
            if (!active.isLoaded) { Debug.LogWarning("Active scene is not loaded."); return; }

            GameObject[] roots = active.GetRootGameObjects();
            ApplyToObjects(roots, false);

            Debug.Log($"Collapsed hierarchy for scene: {active.name}. Root objects: {roots.Length}");
        }

        [MenuItem("GameObject/Expand Hierarchy", false, 0)]
        public static void ExpandSelectedHierarchy()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) { Debug.LogWarning("No GameObject selected."); return; }

            ApplyToHierarchy(selected.transform, true);
            Debug.Log($"Expanded hierarchy for: {selected.name}");
        }

        [MenuItem("GameObject/Expand Hierarchy", true)]
        private static bool ValidateExpandSelectedHierarchy() =>
            Selection.activeGameObject != null;
    }
}
#endif
