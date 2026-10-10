using System.Collections.Generic;
using UI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Runtime.Core.Map.Model;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Region tab. Min/Max read-only. FloorHgt/CeilHgt editable as one "Heights" stepper pair (Floor/Ceil
    /// side by side, same shape as Segment's Texture Offset). Name
    /// is a plain rename field (ComboBoxField, string-typed, via
    /// IGenericNameProvider<string>) - same shape as MapObject/Way/Segment's
    /// Name fields. Two NameTextureSlots (Floor/Ceiling) for texture display
    /// only (Offset section hidden on both - no confirmed per-surface offset
    /// property); both slots share one texture provider.
    /// </summary>
    public class RegionManipulator : ManipulatorWindowBase<Region>
    {
        protected override string TypeLabel => "Region";

        /// <summary>The Floor (X) / Ceil (Y) height stepper, for subclasses adding content next to it.</summary>
        protected Vector2StepperField HeightsStepper => m_heightsStepper;
        protected override bool UsesAngleStep => false;

        IGenericNameProvider<string> m_nameProvider = new SimpleGenericNameProvider(new List<string>());
        ITextureProvider m_textureProvider;

        Label m_minValue;
        Label m_maxValue;
        Vector2StepperField m_heightsStepper; // X = FloorHgt, Y = CeilHgt
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
        public virtual void SetProviders(IGenericNameProvider<string> nameProvider, ITextureProvider textureProvider)
        {
            m_nameProvider = nameProvider ?? new SimpleGenericNameProvider(new List<string>());
            m_textureProvider = textureProvider;
            WireNameProvider();
        }

        /// <summary>Call before Open, next to SetProviders. countByName holds, for every distinct Region
        /// name in the map, the number of Regions carrying it. The base class ignores it; a subclass
        /// overrides this to show usage information.</summary>
        public virtual void SetCountByName(IReadOnlyDictionary<string, int> countByName)
        {
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
            var title = new Label("Heights");
            title.AddToClassList("manip-section-title");
            container.Add(title);

            m_heightsStepper = new Vector2StepperField("Floor", "Ceil");
            m_heightsStepper.ValueChanged += v =>
            {
                if (m_current == null) return;
                m_current.FloorHgt = v.x;
                m_current.CeilHgt = v.y;
            };
            container.Add(m_heightsStepper);
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

            m_heightsStepper.Step = CurrentLinearStep;
            m_heightsStepper.Value = new Vector2(copy.FloorHgt, copy.CeilHgt);

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
            target.FloorHgt = m_heightsStepper.Value.x;
            target.CeilHgt = m_heightsStepper.Value.y;
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
