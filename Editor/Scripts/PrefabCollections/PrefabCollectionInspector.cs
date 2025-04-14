using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Inspector window for editing prefab collections.
    /// </summary>
    public class PrefabCollectionInspector : EditorWindow
    {
        private PrefabCollection targetCollection;
        private SerializedObject serializedObject;
        private SerializedProperty listProperty;
        private Vector2 scrollPos;

        public static void OpenWindow(PrefabCollection obj)
        {
            PrefabCollectionInspector window = GetWindow<PrefabCollectionInspector>("Collection Editor");
            window.targetCollection = obj;
            window.serializedObject = new SerializedObject(obj);
            window.listProperty = window.serializedObject.FindProperty("prefabList");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label($"{targetCollection.Name}", EditorStyles.whiteLargeLabel);

            if (serializedObject != null)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                serializedObject.Update();
                EditorGUILayout.PropertyField(listProperty, true); // Only show the list
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
