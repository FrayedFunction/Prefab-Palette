using UnityEngine;

namespace PrefabPalette
{
    public abstract class PlacementMode
    {
        public abstract void SettingsGUI();
        public abstract void OnEnter();
        public abstract void OnActive(PrefabPaletteTool tool);
        public abstract void OnExit();
    }
}
