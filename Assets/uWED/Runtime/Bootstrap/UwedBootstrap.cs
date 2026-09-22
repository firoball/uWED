using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Runtime.Core.Map.IO;
using uWED.Runtime.Platform;
using uWED.Runtime.UI.Binder;
using uWED.Runtime.UI.EventBus;
using uWED.Runtime.UI.Help;
using uWED.Runtime.UI.Inspector;
using uWED.Runtime.UI.Manipulator;
using uWED.Runtime.UI.View2D;

namespace uWED.Runtime.Bootstrap
{
    // Builds the uWED UI tree and wires panels/binders together. No UnityEditor dependency;
    // called by EditorEntryPoint today, by a future StandaloneEntryPoint later.
    public class UwedBootstrap
    {
        public EditorView EditorView { get; }
        public InfoPanel InfoPanel { get; }
        public MeshPreviewPanel MeshPreviewPanel { get; }
        public EditorHelp EditorHelp { get; }
        public VisualElement Root { get; }

        private readonly IPrefsProvider m_prefsProvider;

        public UwedBootstrap(VisualElement rootVisualElement, Action closeAction)
        {
            m_prefsProvider = ServiceLocator.Get<IPrefsProvider>();

            EditorEventBus.Clear();
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("uWED");
            StyleSheet styleSheet = Resources.Load<StyleSheet>("uWED");
            // TODO(platform-abstraction): m_manipulatorUxml still passed through explicitly;
            // ManipulatorBinder's own asset handling is being reworked separately.
            VisualTreeAsset m_manipulatorUxml = Resources.Load<VisualTreeAsset>("ManipulatorBase");

            EditorEventBus.Clear();
            Root = uxml.Instantiate();
            Root.name = "editorContainer";
            rootVisualElement.Add(Root);
            Root.StretchToParentSize();
            Root.focusable = true;
            Root.tabIndex = 0;

            IEnumerable<VisualElement> containers = Root.Children();
            VisualElement menu = containers.FirstOrDefault(x => x.name == "menu");
            VisualElement editor = containers.FirstOrDefault(x => x.name == "editor");
            VisualElement inspector = containers.FirstOrDefault(x => x.name == "inspector");

            VisualElement dialogContainer = new VisualElement { name = "dialogContainer", pickingMode = PickingMode.Ignore };
            rootVisualElement.Add(dialogContainer);
            dialogContainer.StretchToParentSize();

            IManipulatorSettings settings = new ManipulatorSettings();

            // Elements interacting with EditorView events must exist before EditorView for event registration.
            InfoPanel = new InfoPanel();
            MeshPreviewPanel = new MeshPreviewPanel();
            EditorHelp = new EditorHelp();
            StatisticsPanel statisticsPanel = new StatisticsPanel();

            EditorView = new EditorView();
            EditorView.styleSheets.Add(styleSheet);

            editor?.Add(EditorView);
            editor?.Add(InfoPanel);
            editor?.Add(MeshPreviewPanel);
            dialogContainer.Add(EditorHelp);
            inspector?.Add(statisticsPanel);

            MenuBinder menuBinder = new MenuBinder(EditorView, EditorHelp, menu, closeAction);
            InspectorBinder inspectorBinder = new InspectorBinder(Root, InfoPanel, MeshPreviewPanel, statisticsPanel);
            ManipulatorBinder manipulatorBinder = new ManipulatorBinder(m_manipulatorUxml, dialogContainer, settings);
            KeyBinder keyBinder = new KeyBinder(Root);

            LoadPrefs();
            Root.Focus();
        }

        public static void OpenMap(IMapLoader loader, string assetName)
        {
            EditorEventBus.Instance.LoadMap.Raise(loader, assetName);
            EditorEventBus.Instance.FitViewToWindow.Raise();
        }

        public void LoadPrefs() => EditorEventBus.Instance.LoadPrefs.Raise(m_prefsProvider);
        public void SavePrefs() => EditorEventBus.Instance.SavePrefs.Raise(m_prefsProvider);

        // Required for properly freeing MeshPreviewPanel scene.
        public void Dispose() => MeshPreviewPanel?.Dispose();
    }
}
