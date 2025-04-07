using System;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class CreateCollectionWindow : EditorWindow
    {
        private string collectionName = "NewPrefabCollection";
        private Action<string> onCreate;

        private const float WindowWidth = 300f;
        private const float WindowHeight = 90f;

        public static void Show(Action<string> onCreateCallback)
        {
            var window = CreateInstance<CreateCollectionWindow>();
            window.titleContent = new GUIContent("Name Your Collection");
            window.position = new Rect(
                (Screen.width - WindowWidth) / 2f,
                (Screen.height - WindowHeight) / 2f,
                WindowWidth, WindowHeight
            );

            window.minSize = new Vector2(WindowWidth, WindowHeight);
            window.maxSize = new Vector2(WindowWidth, WindowHeight);
            window.onCreate = onCreateCallback;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter Collection Name:", EditorStyles.boldLabel);
            GUI.SetNextControlName("CollectionNameField");
            collectionName = EditorGUILayout.TextField(collectionName);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            if (GUILayout.Button("Create"))
            {
                if (!string.IsNullOrEmpty(collectionName.Trim()))
                {
                    onCreate?.Invoke(collectionName.Trim());
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Name", "Please enter a valid name.", "OK");
                }
            }
            GUILayout.EndHorizontal();

            EditorGUI.FocusTextInControl("CollectionNameField");
        }
    }

}
