using UnityEngine;

namespace PrefabPalette
{
    public interface IPlacementMode
    {
        public abstract void SettingsGUI(PrefabPaletteTool tool);
        public abstract void OnEnter(PrefabPaletteTool tool);
        public abstract void OnActive(PrefabPaletteTool tool);
        public abstract void OnExit(PrefabPaletteTool tool);
    }
}
