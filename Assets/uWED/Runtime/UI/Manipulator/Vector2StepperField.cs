using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// One composite stepper for a Vector2 - X and Y sub-fields each with their
    /// own [-][+]. Used for Segment's Offset (and any future Vector2 value)
    /// instead of building two independent NumberStepperField rows by hand.
    /// The two axis labels default to "X"/"Y" and can be replaced (e.g. "Floor"/"Ceil" for Region heights).
    /// Further elements added with Add() are placed at the end of the row, after both axes.
    /// </summary>
    public class Vector2StepperField : VisualElement
    {
        /// <summary>Raised with the new value whenever either axis changes.</summary>
        public event Action<Vector2> ValueChanged;

        /// <summary>Stepper for the X component.</summary>
        public NumberStepperField XField { get; }

        /// <summary>Stepper for the Y component.</summary>
        public NumberStepperField YField { get; }

        /// <summary>Step size applied to both axes.</summary>
        public float Step
        {
            get => XField.Step;
            set
            {
                XField.Step = value;
                YField.Step = value;
            }
        }

        /// <summary>Current value of both axes.</summary>
        public Vector2 Value
        {
            get => new Vector2(XField.Value, YField.Value);
            set
            {
                XField.Value = value.x;
                YField.Value = value.y;
            }
        }

        /// <summary>Creates the field with the given axis labels.</summary>
        public Vector2StepperField(string xLabelText = "X", string yLabelText = "Y")
        {
            AddToClassList("vector2-stepper-row");

            var xLabel = new Label(xLabelText);
            xLabel.AddToClassList("vector2-stepper-axis-label");
            xLabel.AddToClassList("vector2-stepper-axis-label-first");

            XField = new NumberStepperField();
            XField.AddToClassList("vector2-stepper-axis");
            XField.ValueChanged += x => ValueChanged?.Invoke(new Vector2(x, YField.Value));

            var yLabel = new Label(yLabelText);
            yLabel.AddToClassList("vector2-stepper-axis-label");

            YField = new NumberStepperField();
            YField.AddToClassList("vector2-stepper-axis");
            YField.ValueChanged += y => ValueChanged?.Invoke(new Vector2(XField.Value, y));

            Add(xLabel);
            Add(XField);
            Add(yLabel);
            Add(YField);
        }

        /// <summary>Enables or disables the X stepper's fields.</summary>
        public void SetXEnabled(bool enabled) => XField.SetFieldsEnabled(enabled);

        /// <summary>Enables or disables the Y stepper's fields.</summary>
        public void SetYEnabled(bool enabled) => YField.SetFieldsEnabled(enabled);
    }
}
