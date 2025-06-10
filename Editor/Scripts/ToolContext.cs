using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Singleton context class that holds shared tool data and settings
    /// used across the Prefab Palette tool.
    /// </summary>
    public class ToolContext
    {
        private static ToolContext instance;

        /// <summary>
        /// Singleton instance of the ToolContext.
        /// Initialized on first access.
        /// </summary>
        public static ToolContext Instance => instance ??= new();

        public ToolSettings Settings { get; private set; }

        /// <summary>
        /// Currently selected prefab in the palette.
        /// </summary>
        public GameObject SelectedPrefab { get; set; }

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// Loads or creates the ToolSettings asset on instantiation.
        /// </summary>
        ToolContext()
        {
            Settings = Helpers.LoadOrCreateAsset<ToolSettings>(PathDr.GetGeneratedFolderPath, "ToolSettings.asset", out _);
        }
    }
}
