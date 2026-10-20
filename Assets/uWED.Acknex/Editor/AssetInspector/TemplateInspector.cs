using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Acknex.Runtime.UI.Manipulator;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Editor.Bootstrap;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>
    /// Base of the Unity Inspector for a Template asset: shows the property groups of the Template type's
    /// layout one under the other, using the same TemplatePropertyPanel as the Manipulator tabs. Edits go
    /// straight to the asset through a direct TemplateEditSession (no Edit/Apply protection), each one
    /// recorded for Undo and marking the asset dirty. Edits are reported to the Template registry (when a
    /// subclass provides it), and changes made elsewhere reload the fields. A "Name" label shows the
    /// link name. A "Step" field (shown when there are property groups) sets the step of the float fields;
    /// its value is kept in EditorPrefs and shared by all Template Inspectors. A subclass names the layout
    /// of its Template type and, to offer the map highlight buttons, the kind of map data using it (see
    /// HighlightedDataType).
    /// </summary>
    public abstract class TemplateInspector : UnityEditor.Editor
    {
        const string StepPrefsKey = "uWED.Acknex::Editor::AssetInspector::FloatStep";
        const float DefaultFloatStep = 0.1f;
        const float MinFloatStep = 0.001f;
        const float StepFieldStep = 0.01f;
        const string BaseStyleSheetName = "ManipulatorBase";
        const string SectionTitleClass = "manip-section-title";
        const string HighlightText = "Highlight in Map";
        const string HighlightActiveText = "Highlighted";

        readonly List<TemplatePropertyPanel> m_panels = new();
        TemplateEditSession m_session;
        Label m_nameLabel;
        NumberStepperField m_stepField;
        float m_step;
        Button m_highlightButton;
        bool m_highlighting;
        bool m_notifying;

        /// <summary>The registry of the inspected Template's type, or null if there is none. Edits are
        /// reported to it, and its changes reload the fields.</summary>
        protected virtual ITemplateRegistry Registry => null;

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
            m_session = new TemplateEditSession(template, MarkDirty, direct: true);
            m_session.BeforeEdit += () => Undo.RecordObject(template, "Edit Template");

            m_step = Mathf.Max(EditorPrefs.GetFloat(StepPrefsKey, DefaultFloatStep), MinFloatStep);

            if (HighlightedDataType != null)
                root.Add(BuildHighlightRow());
            root.Add(BuildNameRow());
            if (Groups.Count > 0)
                root.Add(BuildStepRow());
            root.Add(BuildSeparator());

            m_panels.Clear();
            foreach (var group in Groups)
            {
                var title = new Label(group.Title);
                title.AddToClassList(SectionTitleClass);
                root.Add(title);

                var panel = new TemplatePropertyPanel(target.GetType(), group.Properties, group.Columns, group.ClearAllButton, group.Placeholders, group.Note);
                panel.SetStep(m_step);
                panel.Bind(m_session);
                root.Add(panel);
                m_panels.Add(panel);
            }

            root.RegisterCallback<AttachToPanelEvent>(_ => Subscribe());
            root.RegisterCallback<DetachFromPanelEvent>(_ => Unsubscribe());
            return root;
        }

        VisualElement BuildNameRow()
        {
            var row = new VisualElement();
            row.AddToClassList("manip-readonly-row");

            var label = new Label("Name");
            label.AddToClassList("manip-readonly-label");
            row.Add(label);

            m_nameLabel = new Label(((TemplateAsset)target).Name);
            m_nameLabel.AddToClassList("manip-readonly-value");
            row.Add(m_nameLabel);
            return row;
        }

        VisualElement BuildStepRow()
        {
            var row = new VisualElement();
            row.AddToClassList("manip-field-row");

            var label = new Label("Step");
            label.AddToClassList("manip-field-label");
            row.Add(label);

            m_stepField = new NumberStepperField { Step = StepFieldStep, Value = m_step };
            m_stepField.ValueChanged += OnStepChanged;
            row.Add(m_stepField);
            return row;
        }

        static VisualElement BuildSeparator()
        {
            var separator = new VisualElement();
            separator.style.height = 1;
            separator.style.marginTop = 4;
            separator.style.marginBottom = 6;
            separator.style.backgroundColor = new Color(1f, 1f, 1f, 0.15f);
            return separator;
        }

        void OnStepChanged(float value)
        {
            m_step = Mathf.Max(value, MinFloatStep);
            if (!Mathf.Approximately(m_step, value))
                m_stepField.Value = m_step;

            EditorPrefs.SetFloat(StepPrefsKey, m_step);
            foreach (var panel in m_panels)
                panel.SetStep(m_step);
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

        /// <summary>Marks the asset for saving by Unity and tells the registry; this Inspector's own fields
        /// are not reloaded for its own notification.</summary>
        void MarkDirty(TemplateAsset asset)
        {
            EditorUtility.SetDirty(asset);
            Notify(asset);
        }

        void Notify(TemplateAsset asset)
        {
            m_notifying = true;
            Registry?.NotifyChanged(asset);
            m_notifying = false;
        }

        void Subscribe()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            var registry = Registry;
            if (registry != null)
                registry.TemplateChanged += OnTemplateChanged;
        }

        void Unsubscribe()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            var registry = Registry;
            if (registry != null)
                registry.TemplateChanged -= OnTemplateChanged;
        }

        void OnUndoRedo()
        {
            RefreshPanels();
            Notify((TemplateAsset)target);
        }

        void OnTemplateChanged(TemplateAsset changed)
        {
            if (!m_notifying && changed == target)
                RefreshPanels();
        }

        void RefreshPanels()
        {
            m_nameLabel.text = ((TemplateAsset)target).Name;
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
