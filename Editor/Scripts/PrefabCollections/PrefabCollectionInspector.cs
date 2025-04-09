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
        private PrefabCollection targetObject;
        private SerializedObject serializedObject;
        private SerializedProperty listProperty;

        public static void OpenEditWindow(PrefabCollection obj)
        {
            PrefabCollectionInspector window = GetWindow<PrefabCollectionInspector>("Collection Editor");
            window.targetObject = obj;
            window.serializedObject = new SerializedObject(obj);
            window.listProperty = window.serializedObject.FindProperty("prefabList");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label($"{targetObject.Name}", EditorStyles.whiteLargeLabel);

            if (serializedObject != null)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(listProperty, true); // Only show the list
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
