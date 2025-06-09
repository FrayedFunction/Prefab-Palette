using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class CollectionsManagerWindow : EditorWindow
    {
        static ToolContext tool;
        float buttonSpace = 5;

        [MenuItem("Window/Prefab Palette/Collections Manager")]
        public static void OpenMainWindow()
        {
            GetWindow<CollectionsManagerWindow>("Prefab Palete: Collections Manager");
        }

        private void OnEnable()
        {
            tool = ToolContext.Instance;
            minSize = new Vector2(600, 500);
            maxSize = new Vector2(601, 501);
        }

        private void OnGUI()
        {
            Helpers.TitleText("Prefab Palette: Collections Manager");

            // Force the name dropdown to None to avoid regenerating assets accidentally if the list inspector is open
            if (HasOpenInstances<CollectionsListInspector>())
            {
                tool.Settings.currentCollectionName = CollectionName.None;
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

            tool.Settings.currentCollectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", tool.Settings.currentCollectionName);

            // if the enum only contains .None
            if (!Enum.GetValues(typeof(CollectionName))
                     .Cast<CollectionName>()
                     .Any(c => c != CollectionName.None))
            {
                EditorGUILayout.HelpBox("You don't have any collections yet,\nAdd one by using the Manage Collections button", MessageType.Warning);
                return;
            }

            if (tool.Settings.currentCollectionName == CollectionName.None)
            {
                EditorGUILayout.HelpBox("Choose a Collection to get Started", MessageType.Warning);
                return;
            }

            GUILayout.Space(buttonSpace);
            if (GUILayout.Button("Edit Prefab Collection", GUILayout.Height(25)))
            {
                // Inspect the currentPrefabCollection scriptable object
                PrefabCollectionInspector.OpenWindow(tool.Settings.CurrentPrefabCollection);
            }

            GUILayout.Space(buttonSpace);

            if (GUILayout.Button("Open Palette", GUILayout.Height(25)))
            {
                PaletteWindow.OnShowToolWindow(tool);
            }

            GUILayout.Space(buttonSpace);
            Helpers.DrawLine(Color.grey);
        }
    }
}
