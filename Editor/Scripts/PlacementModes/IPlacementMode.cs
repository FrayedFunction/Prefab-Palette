using UnityEngine;

namespace PrefabPalette
{
    /// <summary>
    /// Defines the behavior for different placement modes.
    /// </summary>
    public interface IPlacementMode
    {
        /// <summary>
        /// Tells you what buttons to press from the context menu
        /// </summary>
        string[] ControlsHelpBox { get; }

        /// <summary>
        /// Draws the settings user interface for this placement mode in the Editor Overlay.
        /// </summary>
        /// <param name="tool">The context of the current tool.</param>
        void SettingsOverlayGUI(ToolContext tool);

        /// <summary>
        /// Called when the placement mode is selected.
        /// </summary>
        /// <param name="tool">The context of the current tool.</param>
        void OnEnter(ToolContext tool);

        /// <summary>
        /// Called when the mode is actively being used.
        /// </summary>
        /// <param name="tool">The context of the current tool.</param>
        void OnActive(ToolContext tool);

        /// <summary>
        /// Called when the mode is exited.
        /// </summary>
        /// <param name="tool">The context of the current tool.</param>
        void OnExit(ToolContext tool);
    }

}
