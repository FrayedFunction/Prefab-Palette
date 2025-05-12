using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class CollectionsWindow : EditorWindow
    {
        static PrefabPaletteTool tool;
        float buttonSpace = 4;

        [MenuItem("Window/Prefab Palette/Collections")]
        public static void OpenMainWindow()
        {
            GetWindow<CollectionsWindow>("Prefab Palete: Collections");
        }

        [MenuItem("Window/Prefab Palette/Palette")]
        public static void OpenPalette()
        {
            tool = PrefabPaletteTool.Instance;
            PaletteWindow.OnShowToolWindow(tool);
        }

        private void OnEnable()
        {
            tool = PrefabPaletteTool.Instance;
            minSize = new Vector2(300, 150);
            maxSize = new Vector2(350, 200);

        }

        private void OnGUI()
        {
            Helpers.TitleText("Prefab Palette: Collections");

            // Force the name dropdown to None to avoid regenerating assets accidentally if the list inspector is open
            if (HasOpenInstances<CollectionsListInspector>())
            {
                tool.Settings.collectionName = CollectionName.None;
                EditorGUILayout.HelpBox("Collections Inspector window is open, close it when you're finished editing", MessageType.Warning);
                return;
            }

            GUILayout.Space(buttonSpace);

            Helpers.DrawLine(Color.grey);
            
            if (GUILayout.Button("Manage Collections"))
            {
                CollectionsListInspector.OpenWindow(tool);

                if (HasOpenInstances<PaletteWindow>())
                {
                    GetWindow<PaletteWindow>().Close();
                }
            }

            tool.Settings.collectionName = (CollectionName)EditorGUILayout.EnumPopup("Prefab Collection", tool.Settings.collectionName);
            tool.CurrentPrefabCollection = tool.GetPrefabCollection(tool.Settings.collectionName);

            // if the enum only contains .None
            if (!Enum.GetValues(typeof(CollectionName))
                     .Cast<CollectionName>()
                     .Any(c => c != CollectionName.None))
            {
                EditorGUILayout.HelpBox("You don't have any collections yet,\nAdd one by using the Manage Collections button", MessageType.Warning);
                return;
            }

            if (tool.Settings.collectionName == CollectionName.None)
            {
                EditorGUILayout.HelpBox("Choose a Collection to get Started", MessageType.Warning);
                return;
            }

            GUILayout.Space(buttonSpace);
            if (GUILayout.Button("Edit Prefab Collection"))
            {
                // Inspect the currentPrefabCollection scriptable object
                PrefabCollectionInspector.OpenWindow(tool.CurrentPrefabCollection);
            }

            GUILayout.Space(buttonSpace);

            if (GUILayout.Button("Open Palette"))
            {
                PaletteWindow.OnShowToolWindow(tool);
            }

            GUILayout.Space(buttonSpace);
            Helpers.DrawLine(Color.grey);
        }
    }
}
