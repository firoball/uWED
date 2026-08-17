using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Shared scaffold for a single object type's value-editing popup: header
    /// (type + index), settings bar (linear/angle step, always snapped), one Tab
    /// for now (more per-type tabs to follow later), footer (Cancel/OK).
    ///
    /// Editing works on a local copy: Open() clones the source object, all fields
    /// read/write the copy, Cancel just discards it. OK writes the copy back via
    /// WriteBack(). No live-apply, no revert logic needed.
    ///
    /// Subclasses only need to implement the four abstract members below - all
    /// shared chrome lives here and in ManipulatorBase.uxml/.uss.
    /// </summary>
    /// <typeparam name="T">The data class being edited (Vertex, Segment, ...).</typeparam>
    public abstract class ManipulatorWindowBase<T> : VisualElement
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

        T m_editedCopy;
        T m_originalTarget;

        /// <summary>Display name for the header, e.g. "Vertex", "Segment".</summary>
        protected abstract string TypeLabel { get; }

        /// <summary>
        /// Whether this type has any field using the linear step (most types do).
        /// Override to false to grey out the settings-bar field instead of leaving
        /// it live but meaningless.
        /// </summary>
        protected virtual bool UsesLinearStep => true;

        /// <summary>Whether this type has any field using the angle step.</summary>
        protected virtual bool UsesAngleStep => true;

        /// <summary>
        /// Build this type's fields into <paramref name="container"/>. Called once,
        /// right after construction. Read-only context rows go first (wrapped in a
        /// "manip-readonly-block" VisualElement), editable fields after.
        /// </summary>
        protected abstract void PopulateContent(VisualElement container);

        /// <summary>Produce an editable copy of <paramref name="source"/>.</summary>
        protected abstract T Clone(T source);

        /// <summary>Push m_editedCopy's values into the UI fields built in PopulateContent.</summary>
        protected abstract void LoadValues(T copy);

        /// <summary>Write the edited copy's values back onto the real target object.</summary>
        protected abstract void WriteBack(T target, T editedCopy);

        protected ManipulatorWindowBase(VisualTreeAsset baseUxml, IManipulatorSettings settings)
        {
            Settings = settings;

            this.StretchToParentSize();
            this.pickingMode = PickingMode.Ignore; // wrapper only, must not block clicks

            VisualElement instance = baseUxml.Instantiate();
            instance.StretchToParentSize();
            instance.pickingMode = PickingMode.Ignore; // pass-through layer

            m_manipRoot = instance.Q<VisualElement>("ManipulatorRoot");
            m_manipRoot.style.display = DisplayStyle.None;
            m_manipRoot.pickingMode = PickingMode.Position; // the actual click barrier
            m_manipRoot.focusable = true;

            m_typeLabel = instance.Q<Label>("manip-type-label");
            m_closeButton = instance.Q<Button>("manip-close-button");
            m_linearStepField = instance.Q<FloatField>("manip-linear-step");
            m_angleStepField = instance.Q<FloatField>("manip-angle-step");
            m_contentContainer = instance.Q<VisualElement>("manip-content-container");
            m_cancelButton = instance.Q<Button>("manip-cancel-button");
            m_okButton = instance.Q<Button>("manip-ok-button");

            m_typeLabel.text = TypeLabel; // index appended once Open() knows it

            // Settings bar: shared across all tabs/types, backed by caller's
            // IManipulatorSettings (EditorPrefs-backed, wired by the caller).
            m_linearStepField.SetValueWithoutNotify(Settings.LinearStep);
            m_angleStepField.SetValueWithoutNotify(Settings.AngleStep);
            m_linearStepField.RegisterValueChangedCallback(evt => Settings.LinearStep = evt.newValue);
            m_angleStepField.RegisterValueChangedCallback(evt => Settings.AngleStep = evt.newValue);

            // Grey out (not hide) whichever step isn't relevant to this type - keeps
            // the settings bar's layout identical across every tab/type instead of
            // fields jumping position depending on what's currently open.
            m_linearStepField.SetEnabled(UsesLinearStep);
            m_angleStepField.SetEnabled(UsesAngleStep);
            if (!UsesLinearStep) m_linearStepField.tooltip = "Not used by this object type";
            if (!UsesAngleStep) m_angleStepField.tooltip = "Not used by this object type";

            m_closeButton.clicked += Cancel;
            m_cancelButton.clicked += Cancel;
            m_okButton.clicked += Apply;

            m_manipRoot.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    Cancel();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    Apply();
                    evt.StopPropagation();
                }
            });

            PopulateContent(m_contentContainer);

            Add(instance);
        }

        /// <summary>Open the window for editing <paramref name="target"/>.</summary>
        public void Open(T target, int index)
        {
            m_originalTarget = target;
            m_editedCopy = Clone(target);

            m_typeLabel.text = $"{TypeLabel} #{index}";
            LoadValues(m_editedCopy);

            m_manipRoot.style.display = DisplayStyle.Flex;
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
            {
                WriteBack(m_originalTarget, m_editedCopy);
            }
            m_manipRoot.style.display = DisplayStyle.None;
            m_editedCopy = default;
            m_originalTarget = default;
        }

        /// <summary>
        /// Current linear step, for subclasses wiring up NumberStepperFields.
        /// Re-reads Settings each time rather than caching, since the field is
        /// editable live in the settings bar.
        /// </summary>
        protected float CurrentLinearStep => Settings.LinearStep;
        protected float CurrentAngleStep => Settings.AngleStep;
    }
}
