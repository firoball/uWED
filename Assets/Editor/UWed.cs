using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UWed : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_uxml = default;
    [SerializeField]
    private StyleSheet m_StyleSheet = default;

    [MenuItem("Window/UI Toolkit/uWED")]
    public static void OpenWindow()
    {
        UWed wnd = GetWindow<UWed>();
        wnd.titleContent = new GUIContent("uWED");
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

        VisualElement editorView = new EditorView();
        editorView.styleSheets.Add(m_StyleSheet);
        editor.Add(editorView);

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("A wild uWED appears.");
                inspector.Add(label);

        }
}
