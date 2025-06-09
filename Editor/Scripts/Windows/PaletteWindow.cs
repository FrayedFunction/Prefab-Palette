using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GridBrushBase;

namespace PrefabPalette
{
    /// <summary>
    /// Palette Window.
    /// </summary>
    public class PaletteWindow : EditorWindow
    {
        ToolContext tool;

        Vector2 paletteScrollPosition;
        Vector2 windowScrollPosition;
        float dynamicPrefabIconSize;

        /// <summary>
        /// Opens the main Prefab Palette window via the Unity menu.
        /// </summary>
        [MenuItem("Window/Prefab Palette/Palette")]
        public static void OpenPalette()
        {
            PaletteWindow.OnShowToolWindow(ToolContext.Instance);
        }

        public static void OnShowToolWindow(ToolContext tool)
        {
            var window = GetWindow<PaletteWindow>("Prefab Palette");
            window.tool = tool;
        }

        private void OnEnable()
        {
            tool = ToolContext.Instance;
            VisualPlacer.OnEnable(tool.Settings);
            SceneInteraction.OnEnable(tool.Settings);
            PlacementModeManager.CurrentMode.OnEnter(tool);

            SceneView.duringSceneGui += OnSceneGUI;
            minSize = new Vector2(400, 400);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            VisualPlacer.OnDisable();
            SceneInteraction.OnDisable();
            PlacementModeManager.CurrentMode.OnExit(tool);
        }

        void OnGUI()
        {
            // Select collection
            GUILayout.Space(5);
            tool.Settings.currentCollectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", tool.Settings.currentCollectionName);
            GUILayout.Space(5);

            // if the enum only contains .None
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

            if (tool.Settings.CurrentPrefabCollection != null)
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
            GUILayout.Label($"Palette - {tool.Settings.CurrentPrefabCollection.Name}", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.BeginVertical("box");

            float windowWidth = EditorGUIUtility.currentViewWidth - 10; // Get editor window width (minus padding)

            dynamicPrefabIconSize = Mathf.Clamp(Mathf.Max(windowWidth / tool.Settings.palette_gridColumns - 10, 40), tool.Settings.palette_minScale, tool.Settings.palette_maxScale);

            // Start Scroll View
            paletteScrollPosition = GUILayout.BeginScrollView(paletteScrollPosition); // Set max visible height

            var prefabList = tool.Settings.CurrentPrefabCollection.prefabList;
            int rowCount = Mathf.CeilToInt((float)prefabList.Count / tool.Settings.palette_gridColumns);

            // Calculate the total width of the grid (based on the number of columns and button size)
            float gridWidth = tool.Settings.palette_gridColumns * dynamicPrefabIconSize;

            // Calculate the left padding required to center the grid
            float gridPadding = Mathf.Max((windowWidth - gridWidth) * 0.2f, 0);

            for (int row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(gridPadding);

                for (int col = 0; col < tool.Settings.palette_gridColumns; col++)
                {
                    int index = row * tool.Settings.palette_gridColumns + col;
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
                        bool isSelected = tool.SelectedPrefab == prefab;

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
                            if (tool.SelectedPrefab != null && tool.SelectedPrefab == prefab)
                            {
                                tool.SelectedPrefab = null;
                            }
                            else
                            {
                                tool.SelectedPrefab = prefab;

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
            if (tool != null && tool.SelectedPrefab != null)
            {
                PlacementModeManager.CurrentMode.OnActive(tool);
                VisualPlacer.ShowTarget();
            }
            else
            {
                PlacementModeManager.CurrentMode.OnExit(tool);
                VisualPlacer.Stop();
            }
        }
    }
}
