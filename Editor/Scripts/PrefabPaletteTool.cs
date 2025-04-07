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

        public GameObject selectedPrefab;
        Vector2 paletteScrollPosition;
        Vector2 windowScrollPosition;
        float dynamicPrefabIconSize;
        bool canInteractWithCollectionDropdown = true;

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
            
            // Force the name dropdown to None to avoid regenerating assets accidentally if the list inspector is open
            if (HasOpenInstances<CollectionsListInspector>())
            {
                Settings.collectionName = CollectionName.None;
                canInteractWithCollectionDropdown = false;
                EditorGUILayout.HelpBox("Collections Inspector window is open, close it when you're finished editing", MessageType.Warning);
                return;
            }
            else
            {
                canInteractWithCollectionDropdown = true;
            }


            if (GUILayout.Button("Manage Collections"))
            {
                CollectionsListInspector.OpenWindow(this);
            }

            EditorGUILayout.Space(5);

            // if the enum only contains .None
            if (!Enum.GetValues(typeof(CollectionName))
                     .Cast<CollectionName>()
                     .Any(c => c != CollectionName.None))
            {
                EditorGUILayout.HelpBox("You don't have any collections yet,\nAdd one by using the Manage Collections button", MessageType.Warning);
                return;
            }

            // Header
            Settings.showHeader = EditorGUILayout.Toggle("Show Settings", Settings.showHeader);

            if (Settings.showHeader)
            {
                EditorGUI.indentLevel++;

                // Palette Settings
                Settings.showPaletteSettings = EditorGUILayout.Foldout(Settings.showPaletteSettings, "Palette Settings");
                if (Settings.showPaletteSettings)
                {
                    EditorGUI.indentLevel++;
                    Settings.gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Palette Columns", Settings.gridColumns));
                    Settings.minPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Min Palette Scale", Settings.minPaletteScale), 50f, Settings.maxPaletteScale);
                    Settings.maxPaletteScale = Mathf.Clamp(EditorGUILayout.FloatField("Max Palette Scale", Settings.maxPaletteScale), Settings.minPaletteScale, 500f);
                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(2);

                // Placer Setttings
                Settings.showPlacerSettings = EditorGUILayout.Foldout(Settings.showPlacerSettings, "Placer Settings");
                if (Settings.showPlacerSettings)
                {
                    EditorGUI.indentLevel++;
                    Settings.placerColor = EditorGUILayout.ColorField("Placer Color", Settings.placerColor);
                    Settings.placerRadius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Placer Visual Radius", Settings.placerRadius));
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                GUILayout.Space(5);
            }

            GUILayout.Space(7.5f);
            DrawLine(Color.grey);
            GUILayout.Space(7.5f);

            GUI.enabled = canInteractWithCollectionDropdown;
            // Select collection
            Settings.collectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", Settings.collectionName);
            currentPrefabCollection = GetPrefabCollection(Settings.collectionName);

            if (Settings.collectionName == CollectionName.None)
                return;

            if (GUILayout.Button("Edit Prefab Collection"))
            {
                // Inspect the currentPrefabCollection scriptable object
                PrefabCollectionInspector.OpenEditWindow(currentPrefabCollection);
            }
            EditorGUILayout.Space(5);

            GUILayout.Label($"Palette - {currentPrefabCollection.Name}", EditorStyles.boldLabel);

            GUILayout.Space(5);

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
            var prefabList = currentPrefabCollection.prefabList;

            if (selectedPrefab != null)
            {
                if (GUILayout.Button("Stop Placing Prefabs", GUILayout.Height(50)))
                {
                    selectedPrefab = null;
                }
            }

            GUILayout.Label($"{PlacementModeManager.CurrentType} Mode", EditorStyles.largeLabel);
            GUILayout.Space(2.5f);

            // Placement mode toolbar
            PlacementModeManager.ToolbarGUI(this);
            GUILayout.Space(5);

            PlacementModeManager.CurrentMode.SettingsGUI(this);

            GUILayout.Space(5);

            float windowWidth = EditorGUIUtility.currentViewWidth - 10; // Get editor window width (minus padding)

            dynamicPrefabIconSize = Mathf.Clamp(Mathf.Max(windowWidth / Settings.gridColumns - 10, 40), Settings.minPaletteScale, Settings.maxPaletteScale);

            GUILayout.BeginVertical("box");

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

            GUILayout.EndVertical();
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

            return PrefabCollection.CreateNewCollection(name);
        }

        public static T LoadOrCreateAsset<T>(string folderPath, string assetName, out string assetPath) where T : ScriptableObject
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

        private static void DrawLine(Color color, int thickness = 1, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}
