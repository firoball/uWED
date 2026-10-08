using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.Platform;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Shared scaffold: header, settings bar, TabView (one Tab by default),
    /// footer. Local-copy editing - Open() clones, fields edit the copy,
    /// Cancel discards it, OK writes back via WriteBack().
    /// Optionally shows a type selector above the content, editing IndexedData.TypeId. The entries come
    /// from a registered IManipulatorTypeProvider; without one (or without entries for T) no selector is shown.
    /// </summary>
    /// <typeparam name="T">Data class being edited - must derive from IndexedData.</typeparam>
    public abstract class ManipulatorWindowBase<T> : VisualElement where T : IndexedData
    {
        /// <summary>Settings shared by all Manipulators (step sizes).</summary>
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

        VisualElement m_typeSelectorRow;
        DropdownField m_typeSelector;
        readonly List<ManipulatorTypeOption> m_typeOptions = new List<ManipulatorTypeOption>();

        T m_editedCopy;
        T m_originalTarget;

        /// <summary>USS class marking a NumberStepperField as an angle stepper. Every NumberStepperField in the
        /// window (including the axes of a Vector2StepperField) follows the settings bar's linear step,
        /// both while loading and while the window is open, except those carrying this class, which follow
        /// the angle step. Add it to every stepper editing an angle.</summary>
        protected const string AngleStepperClass = "manip-angle-stepper";

        /// <summary>Real object Open() was called with, not the edit copy. Use for
        /// read-only fields Clone() doesn't carry over.</summary>
        protected T OriginalTarget => m_originalTarget;

        /// <summary>TabView itself, for subclasses adding extra tabs.</summary>
        protected TabView TabView => m_tabView;

        /// <summary>TypeId of the edit copy: the value currently shown in the type selector. Equals the
        /// target's TypeId right after Open() until the user picks another type.</summary>
        protected int EditedTypeId => m_editedCopy != null ? m_editedCopy.TypeId : 0;

        /// <summary>Header text naming the edited object type (the object's Index is appended on Open()).</summary>
        protected abstract string TypeLabel { get; }

        /// <summary>False disables the linear step field (with tooltip) for objects without linear values.</summary>
        protected virtual bool UsesLinearStep => true;

        /// <summary>False disables the angle step field (with tooltip) for objects without an angle.</summary>
        protected virtual bool UsesAngleStep => true;

        /// <summary>False shows the type selector disabled (with tooltip) instead of editable. Evaluated on every Open().</summary>
        protected virtual bool AllowsTypeChange => true;

        /// <summary>Builds the Manipulator specific fields into the content container, once, at construction.</summary>
        protected abstract void PopulateContent(VisualElement container);

        /// <summary>Returns an editable copy of source. TypeId is copied by the base class afterwards.</summary>
        protected abstract T Clone(T source);

        /// <summary>Fills the fields from the edit copy. Called on every Open().</summary>
        protected abstract void LoadValues(T copy);

        /// <summary>Writes the edit copy's values into target. TypeId is written by the base class before this call.</summary>
        protected abstract void WriteBack(T target, T editedCopy);

        /// <summary>Called after the user picked another entry in the type selector. EditedTypeId already holds
        /// newTypeId. Not called for the initial selection on Open().</summary>
        protected virtual void OnTypeChanged(int newTypeId) { }

        /// <summary>Builds the shared scaffold from baseUxml and lets the subclass populate its content.</summary>
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
            m_linearStepField.RegisterValueChangedCallback(evt =>
            {
                Settings.LinearStep = evt.newValue;
                ApplyStepToSteppers(evt.newValue, angle: false);
            });
            m_angleStepField.RegisterValueChangedCallback(evt =>
            {
                Settings.AngleStep = evt.newValue;
                ApplyStepToSteppers(evt.newValue, angle: true);
            });

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

        /// <summary>Opens the Manipulator on target: edits a clone (TypeId included), shows the window and loads the fields.</summary>
        public void Open(T target)
        {
            m_originalTarget = target;
            m_editedCopy = Clone(target);
            m_editedCopy.TypeId = target.TypeId;
            m_typeLabel.text = $"{TypeLabel} #{target.Index}";

            // display:Flex before RefreshTypeSelector()/LoadValues(): populating a DropdownField's
            // choices while still display:none corrupts its popup measurement.
            m_manipRoot.style.display = DisplayStyle.Flex;
            RefreshTypeSelector();
            LoadValues(m_editedCopy);
            m_manipRoot.Focus();
        }

        /// <summary>Writes the current edit copy into the original target without closing the window.</summary>
        public void ApplyNow()
        {
            Commit();
        }

        void Commit()
        {
            if (m_originalTarget == null) return;
            m_originalTarget.TypeId = m_editedCopy.TypeId;
            WriteBack(m_originalTarget, m_editedCopy);
        }

        void Cancel()
        {
            m_manipRoot.style.display = DisplayStyle.None;
            m_editedCopy = default;
            m_originalTarget = default;
        }

        void Apply()
        {
            Commit();
            m_manipRoot.style.display = DisplayStyle.None;
            m_editedCopy = default;
            m_originalTarget = default;
        }

        /// <summary>
        /// Queries the IManipulatorTypeProvider for T and (re)fills the type selector. The selector is built
        /// lazily the first time options exist and is inserted at the top of the content container as a
        /// regular field row (label column + dropdown); the row is hidden whenever there are no options.
        /// </summary>
        void RefreshTypeSelector()
        {
            m_typeOptions.Clear();
            if (ServiceLocator.TryGet<IManipulatorTypeProvider>(out var provider))
            {
                IReadOnlyList<ManipulatorTypeOption> options = provider.GetOptions(typeof(T));
                if (options != null) m_typeOptions.AddRange(options);
            }

            if (m_typeOptions.Count == 0)
            {
                if (m_typeSelectorRow != null) m_typeSelectorRow.style.display = DisplayStyle.None;
                return;
            }

            if (m_typeSelector == null)
            {
                m_typeSelectorRow = new VisualElement();
                m_typeSelectorRow.AddToClassList("manip-field-row");

                var label = new Label("Type");
                label.AddToClassList("manip-field-label");
                m_typeSelectorRow.Add(label);

                m_typeSelector = new DropdownField();
                m_typeSelector.AddToClassList("manip-picker-dropdown");
                m_typeSelector.RegisterValueChangedCallback(OnTypeSelectorChanged);
                m_typeSelectorRow.Add(m_typeSelector);

                m_contentContainer.Insert(0, m_typeSelectorRow);
            }

            int currentId = m_editedCopy.TypeId;
            int selectedIndex = m_typeOptions.FindIndex(o => o.Id == currentId);

            // A TypeId the provider doesn't list stays visible (and unchanged) as its own entry.
            if (selectedIndex < 0)
            {
                m_typeOptions.Add(new ManipulatorTypeOption(currentId, "(unknown)"));
                selectedIndex = m_typeOptions.Count - 1;
            }

            var choices = new List<string>(m_typeOptions.Count);
            foreach (var option in m_typeOptions) choices.Add($"{option.Id} - {option.Label}");

            m_typeSelectorRow.style.display = DisplayStyle.Flex;
            m_typeSelector.choices = choices;
            m_typeSelector.SetValueWithoutNotify(choices[selectedIndex]);

            bool allowed = AllowsTypeChange;
            m_typeSelector.SetEnabled(allowed);
            m_typeSelector.tooltip = allowed ? string.Empty : "Type can't be changed for this object";
        }

        void OnTypeSelectorChanged(ChangeEvent<string> evt)
        {
            int index = m_typeSelector.index;
            if (index < 0 || index >= m_typeOptions.Count || m_editedCopy == null) return;

            int newId = m_typeOptions[index].Id;
            if (newId == m_editedCopy.TypeId) return;

            m_editedCopy.TypeId = newId;
            OnTypeChanged(newId);
        }

        /// <summary>Pushes a changed step size to every NumberStepperField in the window, so the new value
        /// applies while the window stays open. Steppers carrying AngleStepperClass follow the angle step,
        /// all others (including the axes of a Vector2StepperField) the linear step.</summary>
        void ApplyStepToSteppers(float step, bool angle)
        {
            m_manipRoot.Query<NumberStepperField>().ForEach(stepper =>
            {
                if (stepper.ClassListContains(AngleStepperClass) == angle)
                    stepper.Step = step;
            });
        }

        void OnPanelFocusIn(FocusInEvent evt)
        {
            if (m_manipRoot.style.display != DisplayStyle.Flex) return;
            if (evt.target is VisualElement target && target != m_manipRoot && !m_manipRoot.Contains(target))
                m_manipRoot.Focus();
        }

        /// <summary>Linear step size currently set in the settings bar.</summary>
        protected float CurrentLinearStep => Settings.LinearStep;

        /// <summary>Angle step size currently set in the settings bar.</summary>
        protected float CurrentAngleStep => Settings.AngleStep;
    }
}
