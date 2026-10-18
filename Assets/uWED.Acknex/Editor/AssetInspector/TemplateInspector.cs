using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.UI.Manipulator;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Editor.Bootstrap;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>
    /// Base of the Unity Inspector for a Template asset: shows the property groups of the Template type's
    /// layout one under the other, using the same TemplatePropertyPanel as the Manipulator tabs. Edits go
    /// straight to the asset through a direct TemplateEditSession (no Edit/Apply protection), each one
    /// recorded for Undo and marking the asset dirty. A subclass names the layout of its Template type and,
    /// to offer the map highlight buttons, the kind of map data using it (see HighlightedDataType).
    /// </summary>
    public abstract class TemplateInspector : UnityEditor.Editor
    {
        const float FloatStep = 0.1f;
        const string BaseStyleSheetName = "ManipulatorBase";
        const string SectionTitleClass = "manip-section-title";
        const string HighlightText = "Highlight in Map";
        const string HighlightActiveText = "Highlighted";

        readonly List<TemplatePropertyPanel> m_panels = new();
        TemplateEditSession m_session;
        Button m_highlightButton;
        bool m_highlighting;

        /// <summary>The property groups to show, in display order.</summary>
        protected abstract IReadOnlyList<TemplatePropertyGroup> Groups { get; }

        /// <summary>The kind of map data (Segment, Region, MapObject) whose instances use this Template type.
        /// Null (the default) shows no highlight buttons.</summary>
        protected virtual Type HighlightedDataType => null;

        /// <summary>Returns a test telling whether an instance of HighlightedDataType with the given name
        /// uses the inspected Template, or null if it can't be built.</summary>
        protected virtual Func<string, bool> CreateNameMatcher() => null;

        /// <summary>Builds one titled panel per group, bound to a direct session on the inspected Template.</summary>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            ApplyStyleSheets(root);

            var template = (TemplateAsset)target;
            m_session = new TemplateEditSession(template, asset => EditorUtility.SetDirty(asset), direct: true);
            m_session.BeforeEdit += () => Undo.RecordObject(template, "Edit Template");

            if (HighlightedDataType != null)
                root.Add(BuildHighlightRow());

            m_panels.Clear();
            foreach (var group in Groups)
            {
                var title = new Label(group.Title);
                title.AddToClassList(SectionTitleClass);
                root.Add(title);

                var panel = new TemplatePropertyPanel(target.GetType(), group.Properties, group.Columns, group.ClearAllButton, group.Placeholders, group.Note);
                panel.SetStep(FloatStep);
                panel.Bind(m_session);
                root.Add(panel);
                m_panels.Add(panel);
            }

            root.RegisterCallback<AttachToPanelEvent>(_ => Undo.undoRedoPerformed += RefreshPanels);
            root.RegisterCallback<DetachFromPanelEvent>(_ => Undo.undoRedoPerformed -= RefreshPanels);
            return root;
        }

        VisualElement BuildHighlightRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 6;

            m_highlightButton = BuildButton(HighlightText, HighlightInMap);
            row.Add(m_highlightButton);
            row.Add(BuildButton("Clear highlight", ClearHighlight));
            SetHighlighting(m_highlighting);
            return row;
        }

        static Button BuildButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.AddToClassList(AcknexManipulatorStyles.TemplateBarButton);
            return button;
        }

        void HighlightInMap()
        {
            var matcher = CreateNameMatcher();
            if (matcher == null)
                return;

            EditorEntryPoint.HighlightObjects(HighlightedDataType, matcher);
            SetHighlighting(true);
        }

        void ClearHighlight()
        {
            EditorEntryPoint.ClearHighlights();
            SetHighlighting(false);
        }

        void SetHighlighting(bool highlighting)
        {
            m_highlighting = highlighting;
            if (m_highlightButton == null)
                return;

            m_highlightButton.text = highlighting ? HighlightActiveText : HighlightText;
            m_highlightButton.EnableInClassList(AcknexManipulatorStyles.TemplateBarButtonActive, highlighting);
        }

        /// <summary>Removes a highlight this Inspector set when it goes away, e.g. when another asset is selected.</summary>
        void OnDisable()
        {
            if (m_highlighting)
                ClearHighlight();
        }

        void RefreshPanels()
        {
            foreach (var panel in m_panels)
                panel.Refresh();
        }

        static void ApplyStyleSheets(VisualElement root)
        {
            var baseStyleSheet = Resources.Load<StyleSheet>(BaseStyleSheetName);
            if (baseStyleSheet != null)
                root.styleSheets.Add(baseStyleSheet);
            else
                Debug.LogWarning($"TemplateInspector: could not load '{BaseStyleSheetName}' from a Resources folder.");

            AcknexManipulatorStyles.ApplyTo(root);
        }
    }
}
