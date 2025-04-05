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

        public ToolSettings Settings { get; private set; }

        PrefabCollection currentPrefabCollection;

        static CollectionsList collectionsList;

        public GameObject selectedPrefab;
        Vector2 paletteScrollPosition;
        Vector2 windowScrollPosition;
        float dynamicPrefabIconSize;

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

            Settings.showHeader = EditorGUILayout.Toggle("Show Settings", Settings.showHeader);

            if (Settings.showHeader)
            {
                if (GUILayout.Button("Manage Collections"))
                {
                    if (collectionsList == null)
                    {
                        collectionsList = LoadOrCreateAsset<CollectionsList>(PathDr.GetGeneratedFolderPath, "CollectionNamesList.asset", out string assetPath);

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
                         .Any(c => c != CollectionName.None))
                {

                    EditorGUILayout.HelpBox("Use the above button to edit the collections list", MessageType.Warning);
                    return;
                }

                // Create a foldout for "Palette Settings"
                Settings.showPaletteSettings = EditorGUILayout.Foldout(Settings.showPaletteSettings, "Palette Settings");

                // If the foldout is expanded, display the settings
                if (Settings.showPaletteSettings)
                {
                    Settings.gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Palette Columns", Settings.gridColumns));
                    Settings.minPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Palette Scale", Settings.minPaletteScale), 50f, Settings.maxPaletteScale);
                    Settings.maxPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Palette Scale", Settings.maxPaletteScale), Settings.minPaletteScale, 500f);
                }

                GUILayout.Space(2);

                Settings.showPlacementSettings = EditorGUILayout.Foldout(Settings.showPlacementSettings, "Placement Settings");

                if (Settings.showPlacementSettings)
                {
                    Settings.placerColor = EditorGUILayout.ColorField("Placer Color", Settings.placerColor);
                    Settings.placerRadius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Placer Visual Radius", Settings.placerRadius));
                }
            }

            GUI.enabled = Settings.isNameDropdownActive;

            // Select collection to show.
            Settings.collectionNameDropdown = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", Settings.collectionNameDropdown);
            currentPrefabCollection = GetPrefabCollection(Settings.collectionNameDropdown);

            // Force the name dropdown to None to avoid regenerating assets accidentally if the list inspector is open
            if (HasOpenInstances<CollectionsListInspector>())
            {
                Settings.collectionNameDropdown = CollectionName.None;
                Settings.isNameDropdownActive = false;
                EditorGUILayout.HelpBox("Close the Collections Inspector window to choose a collection", MessageType.Warning);
                return;
            }
            else
            {
                Settings.isNameDropdownActive = true;
            }

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
            GUILayout.Label($"Palette - {currentPrefabCollection.Name}", EditorStyles.boldLabel);

            var prefabList = currentPrefabCollection.prefabList;

            if (GUILayout.Button("Edit Prefab Collection"))
            {
                // Inspect the currentPrefabCollection scriptable object
                PrefabCollectionInspector.OpenEditWindow(currentPrefabCollection);
            }

            if (selectedPrefab != null)
            {
                if (GUILayout.Button("Stop Placing Prefabs", GUILayout.Height(50)))
                {
                    selectedPrefab = null;
                }
            }

            // Placement mode toolbar
            PlacementModeManager.ToolbarGUI(this);
            GUILayout.Space(5);
            GUILayout.Label($"{PlacementModeManager.CurrentType} Mode Settings", EditorStyles.largeLabel);
            GUILayout.Space(5);
            PlacementModeManager.CurrentMode.SettingsGUI(this);
            GUILayout.Space(20);

            float windowWidth = EditorGUIUtility.currentViewWidth - 10; // Get editor window width (minus padding)

            dynamicPrefabIconSize = Mathf.Clamp(Mathf.Max(windowWidth / Settings.gridColumns - 10, 40), Settings.minPaletteScale, Settings.maxPaletteScale);

            // Start Scroll View
            paletteScrollPosition = GUILayout.BeginScrollView(paletteScrollPosition); // Set max visible height

            int rowCount = Mathf.CeilToInt((float)prefabList.Count / Settings.gridColumns);

            // Calculate the total width of the grid (based on the number of columns and button size)
            float gridWidth = Settings.gridColumns * dynamicPrefabIconSize;

            // Calculate the left padding required to center the grid
            float gridPadding = Mathf.Max((windowWidth - gridWidth) * 0.2f, 0);

            for (int row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(gridPadding);

                for (int col = 0; col < Settings.gridColumns; col++)
                {
                    int index = row * Settings.gridColumns + col;
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
        /// Returns prefab collection object by name, creates it if it doesn't exist
        /// </summary>
        /// <remarks>
        /// Note: CollectionName.None returns null.
        /// </remarks>
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

        private static T LoadOrCreateAsset<T>(string folderPath, string assetName, out string assetPath) where T : ScriptableObject
        {
            // Find existing asset
            T asset = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .FirstOrDefault();

            if (asset != null)
            {
                assetPath = AssetDatabase.GetAssetPath(asset);
                return asset;
            }

            // Create new asset
            asset = ScriptableObject.CreateInstance<T>();
            assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{assetName}");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
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
            PlacementModeManager.CurrentMode.OnActive(this);
        }

        private void OnEnable()
        {
            if (Settings == null)
                Settings = LoadOrCreateAsset<ToolSettings>(PathDr.GetGeneratedFolderPath, "ToolSettings.asset", out string assetPath);

            SceneView.duringSceneGui += OnSceneGUI;
            VisualPlacer.OnEnable();
            SceneInteraction.OnEnable();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            VisualPlacer.OnDisable();
            SceneInteraction.OnDisable();
        }
    }
}
