using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Controls;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Segment tab. Vertex1/Vertex2/Left Region/Right Region/Length read-only.
    /// Name is a plain rename field (ComboBoxField, string-typed, via
    /// IGenericNameProvider&lt;string&gt;) - same shape as MapObject/Region/Way's
    /// Name fields (no special-cased template type at the base level). One
    /// NameTextureSlot, mandatory - Offset section stays visible (genuinely
    /// instance data, backed by Segment.Offset directly, unrelated to Name).
    ///
    /// A second slot is not base-class behavior - see
    /// docs/SegmentManipulator.md for the extension pattern (a derived
    /// ExtendedSegment adding Offset2, built the same way MapObject's template
    /// picker extension is built).
    /// </summary>
    public class SegmentManipulator : ManipulatorWindowBase<Segment>
    {
        protected override string TypeLabel => "Segment";
        protected override bool UsesAngleStep => false;

        IGenericNameProvider<string> m_nameProvider = new SimpleGenericNameProvider(new List<string>());
        ITextureProvider m_textureProvider;

        Label m_vertex1Value;
        Label m_vertex2Value;
        Label m_regionLeftValue;
        Label m_regionRightValue;
        Label m_lengthValue;

        VisualElement m_nameFieldContainer;
        ComboBoxField m_nameCombo;

        NameTextureSlot m_slot;

        public SegmentManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once providers are ready. nameProvider backs the Name combo box
        /// (null falls back to the default in-memory provider); textureProvider backs the
        /// texture slot's "..." select (placeholder).</summary>
        public void SetProviders(IGenericNameProvider<string> nameProvider, ITextureProvider textureProvider)
        {
            m_nameProvider = nameProvider ?? new SimpleGenericNameProvider(new List<string>());
            m_textureProvider = textureProvider;
            WireNameProvider();
        }

        void WireNameProvider()
        {
            m_nameCombo.Choices = m_nameProvider.Choices;
            m_nameCombo.ItemFactory = m_nameProvider.ItemFactory;
            m_nameCombo.Sanitizer = m_nameProvider.Sanitizer;
        }

        /// <summary>Hides the Name title + combo box together - for a subclass
        /// that replaces the plain rename field with a template picker (see
        /// docs/SegmentManipulator.md).</summary>
        protected void SetNameFieldVisible(bool visible)
        {
            m_nameFieldContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildNameField(container);
            BuildSlot(container);
        }

        void BuildReadonlyBlock(VisualElement container)
        {
            var block = new VisualElement();
            block.AddToClassList("manip-readonly-block");

            m_vertex1Value = AddReadonlyRow(block, "Vertex 1");
            m_vertex2Value = AddReadonlyRow(block, "Vertex 2");
            m_regionLeftValue = AddReadonlyRow(block, "Left Region");
            m_regionRightValue = AddReadonlyRow(block, "Right Region");
            m_lengthValue = AddReadonlyRow(block, "Length");

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

        void BuildSlot(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-slots-row");

            m_slot = new NameTextureSlot();
            m_slot.AddToClassList("manip-slot-first");
            // Offset section stays visible (default) - Segment.Offset is real,
            // instance-specific data, unlike MapObject/Region's hidden offsets.

            m_slot.TextureSelectButton.clicked += () =>
            {
                var names = m_textureProvider != null
                    ? m_textureProvider.GetTextureNames()
                    : (IReadOnlyList<string>)new List<string>();
                Debug.Log($"TODO: open texture selection menu. Available: {string.Join(", ", names)}");
            };

            row.Add(m_slot);
            container.Add(row);
        }

        protected override Segment Clone(Segment source)
        {
            // Vertex1/Vertex2 only settable via constructor, not an initializer.
            return new Segment(source.Vertex1, source.Vertex2)
            {
                Name = source.Name,
                Offset = source.Offset
            };
        }

        protected override void LoadValues(Segment copy)
        {
            m_vertex1Value.text = FormatVertex(copy.Vertex1);
            m_vertex2Value.text = FormatVertex(copy.Vertex2);
            m_regionLeftValue.text = FormatRegionRef(OriginalTarget.Left);
            m_regionRightValue.text = FormatRegionRef(OriginalTarget.Right);
            m_lengthValue.text = FormatSegmentLength(OriginalTarget.Length);

            m_nameCombo.Refresh();
            m_nameCombo.SetValueWithoutNotify(copy.Name);

            m_slot.OffsetStepper.Step = CurrentLinearStep;
            m_slot.OffsetStepper.Value = copy.Offset;
            LoadTextureInfo(m_slot, copy);
        }

        static string FormatVertex(Vertex v)
        {
            if (v == null) return "-";
            string x = v.X.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string y = v.Y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string z = v.Z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            return $"x {x} y {y} z {z}";
        }

        static string FormatRegionRef(Region region)
        {
            return region == null ? "-" : $"{region.Name} #{region.Index}";
        }

        static string FormatSegmentLength(float f)
        {
            return f.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        protected override void WriteBack(Segment target, Segment editedCopy)
        {
            target.Name = m_nameCombo.value;
            target.Offset = m_slot.OffsetStepper.Value;
        }

        // ---- Extension point ----

        /// <summary>Texture Name/Scale for the slot (placeholder). Override once real texture data exists.</summary>
        protected virtual void LoadTextureInfo(NameTextureSlot slot, Segment copy)
        {
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }
    }
}
