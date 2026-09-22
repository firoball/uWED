using System.Collections.Generic;
using UnityEngine.UIElements;
using UI.Controls;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Way tab. Name is a plain rename field (ComboBoxField, string-typed, via
    /// IGenericNameProvider&lt;string&gt;) - same shape as MapObject/Region/Segment's
    /// Name fields. Vertex list is shown read-only as its Count.
    /// </summary>
    public class WayManipulator : ManipulatorWindowBase<Way>
    {
        protected override string TypeLabel => "Way";
        protected override bool UsesLinearStep => false;
        protected override bool UsesAngleStep => false;

        IGenericNameProvider<string> m_nameProvider = new SimpleGenericNameProvider(new List<string>());

        VisualElement m_nameFieldContainer;
        ComboBoxField m_nameCombo;

        Label m_vertexCountValue;

        public WayManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once the provider is ready (null falls back to the default in-memory provider).</summary>
        public void SetProviders(IGenericNameProvider<string> nameProvider)
        {
            m_nameProvider = nameProvider ?? new SimpleGenericNameProvider(new List<string>());
            WireNameProvider();
        }

        void WireNameProvider()
        {
            m_nameCombo.Choices = m_nameProvider.Choices;
            m_nameCombo.ItemFactory = m_nameProvider.ItemFactory;
            m_nameCombo.Sanitizer = m_nameProvider.Sanitizer;
        }

        /// <summary>Hides the Name title + combo box together - for a subclass
        /// that replaces the plain rename field with a template picker.</summary>
        protected void SetNameFieldVisible(bool visible)
        {
            m_nameFieldContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildNameField(container);
            BuildReadonlyBlock(container);
        }

        void BuildNameField(VisualElement container)
        {
            m_nameFieldContainer = new VisualElement();

            var title = new Label("Name");
            title.AddToClassList("manip-section-title");
            m_nameFieldContainer.Add(title);

            m_nameCombo = new ComboBoxField();
            m_nameCombo.AddToClassList("manip-picker-dropdown");
            m_nameFieldContainer.Add(m_nameCombo);

            container.Add(m_nameFieldContainer);

            WireNameProvider(); // default provider active immediately, even before SetProviders is called
        }

        void BuildReadonlyBlock(VisualElement container)
        {
            var block = new VisualElement();
            block.AddToClassList("manip-readonly-block");

            m_vertexCountValue = AddReadonlyRow(block, "Vertices");

            container.Add(block);
        }

        static Label AddReadonlyRow(VisualElement block, string labelText)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-readonly-row");

            var label = new Label(labelText);
            label.AddToClassList("manip-readonly-label");
            row.Add(label);

            var value = new Label();
            value.AddToClassList("manip-readonly-value");
            row.Add(value);

            block.Add(row);
            return value;
        }

        protected override Way Clone(Way source)
        {
            // Positions has no public setter beyond the ctor - pass the same
            // list reference through (Way's vertex list is not editable here).
            return new Way(source.Positions, source.Name);
        }

        protected override void LoadValues(Way copy)
        {
            m_nameCombo.Refresh();
            m_nameCombo.SetValueWithoutNotify(copy.Name);

            m_vertexCountValue.text = OriginalTarget.Positions != null
                ? OriginalTarget.Positions.Count.ToString()
                : "0";
        }

        protected override void WriteBack(Way target, Way editedCopy)
        {
            target.Name = m_nameCombo.value;
        }
    }
}
