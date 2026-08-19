using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <typeparam name="T">
    /// The data class being edited (Vertex, Segment, ...) - must derive from
    /// IndexedData so Open() can read .Index itself instead of taking it as a
    /// separate parameter.
    /// </typeparam>
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

        T m_editedCopy;
        T m_originalTarget;

        /// <summary>
        /// The real object Open() was called with - not the local edit copy.
        /// For read-only display fields that Clone() doesn't carry over (e.g.
        /// Segment.LeftRegion/RightRegion aren't settable through its
        /// constructor), read from here instead of the LoadValues(T copy)
        /// parameter, which would otherwise show stale/default data.
        /// </summary>
        protected T OriginalTarget => m_originalTarget;

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
            // tabIndex -1: still focusable via code (Open() calls .Focus() on it
            // so ESC/Enter work immediately without clicking a field first), but
            // excluded from the Tab-key cycle. Without this it was an invisible
            // stop - unstyled, no focus ring, easy to mistake for "focus lost".
            m_manipRoot.tabIndex = -1;

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

            // Focus trap: registering Tab-handling on m_manipRoot alone is not
            // enough. It only fires for events bubbling up FROM one of its own
            // descendants - but the bug is Tab moving focus OUT of the overlay
            // into the background UI in the first place, at which point the
            // focused element is no longer a descendant of m_manipRoot at all,
            // so a handler there would never see it. Registering on the actual
            // panel root instead (an ancestor of both the overlay and the
            // background) with TrickleDown means we intercept every Tab press
            // in the whole panel during the capture phase, before UI Toolkit's
            // own default navigation gets a chance to move focus anywhere - so
            // we can keep it confined to m_manipRoot's own focusable
            // descendants regardless of where focus currently happens to be.
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                evt.destinationPanel?.visualTree.RegisterCallback<KeyDownEvent>(OnPanelKeyDown, TrickleDown.TrickleDown);
            });
            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                evt.originPanel?.visualTree.UnregisterCallback<KeyDownEvent>(OnPanelKeyDown, TrickleDown.TrickleDown);
            });

            PopulateContent(m_contentContainer);

            Add(instance);
        }

        /// <summary>Open the window for editing <paramref name="target"/>.</summary>
        public void Open(T target)
        {
            m_originalTarget = target;
            m_editedCopy = Clone(target);

            m_typeLabel.text = $"{TypeLabel} #{target.Index}";

            // Show first, populate after: fields like the Name DropdownField
            // measure their popup against the current layout. Populating while
            // m_manipRoot is still display:none means that measurement happens
            // against zero-size geometry, which shows up as blank rows in the
            // popup until something (e.g. a manual scroll) forces a relayout.
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
            {
                WriteBack(m_originalTarget, m_editedCopy);
            }
            m_manipRoot.style.display = DisplayStyle.None;
            m_editedCopy = default;
            m_originalTarget = default;
        }

        void OnPanelKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Tab) return;
            if (m_manipRoot.style.display != DisplayStyle.Flex) return; // this window isn't open

            var focusable = GetFocusableDescendants();
            if (focusable.Count == 0) return;

            var current = panel?.focusController?.focusedElement as VisualElement;
            int currentIndex = current != null ? focusable.IndexOf(current) : -1;

            int nextIndex;
            if (evt.shiftKey)
                nextIndex = currentIndex <= 0 ? focusable.Count - 1 : currentIndex - 1;
            else
                nextIndex = currentIndex < 0 || currentIndex >= focusable.Count - 1 ? 0 : currentIndex + 1;

            focusable[nextIndex].Focus();
            evt.StopPropagation();
            evt.PreventDefault();
        }

        /// <summary>
        /// m_manipRoot's focusable descendants, in visual tree order. currentIndex
        /// of -1 (nothing focused, or focus was somewhere outside m_manipRoot
        /// entirely - the exact bug this trap exists to fix) is treated the same
        /// as "before the first element", so Tab still lands somewhere sane.
        /// </summary>
        List<VisualElement> GetFocusableDescendants()
        {
            return m_manipRoot.Query<VisualElement>()
                .Where(e => e.focusable && e.tabIndex >= 0 && e.enabledInHierarchy && IsActuallyVisible(e))
                .Build()
                .ToList();
        }

        /// <summary>
        /// resolvedStyle.display only reflects an element's OWN display value,
        /// not whether an ancestor collapsed it (e.g. a field inside the hidden
        /// slot 2 still reports its own display as Flex). Checking actual
        /// rendered size catches that case too.
        /// </summary>
        static bool IsActuallyVisible(VisualElement e)
        {
            return e.resolvedStyle.display != DisplayStyle.None
                && e.worldBound.width > 0 && e.worldBound.height > 0;
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
