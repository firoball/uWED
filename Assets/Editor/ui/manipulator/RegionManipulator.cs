using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Controls;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Region tab. Min/Max read-only. FloorHgt/CeilHgt editable steppers. Name
    /// is a plain rename field (ComboBoxField, string-typed, via
    /// IGenericNameProvider&lt;string&gt;) - same shape as MapObject/Way/Segment's
    /// Name fields. Two NameTextureSlots (Floor/Ceiling) for texture display
    /// only (Offset section hidden on both - no confirmed per-surface offset
    /// property); both slots share one texture provider.
    /// </summary>
    public class RegionManipulator : ManipulatorWindowBase<Region>
    {
        protected override string TypeLabel => "Region";
        protected override bool UsesAngleStep => false;

        IGenericNameProvider<string> m_nameProvider = new SimpleGenericNameProvider(new List<string>());
        ITextureProvider m_textureProvider;

        Label m_minValue;
        Label m_maxValue;
        NumberStepperField m_floorHgtStepper;
        NumberStepperField m_ceilHgtStepper;

        VisualElement m_nameFieldContainer;
        ComboBoxField m_nameCombo;

        NameTextureSlot m_floorSlot;
        NameTextureSlot m_ceilSlot;

        Region m_current;

        public RegionManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once providers are ready. nameProvider backs the Name combo box
        /// (null falls back to the default in-memory provider); textureProvider backs both
        /// texture slots' "..." select (placeholder, shared between Floor and Ceiling).</summary>
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
        /// that replaces the plain rename field with a template picker.</summary>
        protected void SetNameFieldVisible(bool visible)
        {
            m_nameFieldContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildHeightFields(container);
            BuildNameField(container);
            BuildSlots(container);
        }

        void BuildReadonlyBlock(VisualElement container)
        {
            var block = new VisualElement();
            block.AddToClassList("manip-readonly-block");
            m_minValue = AddReadonlyRow(block, "Min");
            m_maxValue = AddReadonlyRow(block, "Max");
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

        void BuildHeightFields(VisualElement container)
        {
            m_floorHgtStepper = BuildFieldRow(container, "Floor Height", v => { if (m_current != null) m_current.FloorHgt = v; });
            m_ceilHgtStepper = BuildFieldRow(container, "Ceiling Height", v => { if (m_current != null) m_current.CeilHgt = v; });
        }

        static NumberStepperField BuildFieldRow(VisualElement container, string labelText, System.Action<float> onChanged)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-field-row");
            var label = new Label(labelText);
            label.AddToClassList("manip-field-label");
            row.Add(label);
            var stepper = new NumberStepperField();
            stepper.ValueChanged += onChanged;
            row.Add(stepper);
            container.Add(row);
            return stepper;
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

        void BuildSlots(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-slots-row");

            m_floorSlot = new NameTextureSlot();
            m_floorSlot.AddToClassList("manip-slot-first");
            m_floorSlot.SetOffsetSectionVisible(false); // no confirmed per-surface offset property
            WireSlotTextureButton(m_floorSlot);
            row.Add(m_floorSlot);

            m_ceilSlot = new NameTextureSlot();
            m_ceilSlot.SetOffsetSectionVisible(false);
            WireSlotTextureButton(m_ceilSlot);
            row.Add(m_ceilSlot);

            container.Add(row);
        }

        void WireSlotTextureButton(NameTextureSlot slot)
        {
            slot.TextureSelectButton.clicked += () =>
            {
                var names = m_textureProvider != null
                    ? m_textureProvider.GetTextureNames()
                    : (IReadOnlyList<string>)new List<string>();
                Debug.Log($"TODO: open texture selection menu. Available: {string.Join(", ", names)}");
            };
        }

        protected override Region Clone(Region source)
        {
            return new Region(source.FloorHgt, source.CeilHgt, source.Name);
        }

        protected override void LoadValues(Region copy)
        {
            m_current = copy;

            m_minValue.text = FormatVector(OriginalTarget.Min);
            m_maxValue.text = FormatVector(OriginalTarget.Max);

            m_floorHgtStepper.Step = CurrentLinearStep;
            m_floorHgtStepper.Value = copy.FloorHgt;
            m_ceilHgtStepper.Step = CurrentLinearStep;
            m_ceilHgtStepper.Value = copy.CeilHgt;

            m_nameCombo.Refresh();
            m_nameCombo.SetValueWithoutNotify(copy.Name);

            LoadTextureInfo(m_floorSlot, copy, isFloor: true);
            LoadTextureInfo(m_ceilSlot, copy, isFloor: false);
        }

        static string FormatVector(Vector3 v)
        {
            string x = v.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string y = v.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string z = v.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            return $"x {x} y {y} z {z}";
        }

        protected override void WriteBack(Region target, Region editedCopy)
        {
            target.FloorHgt = m_floorHgtStepper.Value;
            target.CeilHgt = m_ceilHgtStepper.Value;
            target.Name = m_nameCombo.value;
        }

        // ---- Extension point ----

        /// <summary>Texture Name/Scale for a slot (placeholder). Override once real texture data exists.</summary>
        protected virtual void LoadTextureInfo(NameTextureSlot slot, Region copy, bool isFloor)
        {
            slot.TextureHintValue.text = isFloor ? "Floor" : "Ceiling";
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }
    }
}
