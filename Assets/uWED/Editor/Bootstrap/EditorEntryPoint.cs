using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using uWED.Editor.Platform;
using uWED.Runtime.Bootstrap;

namespace uWED.Editor.Bootstrap
{
    // EditorWindow shell. Holds only what genuinely requires UnityEditor;
    // all UI construction/wiring lives in UwedBootstrap (Runtime, engine-only).
    public class EditorEntryPoint : EditorWindow
    {
        private const string c_defaultAsset = "assets/DefaultMapAsset.asset";

        private UwedBootstrap m_bootstrap;

        [MenuItem("Window/uWED/Map Editor")]
        public static void OpenWindow()
        {
            EditorEntryPoint wnd = GetWindow<EditorEntryPoint>();
            wnd.titleContent = new GUIContent("uWED");
        }

        // Editor-only: relies on the Unity asset database (MapAssetLoader), no standalone equivalent.
        public static void OpenMap(string assetName)
        {
            UwedBootstrap.OpenMap(new MapAssetLoader(), assetName);
        }

        public void CreateGUI()
        {
            m_bootstrap = new UwedBootstrap(rootVisualElement, Close);
            OpenMap(c_defaultAsset);
            AssemblyReloadEvents.beforeAssemblyReload += m_bootstrap.SavePrefs;
        }

        public void OnEnable()
        {
            this.SetAntiAliasing(4);
        }

        public void OnDestroy()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= m_bootstrap.SavePrefs;
            m_bootstrap?.SavePrefs();
            m_bootstrap?.Dispose();
        }
    }
}
