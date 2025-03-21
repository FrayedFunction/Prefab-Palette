using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Scriptable Object that holds a list of prefabs and its collection name.
    /// </summary>
    public class PrefabCollection : ScriptableObject
    {
        private string nameAsString;

        /// <summary>
        /// User set name of collection.
        /// </summary>
        /// <returns>
        /// CollectionName.None if its name doesn't exist in the enum
        /// </returns>
        public CollectionName Name
        {
            get => Enum.TryParse(nameAsString, out CollectionName result) ? result : CollectionName.None;
            set => nameAsString = value.ToString();
        }

        /// <summary>
        /// List of prefabs in this collection.
        /// </summary>
        public List<GameObject> prefabList = new List<GameObject>();
    }
}
