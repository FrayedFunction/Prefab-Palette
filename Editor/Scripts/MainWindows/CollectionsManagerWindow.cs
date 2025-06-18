using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Editor window to manage prefab collections within the Prefab Palette system.
    /// Provides UI to select collections, open related inspectors, and navigate to the palette window.
    /// </summary>
    public class CollectionsManagerWindow : EditorWindow
    {
        ToolSettings Settings => ToolContext.Instance.Settings;
        float buttonSpace = 5;

        /// <summary>
        /// Opens the Collections Manager window via the Window dropdown menu.
        /// </summary>
        [MenuItem("Window/Prefab Palette/Collections Manager")]
        public static void OpenMainWindow()
        {
            GetWindow<CollectionsManagerWindow>("Prefab Palete: Collections Manager");
        }

        private void OnEnable()
        {
            minSize = new Vector2(600, 500);
            maxSize = new Vector2(601, 501);
        }

        private void OnGUI()
        {
            Helpers.DrawLogo();
            Helpers.TitleText("Collections Manager");

            // Force the name dropdown to None to avoid regenerating assets accidentally if the list inspector is open
            if (HasOpenInstances<CollectionsListInspector>())
            {
                Settings.currentCollectionName = CollectionName.None;
                EditorGUILayout.HelpBox("Collections Inspector window is open, close it when you're finished editing", MessageType.Warning);
                return;
            }
     
            Helpers.DrawLine(Color.grey);
            GUILayout.Space(10);

            if (GUILayout.Button("Manage Collections", GUILayout.Height(50)))
            {
                CollectionsListInspector.OpenWindow();

                if (HasOpenInstances<PaletteWindow>())
                {
                    GetWindow<PaletteWindow>().Close();
                }
            }
            
            GUILayout.Space(10);
            Helpers.DrawLine(Color.gray);

            Settings.currentCollectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", Settings.currentCollectionName);

            // if the enum only contains .None
            if (!Enum.GetValues(typeof(CollectionName))
                     .Cast<CollectionName>()
                     .Any(c => c != CollectionName.None))
            {
                EditorGUILayout.HelpBox("You don't have any collections yet,\nAdd one by using the Manage Collections button", MessageType.Warning);
                return;
            }

            if (Settings.currentCollectionName == CollectionName.None)
            {
                EditorGUILayout.HelpBox("Choose a Collection to get Started", MessageType.Warning);
                return;
            }

            GUILayout.Space(buttonSpace);
            if (GUILayout.Button("Edit Prefab Collection", GUILayout.Height(25)))
            {
                // Inspect the currentPrefabCollection scriptable object
                PrefabCollectionInspector.OpenWindow(Settings.CurrentPrefabCollection);
            }

            GUILayout.Space(buttonSpace);

            if (GUILayout.Button("Open Palette", GUILayout.Height(25)))
            {
                PaletteWindow.OnShowToolWindow();
            }

            GUILayout.Space(buttonSpace);
            Helpers.DrawLine(Color.grey);
        }
    }
}
