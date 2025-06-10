using System;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class CreateCollectionWindow : EditorWindow
    {
        private string collectionName = "NewPrefabCollection";
        private Action<string> onCollectionNameConfirmed;

        private const float WindowWidth = 300f;
        private const float WindowHeight = 90f;

        public static void Show(Action<string> onCollectionNameConfirmed)
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
            window.onCollectionNameConfirmed = onCollectionNameConfirmed;
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
                    // sanitise the name for enum generation
                    var sanitisedName = Helpers.SanitiseEnumName(collectionName);

                    // Check if a collection already exists with this name
                    if (Enum.TryParse<CollectionName>(sanitisedName, out _))
                    {
                        EditorUtility.DisplayDialog("Collection Already Exisits!", "Please enter a unique name.", "OK");
                        return;
                    }

                    onCollectionNameConfirmed?.Invoke(sanitisedName);
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
