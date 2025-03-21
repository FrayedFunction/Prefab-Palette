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
        private Editor editorInstance;

        public static void OpenEditWindow(PrefabCollection obj)
        {
            PrefabCollectionInspector window = GetWindow<PrefabCollectionInspector>("Collection Editor");
            window.targetObject = obj;
            window.editorInstance = Editor.CreateEditor(obj);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label($"{targetObject.Name}", EditorStyles.whiteLargeLabel);
            if (editorInstance != null)
            {
                editorInstance.OnInspectorGUI();
            }
        }
    }
}
