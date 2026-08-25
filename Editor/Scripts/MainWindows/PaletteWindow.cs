using UnityEditor;
using UnityEngine;

namespace PrefabPalette
{
    public class PaletteWindow : EditorWindow
    {
        PaletteGUI paletteGUI;

        ToolSettings Settings => ToolContext.Instance.Settings;

        [MenuItem("Window/Prefab Palette/Palette")]
        public static void Open()
        {
            GetWindow<PaletteWindow>("Prefab Palette");
            PrefabCollectionList.Instance.Sync();          
        }

        void OnEnable()
        {
            ToolContext.Instance.OnEnable();

            paletteGUI = new PaletteGUI();

            Settings.paletteWindowScale.Resolve(
                Settings.globalMinWindowScale,
                Settings.globalMaxWindowScale,
                out Vector2 min,
                out Vector2 max
            );

            minSize = min;
            maxSize = max;
        }

        private void OnDisable()
        {
            paletteGUI?.Dispose();
            paletteGUI = null;

            var context = ToolContext.Instance;
            context.OnDisable();
        }

        private void OnGUI()
        {
            paletteGUI.Draw(EditorGUIUtility.currentViewWidth - 10);
        }
    }
}
