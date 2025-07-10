using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Palette Window.
    /// </summary>
    public class PaletteWindow : EditorWindow
    {
        Vector2 paletteScrollPosition;
        Vector2 windowScrollPosition;
        float dynamicPrefabIconSize;

        ToolSettings Settings => ToolContext.Instance.Settings;

        /// <summary>
        /// Opens the main Prefab Palette window.
        /// </summary>
        [MenuItem("Window/Prefab Palette/Palette")]
        public static void OnShowToolWindow()
        {
            var window = GetWindow<PaletteWindow>("Prefab Palette");
        }

        private void OnEnable()
        {
            VisualPlacer.OnEnable();
            SceneInteraction.OnEnable();
            PlacementModeManager.CurrentMode.OnEnter(ToolContext.Instance);

            SceneView.duringSceneGui += OnSceneGUI;
            minSize = new Vector2(400, 400);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            VisualPlacer.OnDisable();
            SceneInteraction.OnDisable();
            PlacementModeManager.CurrentMode.OnExit(ToolContext.Instance);
        }

        void OnGUI()
        {
            // Select collection
            GUILayout.Space(5);
            Settings.currentCollectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", Settings.currentCollectionName);
            GUILayout.Space(5);

            // if the enum doesn't only contain .None
            if (!Enum.GetValues(typeof(CollectionName))
                     .Cast<CollectionName>()
                     .Any(c => c != CollectionName.None))
            {
                EditorGUILayout.HelpBox("You don't have any collections yet!", MessageType.Warning);

                if (GUILayout.Button("Open Menu"))
                {
                    CollectionsManagerWindow.OpenMainWindow();
                    CollectionsListInspector.OpenWindow();

                    GetWindow<PaletteWindow>().Close();
                }

                return;
            }

            if (Settings.CurrentPrefabCollection != null)
            {
                windowScrollPosition = GUILayout.BeginScrollView(windowScrollPosition);
                PaletteGUI();
                GUILayout.EndScrollView();
            }

            GUILayout.Space(20);
        }

        void PaletteGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label($"Palette - {Settings.CurrentPrefabCollection.Name}", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.BeginVertical("box");

            float windowWidth = EditorGUIUtility.currentViewWidth - 10; // Get editor window width (minus padding)

            dynamicPrefabIconSize = Mathf.Clamp(Mathf.Max(windowWidth / Settings.palette_gridColumns - 10, 40), Settings.palette_minScale, Settings.palette_maxScale);

            // Start Scroll View
            paletteScrollPosition = GUILayout.BeginScrollView(paletteScrollPosition); // Set max visible height

            var prefabList = Settings.CurrentPrefabCollection.prefabList;
            int rowCount = Mathf.CeilToInt((float)prefabList.Count / Settings.palette_gridColumns);

            // Calculate the total width of the grid (based on the number of columns and button size)
            float gridWidth = Settings.palette_gridColumns * dynamicPrefabIconSize;

            // Calculate the left padding required to center the grid
            float gridPadding = Mathf.Max((windowWidth - gridWidth) * 0.2f, 0);

            for (int row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(gridPadding);

                for (int col = 0; col < Settings.palette_gridColumns; col++)
                {
                    int index = row * Settings.palette_gridColumns + col;
                    if (index >= prefabList.Count) break;

                    GameObject prefab = prefabList[index];
                    if (prefab != null)
                    {
                        Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                        float padding = dynamicPrefabIconSize * 0.1f; // Scale padding relative to button size
                        float labelHeight = dynamicPrefabIconSize * 0.25f; // Label height scales with button size

                        // Calculate the total clickable rect
                        Rect totalRect = GUILayoutUtility.GetRect(dynamicPrefabIconSize, dynamicPrefabIconSize, GUILayout.Width(dynamicPrefabIconSize));

                        // Calculate inner button rect to be properly centered inside totalRect
                        Rect buttonRect = new Rect(
                            totalRect.x + padding,
                            totalRect.y + padding,
                            totalRect.width - 2 * padding,
                            totalRect.height - 2 * padding
                        );

                        bool isHovering = totalRect.Contains(Event.current.mousePosition);
                        bool isSelected = ToolContext.Instance.SelectedPrefab == prefab;

                        // Draw selection background (centering it with button)
                        if (isSelected)
                        {
                            EditorGUI.DrawRect(totalRect, new Color(0.1f, 0.5f, 1f, 1f)); // Blue highlight
                        }

                        // Draw the button manually instead of GUILayout (fixes scaling)
                        GUI.DrawTexture(buttonRect, preview != null ? preview : EditorGUIUtility.IconContent("Prefab Icon").image, ScaleMode.ScaleToFit);

                        // Handle selection logic
                        if (GUI.Button(totalRect, GUIContent.none, GUIStyle.none))
                        {
                            if (ToolContext.Instance.SelectedPrefab != null && ToolContext.Instance.SelectedPrefab == prefab)
                            {
                                ToolContext.Instance.SelectedPrefab = null;
                            }
                            else
                            {
                                ToolContext.Instance.SelectedPrefab = prefab;

                            }
                        }

                        // Draw label on top when hovered or selected.
                        if (isHovering || isSelected)
                        {
                            Rect labelRect = new Rect(totalRect.x, totalRect.yMax - labelHeight, totalRect.width, labelHeight);
                            EditorGUI.DrawRect(labelRect, new Color(0, 0, 0, 0.6f));
                            GUI.Label(labelRect, prefab.name, new GUIStyle(EditorStyles.whiteLabel) { alignment = TextAnchor.MiddleCenter });
                        }
                    }
                    else
                    {
                        GUILayout.Space(dynamicPrefabIconSize);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView(); // End Scroll View

            GUILayout.EndVertical();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (ToolContext.Instance.SelectedPrefab != null)
            {
                PlacementModeManager.CurrentMode.OnActive(ToolContext.Instance);
                VisualPlacer.ShowTarget();
            }
            else
            {
                PlacementModeManager.CurrentMode.OnExit(ToolContext.Instance);
                VisualPlacer.Stop();
            }
        }
    }
}
