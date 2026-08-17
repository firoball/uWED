using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Vertex tab: only Z is editable in 3D (matches the existing vertical-only
    /// vertex movement rule). X/Y are shown read-only for context.
    ///
    /// Vertex has no public X/Y/Z setters - it's only constructible via a
    /// Vector3 constructor, so Clone() rebuilds through that rather than an
    /// object initializer. Internally X/Y/Z are stored as double, so values
    /// need an explicit (float) cast going into Vector3/NumberStepperField
    /// (double -> float is narrowing) - the reverse (float -> double, e.g. in
    /// WriteBack) is implicit and needs no cast.
    /// </summary>
    public class VertexManipulator : ManipulatorWindowBase<Vertex>
    {
        protected override string TypeLabel => "Vertex";
        protected override bool UsesAngleStep => false;


        Label m_posXValue;
        Label m_posYValue;
        NumberStepperField m_zStepper;

        Vertex m_current;

        public VertexManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        protected override void PopulateContent(VisualElement container)
        {
            // --- Read-only context (X/Y locked) ---
            var readonlyBlock = new VisualElement();
            readonlyBlock.AddToClassList("manip-readonly-block");

            var posXRow = new VisualElement();
            posXRow.AddToClassList("manip-readonly-row");
            posXRow.Add(new Label("X") { });
            m_posXValue = new Label();
            posXRow.Add(m_posXValue);
            StyleReadonlyRow(posXRow, m_posXValue);
            readonlyBlock.Add(posXRow);

            var posYRow = new VisualElement();
            posYRow.AddToClassList("manip-readonly-row");
            posYRow.Add(new Label("Y"));
            m_posYValue = new Label();
            posYRow.Add(m_posYValue);
            StyleReadonlyRow(posYRow, m_posYValue);
            readonlyBlock.Add(posYRow);

            container.Add(readonlyBlock);

            // --- Editable ---
            var zRow = new VisualElement();
            zRow.AddToClassList("manip-field-row");
            var zLabel = new Label("Z");
            zLabel.AddToClassList("manip-field-label");
            zRow.Add(zLabel);

            m_zStepper = new NumberStepperField { Step = CurrentLinearStep };
            m_zStepper.ValueChanged += v =>
            {
                if (m_current != null)
                    m_current.Z = v;
            };
            zRow.Add(m_zStepper);

            container.Add(zRow);
        }

        static void StyleReadonlyRow(VisualElement row, Label valueLabel)
        {
            var labelEl = row[0] as Label;
            labelEl?.AddToClassList("manip-readonly-label");
            valueLabel.AddToClassList("manip-readonly-value");
        }

        protected override Vertex Clone(Vertex source)
        {
            // X/Y/Z have no public setters - only settable together via this
            // Vector3 constructor. double -> float narrowing needs an explicit cast.
            return new Vertex(new Vector3((float)source.X, (float)source.Y, (float)source.Z));
        }

        protected override void LoadValues(Vertex copy)
        {
            m_current = copy;

            m_posXValue.text = copy.X.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            m_posYValue.text = copy.Y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

            m_zStepper.Step = CurrentLinearStep;
            m_zStepper.Value = (float)copy.Z; // double -> float, explicit cast required
        }

        protected override void WriteBack(Vertex target, Vertex editedCopy)
        {
            // Only Z is editable - X/Y intentionally left untouched.
            // float -> double is a widening/implicit conversion, no cast needed.
            target.Z = editedCopy.Z;
        }
    }
}
