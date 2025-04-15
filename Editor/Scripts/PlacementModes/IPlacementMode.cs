using UnityEngine;

namespace PrefabPalette
{
    public interface IPlacementMode
    {
        public void SettingsGUI(PrefabPaletteTool tool);
        public void OnEnter(PrefabPaletteTool tool);
        public void OnActive(PrefabPaletteTool tool);
        public void OnExit(PrefabPaletteTool tool);
    }
}
