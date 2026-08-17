using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Ui.Help;
using Editor.UI.Inspector;
using Editor.UI.Manipulator;
using Editor.UI.View2D;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class UWed : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_uxml = default;
    [SerializeField]
    private StyleSheet m_StyleSheet = default;

    [SerializeField]
    private VisualTreeAsset m_helpUxml = default;
    [SerializeField]
    private VisualTreeAsset m_manipulatorUxml = default;

    private EditorView m_editorView;
    private InfoPanel m_infoPanel;
    private MeshPreviewPanel m_meshPreviewPanel;
    private EditorHelp m_editorHelp;

    private static UWed s_instance = null;

    [MenuItem("Window/uWED/Map Editor")]
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

        IManipulatorSettings settings = new ManipulatorSettings();
        VertexManipulator vertexManipulator = new VertexManipulator(m_manipulatorUxml, settings);
        SegmentManipulator segmentManipulator = new SegmentManipulator(m_manipulatorUxml, settings);


        // all Elements interacting with EditorView events must be created earlier for event registration
        m_infoPanel = new InfoPanel();
        m_meshPreviewPanel = new MeshPreviewPanel();
        m_editorHelp = new EditorHelp(ui, m_helpUxml);
        StatisticsPanel statisticsPanel = new StatisticsPanel();

        // now create the EditorView
        m_editorView = new EditorView();
        m_editorView.styleSheets.Add(m_StyleSheet);

        // add UI elements in correct order
        editor?.Add(m_editorView);
        editor?.Add(m_infoPanel);
        editor?.Add(m_meshPreviewPanel);
        ui.Add(m_editorHelp);
        ui.Add(vertexManipulator);
        ui.Add(segmentManipulator);
        inspector?.Add(statisticsPanel);

        // glue things together
        MenuBinder menuBinder = new MenuBinder(m_editorView, m_editorHelp, menu, this); 
        InspectorBinder inspectorBinder = new InspectorBinder(m_editorView, m_infoPanel, m_meshPreviewPanel, statisticsPanel);
        ManipulatorBinder manipulatorBinder = new ManipulatorBinder(m_editorView, vertexManipulator, segmentManipulator);
        
        AssemblyReloadEvents.beforeAssemblyReload += m_editorView.SavePrefs;
    }

    public void OnEnable()
    {
        this.SetAntiAliasing(4);
    }

    public void OnDestroy()
    {
        m_editorView?.SavePrefs();
        m_meshPreviewPanel?.Dispose(); // required for properly freeing PreviewRenderUtility 
        s_instance = null;
    }
}
