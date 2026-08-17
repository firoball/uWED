using System;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// A number field with dedicated [-][+] buttons. Snapping is always on (no
    /// toggle) - Step is whatever the shared settings bar currently holds; the
    /// buttons nudge by exactly Step, typing a value directly bypasses snapping.
    /// Used by every object-type Manipulator instead of duplicating this markup.
    /// </summary>
    public class NumberStepperField : VisualElement
    {
        public event Action<float> ValueChanged;

        readonly FloatField m_field;
        readonly Button m_decButton;
        readonly Button m_incButton;

        public float Step { get; set; } = 0.1f;

        public float Value
        {
            get => m_field.value;
            set => m_field.SetValueWithoutNotify(value);
        }

        public NumberStepperField()
        {
            AddToClassList("stepper-row");

            m_decButton = new Button(() => Nudge(-1)) { text = "-" };
            m_decButton.AddToClassList("stepper-btn");

            m_field = new FloatField();
            m_field.AddToClassList("stepper-field");
            m_field.RegisterValueChangedCallback(evt => ValueChanged?.Invoke(evt.newValue));

            m_incButton = new Button(() => Nudge(1)) { text = "+" };
            m_incButton.AddToClassList("stepper-btn");

            Add(m_decButton);
            Add(m_field);
            Add(m_incButton);
        }

        void Nudge(int direction)
        {
            m_field.value += direction * Step;
            // FloatField.value setter fires the change event itself, ValueChanged
            // above will be invoked through RegisterValueChangedCallback.
        }

        /// <summary>
        /// Enables/disables the inner field and both buttons together. Named
        /// distinctly from VisualElement's own SetEnabled() (which only toggles
        /// this wrapper, not its children) to avoid confusion.
        /// </summary>
        public void SetFieldsEnabled(bool enabled)
        {
            m_field.SetEnabled(enabled);
            m_decButton.SetEnabled(enabled);
            m_incButton.SetEnabled(enabled);
        }
    }
}
