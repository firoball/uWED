using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// One composite stepper for a Vector2 - X and Y sub-fields each with their
    /// own [-][+]. Used for Segment's Offset (and any future Vector2 value)
    /// instead of building two independent NumberStepperField rows by hand.
    /// </summary>
    public class Vector2StepperField : VisualElement
    {
        public event Action<Vector2> ValueChanged;

        public NumberStepperField XField { get; }
        public NumberStepperField YField { get; }

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

        public Vector2StepperField()
        {
            AddToClassList("vector2-stepper-row");

            var xLabel = new Label("X");
            xLabel.AddToClassList("vector2-stepper-axis-label");
            xLabel.AddToClassList("vector2-stepper-axis-label-first");

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
            Add(yLabel);
            Add(YField);
        }

        public void SetXEnabled(bool enabled) => XField.SetFieldsEnabled(enabled);
        public void SetYEnabled(bool enabled) => YField.SetFieldsEnabled(enabled);
    }
}
