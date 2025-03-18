using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
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
            GUILayout.Label($"{targetObject.Type}", EditorStyles.whiteLargeLabel);
            if (editorInstance != null)
            {
                editorInstance.OnInspectorGUI();
            }
        }
    }
}
