using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// One composite stepper for a Vector2 - X and Y sub-fields each with their
    /// own [-][+] (needed since callers may want to disable one axis without
    /// affecting the other, e.g. Segment's per-axis Auto Align). Replaces
    /// building two independent NumberStepperField rows by hand for every
    /// Vector2 value (Segment offset today, others later).
    ///
    /// Optional per-axis Auto Align toggles sit directly next to their own
    /// axis (X toggle right after the X stepper, Y toggle right after the Y
    /// stepper) rather than trailing at the end of the row - keeps the whole
    /// row compact and each toggle visually paired with what it affects.
    /// </summary>
    public class Vector2StepperField : VisualElement
    {
        public event Action<Vector2> ValueChanged;
        public event Action<bool> AutoAlignXChanged;
        public event Action<bool> AutoAlignYChanged;

        public NumberStepperField XField { get; }
        public NumberStepperField YField { get; }

        /// <summary>Null unless this instance was built with showAutoAlignToggles: true.</summary>
        public Toggle AutoAlignXToggle { get; }
        public Toggle AutoAlignYToggle { get; }

        public float Step
        {
            get => XField.Step;
            set
            {
                XField.Step = value;
                YField.Step = value;
            }
        }

        public Vector2 Value
        {
            get => new Vector2(XField.Value, YField.Value);
            set
            {
                XField.Value = value.x;
                YField.Value = value.y;
            }
        }

        public Vector2StepperField(bool showAutoAlignToggles = false)
        {
            AddToClassList("vector2-stepper-row");

            var xLabel = new Label("X");
            xLabel.AddToClassList("vector2-stepper-axis-label");

            XField = new NumberStepperField();
            XField.AddToClassList("vector2-stepper-axis");
            XField.ValueChanged += x => ValueChanged?.Invoke(new Vector2(x, YField.Value));

            var yLabel = new Label("Y");
            yLabel.AddToClassList("vector2-stepper-axis-label");

            YField = new NumberStepperField();
            YField.AddToClassList("vector2-stepper-axis");
            YField.ValueChanged += y => ValueChanged?.Invoke(new Vector2(XField.Value, y));

            Add(xLabel);
            Add(XField);

            if (showAutoAlignToggles)
            {
                AutoAlignXToggle = new Toggle { tooltip = "Auto Align X" };
                AutoAlignXToggle.AddToClassList("vector2-stepper-autoalign");
                AutoAlignXToggle.RegisterValueChangedCallback(evt =>
                {
                    XField.SetFieldsEnabled(!evt.newValue);
                    AutoAlignXChanged?.Invoke(evt.newValue);
                });
                Add(AutoAlignXToggle);
            }

            Add(yLabel);
            Add(YField);

            if (showAutoAlignToggles)
            {
                AutoAlignYToggle = new Toggle { tooltip = "Auto Align Y" };
                AutoAlignYToggle.AddToClassList("vector2-stepper-autoalign");
                AutoAlignYToggle.RegisterValueChangedCallback(evt =>
                {
                    YField.SetFieldsEnabled(!evt.newValue);
                    AutoAlignYChanged?.Invoke(evt.newValue);
                });
                Add(AutoAlignYToggle);
            }
        }

        public void SetXEnabled(bool enabled) => XField.SetFieldsEnabled(enabled);
        public void SetYEnabled(bool enabled) => YField.SetFieldsEnabled(enabled);
    }
}
