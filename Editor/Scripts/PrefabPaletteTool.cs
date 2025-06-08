using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static UnityEngine.GridBrushBase;

namespace PrefabPalette
{
    public class PrefabPaletteTool
    {
        private static PrefabPaletteTool instance;

        public static PrefabPaletteTool Instance => instance ??= new();

        public ToolSettings Settings { get; private set; }

        public GameObject SelectedPrefab { get; set; }

        PrefabPaletteTool()
        {
            Settings = Helpers.LoadOrCreateAsset<ToolSettings>(PathDr.GetGeneratedFolderPath, "ToolSettings.asset", out _);
        }
    }
}
