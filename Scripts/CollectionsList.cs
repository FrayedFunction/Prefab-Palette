using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PrefabPalette 
{
    public class CollectionsList : ScriptableObject
    {
        public List<string> collectionNames = new List<string>();

        public void GenerateEnum()
        {
            string filePath = PathDr.GetToolRootPath + "/Scripts/CollectionNames.cs";
    
            List<string> validNames = new List<string>();

            // Collect valid names (non-empty strings), also going to want to check for spaces and remove them
            foreach (var name in collectionNames)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    validNames.Add(name);
                }
            }

            string content = "namespace PrefabPalette {\n";
            content += "    public enum CollectionName {\n        None,\n";
            foreach (var name in collectionNames)
            {
                content += $"        {name},\n";
            }
            content += "    }\n}";

            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh(); // Trigger recompile
        }
    }
}
