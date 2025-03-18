using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPalette
{
    public class PrefabCollection : ScriptableObject
    {
        private string collectionNameString;

        public CollectionName Type
        {
            get => Enum.TryParse(collectionNameString, out CollectionName result) ? result : CollectionName.None;
            set => collectionNameString = value.ToString();
        }

        public List<GameObject> prefabList = new List<GameObject>();
    }
}
