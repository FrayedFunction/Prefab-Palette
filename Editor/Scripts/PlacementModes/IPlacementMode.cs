using UnityEngine;

namespace PrefabPalette
{
    public interface IPlacementMode
    {
        public void SettingsGUI(ToolContext tool);
        public void OnEnter(ToolContext tool);
        public void OnActive(ToolContext tool);
        public void OnExit(ToolContext tool);
    }
}
