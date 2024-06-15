using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UWed : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_uxml = default;
    [SerializeField]
    private StyleSheet m_StyleSheet = default;

    private EditorView m_editorView;

    private static UWed s_instance = null;

    [MenuItem("Window/UI Toolkit/uWED")]
    public static void OpenWindow()
    {
        UWed wnd = GetWindow<UWed>();
        wnd.titleContent = new GUIContent("uWED");
        s_instance = wnd;
    }

    public static void OpenMap(string assetName)
    {
        s_instance?.m_editorView.Interface.OnLoadMap(new MapAssetLoader(), assetName);
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        VisualElement ui = m_uxml.Instantiate();
        rootVisualElement.Add(ui);
        ui.StretchToParentSize();
        IEnumerable<VisualElement> containers = ui.Children();
        VisualElement menu = containers.Where(x => x.name == "menu").FirstOrDefault();
        VisualElement editor = containers.Where(x => x.name == "editor").FirstOrDefault();
        VisualElement inspector = containers.Where(x => x.name == "inspector").FirstOrDefault();

        m_editorView = new EditorView();
        m_editorView.styleSheets.Add(m_StyleSheet);
        editor.Add(m_editorView);

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("A wild uWED appears.");
        inspector.Add(label);

        MenuBinder binder = new MenuBinder(m_editorView, menu, this);
        AssemblyReloadEvents.beforeAssemblyReload += m_editorView.SavePrefs;
    }

    public void OnDestroy()
    {
        m_editorView?.SavePrefs();
        s_instance = null;
    }
}
