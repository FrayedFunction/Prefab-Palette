using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/* TODO:
 * Fence Placer: 
 * - Show line while editing
 * 
 * Prop Placer:
 */

namespace PrefabPalette
{
    [Serializable]

    /// <summary>
    /// Main tool window.
    /// </summary>
    public class PrefabPaletteTool : EditorWindow
    {
        const string toolWindowPath = "Window/Prefab Palette";

        PrefabCollection currentPrefabCollection;
        CollectionName collectionNameDropdown;

        GUIContent[] toolbarButtons;

        static CollectionsList collectionsList;

        GameObject selectedPrefab;
        GameObject currentPlacedObject;
        Vector2 paletteScrollPosition;
        Vector2 windowScrollPosition;
        int gridColumns = 4;
        float dynamicPrefabIconSize;
        float paletteHeight = 250;

        bool isRotating = false;
        float rotationSpeed = 2f;
        float minPaletteScale = 50f;
        float maxPaletteScale = 300f;
        bool showPaletteSettings = false;
        bool showPlacementSettings = false;
        Vector3 placementOffset = Vector3.zero;
        Color previewColor = Color.white;
        float placerRadius = 0.2f;
        bool isNameDropdownActive = true;
        bool alignWithSurface = false;
        Vector3 lastSurfaceNormal;

        /// <summary>
        /// Returns a list of all saved prefab collections.
        /// </summary>
        public List<PrefabCollection> GetAllCollectionsInFolder =>
            AssetDatabase.FindAssets($"t:{nameof(PrefabCollection)}", new[] { PathDr.GetCollectionsFolder })
            .Select(guid => AssetDatabase.LoadAssetAtPath<PrefabCollection>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();

        [MenuItem(toolWindowPath)]
        public static void OnShowToolWindow()
        {
            GetWindow<PrefabPaletteTool>("Prefab Palette");
        }

        void OnGUI()
        {
            GUILayout.Label("Prefab Palette", EditorStyles.largeLabel);

            GUI.enabled = isNameDropdownActive;
            // Select collection to show.
            collectionNameDropdown = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", collectionNameDropdown);

            // Force the name dropdown to None to avoid regenerating assets accidentally if the list inspector is open
            if (HasOpenInstances<CollectionsListInspector>())
            {
                collectionNameDropdown = CollectionName.None;
                isNameDropdownActive = false;
                EditorGUILayout.HelpBox("Close the Collections Inspector window to choose a collection", MessageType.Warning);
                return;
            }
            else
            {
                isNameDropdownActive = true;
            }

            if (GUILayout.Button("Edit List"))
            {
                if (collectionsList == null)
                {
                    collectionsList = AssetDatabase.FindAssets($"t:{nameof(CollectionsList)}", new[] { PathDr.GetCollectionsFolder })
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid)) // Convert GUID to path
                        .Select(path => AssetDatabase.LoadAssetAtPath<CollectionsList>(path)) // Load asset
                        .FirstOrDefault(asset => asset != null);

                    if (collectionsList != null)
                    {
                        // Open the list inspector window if found
                        CollectionsListInspector.OpenWindow(collectionsList, this);
                        return;
                    }

                    // Create a new asset if it doesn't exist
                    CollectionsList asset = ScriptableObject.CreateInstance<CollectionsList>();
                    string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{PathDr.GetCollectionsFolder}/CollectionNamesList.asset");
                    CreateScriptableObject(asset, assetPath);

                    // Delay the window opening until after the asset database refresh
                    EditorApplication.delayCall += () =>
                    {
                        collectionsList = AssetDatabase.LoadAssetAtPath<CollectionsList>(assetPath);
                        CollectionsListInspector.OpenWindow(collectionsList, this);
                    };
                }
                else
                {
                    // Open window immediately if an asset already exists
                    CollectionsListInspector.OpenWindow(collectionsList, this);
                }
            }

            // if the enum only contains .None
            if (!Enum.GetValues(typeof(CollectionName))
                     .Cast<CollectionName>()
                     .Any(c => c != CollectionName.None)) {

                EditorGUILayout.HelpBox("Use the button to edit the collections list", MessageType.Warning);
                return;
            }

            currentPrefabCollection = GetPrefabCollection(collectionNameDropdown);

            if (currentPrefabCollection != null)
            {
                windowScrollPosition = GUILayout.BeginScrollView(windowScrollPosition);

                PaletteGUI();
                GUILayout.EndScrollView();
            }

            GUILayout.Space(20);
        }

        void PaletteGUI()
        {
            GUILayout.Label("Palette", EditorStyles.boldLabel);

            // Create a foldout for "Palette Settings"
            showPaletteSettings = EditorGUILayout.Foldout(showPaletteSettings, "Palette Settings");

            // If the foldout is expanded, display the settings
            if (showPaletteSettings)
            {
                gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Palette Columns", gridColumns)); // Ensure at least 1 column
                paletteHeight = Mathf.Max(100, EditorGUILayout.FloatField("Palette Height", paletteHeight));
                minPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Palette Scale", minPaletteScale), 50f, maxPaletteScale);
                maxPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Palette Scale", maxPaletteScale), minPaletteScale, 500f);
            }

            GUILayout.Space(2);

            showPlacementSettings = EditorGUILayout.Foldout(showPlacementSettings, "Placement Settings");

            if (showPlacementSettings) 
            {
                previewColor = EditorGUILayout.ColorField("Placer Color", previewColor);
                placerRadius = Mathf.Max( 0.01f, EditorGUILayout.FloatField("Placer Visual Radius", placerRadius));
                rotationSpeed = EditorGUILayout.Slider("Rotation Speed", rotationSpeed, 0.1f, 5);
                placementOffset = EditorGUILayout.Vector3Field("Placement Offset", placementOffset);
                alignWithSurface = EditorGUILayout.Toggle("Align with surface?", alignWithSurface);
            }

            var prefabList = currentPrefabCollection.prefabList;

            if (GUILayout.Button("Edit Prefab Collection"))
            {
                // Inspect the currentPrefabCollection scriptable object
                PrefabCollectionInspector.OpenEditWindow(currentPrefabCollection);
            }

            // Placement mode toolbar
            int selectedIndex = (int)SceneRaycastHelper.CurrentPlacementMode;

            selectedIndex = GUILayout.Toolbar(selectedIndex, toolbarButtons, GUILayout.Height(30));

            SceneRaycastHelper.CurrentPlacementMode = (SceneRaycastHelper.PlacementMode)selectedIndex;

            if (selectedPrefab != null)
            {
                if (GUILayout.Button("Stop Placing Prefabs", GUILayout.Height(50)))
                {
                    selectedPrefab = null;
                }
            }

            float windowWidth = EditorGUIUtility.currentViewWidth - 10; // Get editor window width (minus padding)

            dynamicPrefabIconSize = Mathf.Clamp(Mathf.Max(windowWidth / gridColumns - 10, 40), minPaletteScale, maxPaletteScale);

            // Start Scroll View
            paletteScrollPosition = GUILayout.BeginScrollView(paletteScrollPosition); // Set max visible height

            int rowCount = Mathf.CeilToInt((float)prefabList.Count / gridColumns);

            // Calculate the total width of the grid (based on the number of columns and button size)
            float gridWidth = gridColumns * dynamicPrefabIconSize;

            // Calculate the left padding required to center the grid
            float gridPadding = Mathf.Max((windowWidth - gridWidth) * 0.2f, 0);

            for (int row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(gridPadding);

                for (int col = 0; col < gridColumns; col++)
                {
                    int index = row * gridColumns + col;
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
                        bool isSelected = selectedPrefab == prefab;

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
                            selectedPrefab = prefab;
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
        }

        /// <summary>
        /// Returns prefab collection object by name except for CollectionName.None
        /// </summary>
        private PrefabCollection GetPrefabCollection(CollectionName name)
        {
            if (name == CollectionName.None) return null;

            if (currentPrefabCollection != null && currentPrefabCollection.Name == name)
                return currentPrefabCollection;

            selectedPrefab = null;

            foreach (var collection in GetAllCollectionsInFolder)
            {
                if (collection != null && collection.Name == name)
                {
                    return collection;
                }
            }

            // If no matching collection is found, create a new one
            PrefabCollection asset = ScriptableObject.CreateInstance<PrefabCollection>();
            asset.Name = name; // Assigns string-based enum reference

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{PathDr.GetCollectionsFolder}/{name}_PrefabCollection.asset");
            CreateScriptableObject(asset, assetPath);

            return asset;
        }

        /// <summary>
        /// Creates an instance of <paramref name="so"/> at <paramref name="assetPath"/> and adds it to the Asset Database
        /// </summary>
        private static void CreateScriptableObject(ScriptableObject so, string assetPath)
        {
            AssetDatabase.CreateAsset(so, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (selectedPrefab != null)
            {
                if (!isRotating)
                {
                    VisualPlacer.SetColor(previewColor);
                    VisualPlacer.SetRadius(placerRadius);
                    VisualPlacer.Start();
                }
                else
                {
                    VisualPlacer.Stop();
                }

                HandlePrefabPlacement();
            }
            else
            {
                VisualPlacer.Stop();
            }
        }

        private void HandlePrefabPlacement()
        {
            Event e = Event.current;

            // Place object on left click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                lastSurfaceNormal = SceneRaycastHelper.SurfaceNormal;

                currentPlacedObject = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                currentPlacedObject.transform.SetPositionAndRotation(SceneRaycastHelper.Position + placementOffset, alignWithSurface ? Quaternion.FromToRotation(Vector3.up, lastSurfaceNormal) : Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(currentPlacedObject, "Placed Prop");

                e.Use();
            }

            // Rotate while holding the mouse button
            if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && currentPlacedObject != null)
            {
                if (!isRotating)
                {
                    isRotating = true;
                }

                float angle = e.delta.x * rotationSpeed;
                Vector3 axis = alignWithSurface ? lastSurfaceNormal : Vector3.up;
                currentPlacedObject.transform.Rotate(axis, angle, Space.World);
                e.Use();
            }

            // Stop rotating on mouse release
            if (e.type == EventType.MouseUp && e.button == 0 && isRotating)
            {
                isRotating = false;
                currentPlacedObject = null;
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            toolbarButtons = new GUIContent[]
            {
                    new GUIContent(EditorGUIUtility.IconContent("d_MoveTool").image, "Free Mode"),
                    new GUIContent(EditorGUIUtility.IconContent("SceneViewSnap").image, "Snapping Mode"),
                    new GUIContent(EditorGUIUtility.IconContent("d_Profiler.NetworkOperations").image, "Line Mode")

            };
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            VisualPlacer.Stop();
        }
    }
}
