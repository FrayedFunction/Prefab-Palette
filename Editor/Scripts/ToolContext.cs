using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static UnityEngine.GridBrushBase;

namespace PrefabPalette
{
    public class ToolContext
    {
        private static ToolContext instance;

        public static ToolContext Instance => instance ??= new();

        public ToolSettings Settings { get; private set; }

        public GameObject SelectedPrefab { get; set; }

        ToolContext()
        {
            Settings = Helpers.LoadOrCreateAsset<ToolSettings>(PathDr.GetGeneratedFolderPath, "ToolSettings.asset", out _);
        }
    }
}
