using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Shared scaffold: header, settings bar, TabView (one Tab by default),
    /// footer. Local-copy editing - Open() clones, fields edit the copy,
    /// Cancel discards it, OK writes back via WriteBack().
    /// </summary>
    /// <typeparam name="T">Data class being edited - must derive from IndexedData.</typeparam>
    public abstract class ManipulatorWindowBase<T> : VisualElement where T : IndexedData
    {
        protected readonly IManipulatorSettings Settings;

        VisualElement m_manipRoot;
        Label m_typeLabel;
        Button m_closeButton;
        FloatField m_linearStepField;
        FloatField m_angleStepField;
        VisualElement m_contentContainer;
        Button m_cancelButton;
        Button m_okButton;
        TabView m_tabView;

        T m_editedCopy;
        T m_originalTarget;

        /// <summary>Real object Open() was called with, not the edit copy. Use for
        /// read-only fields Clone() doesn't carry over.</summary>
        protected T OriginalTarget => m_originalTarget;

        /// <summary>TabView itself, for subclasses adding extra tabs.</summary>
        protected TabView TabView => m_tabView;

        protected abstract string TypeLabel { get; }
        protected virtual bool UsesLinearStep => true;
        protected virtual bool UsesAngleStep => true;

        protected abstract void PopulateContent(VisualElement container);
        protected abstract T Clone(T source);
        protected abstract void LoadValues(T copy);
        protected abstract void WriteBack(T target, T editedCopy);

        protected ManipulatorWindowBase(VisualTreeAsset baseUxml, IManipulatorSettings settings)
        {
            Settings = settings;

            this.StretchToParentSize();
            this.pickingMode = PickingMode.Ignore;

            VisualElement instance = baseUxml.Instantiate();
            instance.StretchToParentSize();
            instance.pickingMode = PickingMode.Ignore;

            m_manipRoot = instance.Q<VisualElement>("ManipulatorRoot");
            m_manipRoot.style.display = DisplayStyle.None;
            m_manipRoot.pickingMode = PickingMode.Position; // click barrier
            m_manipRoot.focusable = true;
            m_manipRoot.tabIndex = -1; // focusable via code, but not its own Tab stop

            m_typeLabel = instance.Q<Label>("manip-type-label");
            m_closeButton = instance.Q<Button>("manip-close-button");
            m_linearStepField = instance.Q<FloatField>("manip-linear-step");
            m_angleStepField = instance.Q<FloatField>("manip-angle-step");
            m_contentContainer = instance.Q<VisualElement>("manip-content-container");
            m_cancelButton = instance.Q<Button>("manip-cancel-button");
            m_okButton = instance.Q<Button>("manip-ok-button");
            m_tabView = instance.Q<TabView>("manip-tabview");

            m_typeLabel.text = TypeLabel;

            m_linearStepField.SetValueWithoutNotify(Settings.LinearStep);
            m_angleStepField.SetValueWithoutNotify(Settings.AngleStep);
            m_linearStepField.RegisterValueChangedCallback(evt => Settings.LinearStep = evt.newValue);
            m_angleStepField.RegisterValueChangedCallback(evt => Settings.AngleStep = evt.newValue);

            m_linearStepField.SetEnabled(UsesLinearStep);
            m_angleStepField.SetEnabled(UsesAngleStep);
            if (!UsesLinearStep) m_linearStepField.tooltip = "Not used by this object type";
            if (!UsesAngleStep) m_angleStepField.tooltip = "Not used by this object type";

            // Close ("X") performs the same function as Cancel - not a separate
            // Tab stop, same as Cancel/OK remain normal Tab stops in the footer.
            m_closeButton.focusable = false;
            m_closeButton.clicked += Cancel;
            m_cancelButton.clicked += Cancel;
            m_okButton.clicked += Apply;

            m_manipRoot.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape) { Cancel(); evt.StopPropagation(); }
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) { Apply(); evt.StopPropagation(); }
            });

            // Focus trap: default Tab navigation runs untouched (so in-window
            // tabbing behaves normally); if it ever lands outside m_manipRoot
            // (only possible at the very first/last field), snap back in.
            // Registered on the panel root, not m_manipRoot, since the escaping
            // element is by definition not a descendant of m_manipRoot anymore.
            RegisterCallback<AttachToPanelEvent>(evt =>
                evt.destinationPanel?.visualTree.RegisterCallback<FocusInEvent>(OnPanelFocusIn, TrickleDown.TrickleDown));
            RegisterCallback<DetachFromPanelEvent>(evt =>
                evt.originPanel?.visualTree.UnregisterCallback<FocusInEvent>(OnPanelFocusIn, TrickleDown.TrickleDown));

            PopulateContent(m_contentContainer);

            Add(instance);
        }

        public void Open(T target)
        {
            m_originalTarget = target;
            m_editedCopy = Clone(target);
            m_typeLabel.text = $"{TypeLabel} #{target.Index}";

            // display:Flex before LoadValues(): populating a DropdownField's
            // choices while still display:none corrupts its popup measurement.
            m_manipRoot.style.display = DisplayStyle.Flex;
            LoadValues(m_editedCopy);

            m_manipRoot.Focus();
        }

        void Cancel()
        {
            m_manipRoot.style.display = DisplayStyle.None;
            m_editedCopy = default;
            m_originalTarget = default;
        }

        void Apply()
        {
            if (m_originalTarget != null)
                WriteBack(m_originalTarget, m_editedCopy);
            m_manipRoot.style.display = DisplayStyle.None;
            m_editedCopy = default;
            m_originalTarget = default;
        }

        void OnPanelFocusIn(FocusInEvent evt)
        {
            if (m_manipRoot.style.display != DisplayStyle.Flex) return;
            if (evt.target is VisualElement target && target != m_manipRoot && !m_manipRoot.Contains(target))
                m_manipRoot.Focus();
        }

        protected float CurrentLinearStep => Settings.LinearStep;
        protected float CurrentAngleStep => Settings.AngleStep;
    }
}
