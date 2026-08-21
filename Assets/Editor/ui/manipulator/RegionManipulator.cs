using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Region tab. Min/Max read-only. FloorHgt/CeilHgt editable steppers.
    /// Standalone Name picker (Region's own identity - separate from any
    /// texture name). Two NameTextureSlots (Floor/Ceiling) for texture display
    /// - each slot's own Name dropdown is a texture-pairing name, distinct from
    /// Region.Name, and unwired for now (no confirmed Floor/Ceiling name
    /// property), same as Segment's slot 2.
    /// </summary>
    public class RegionManipulator : ManipulatorWindowBase<Region>
    {
        protected override string TypeLabel => "Region";
        protected override bool UsesAngleStep => false;

        INameProvider m_regionNameProvider;
        INameProvider m_floorNameProvider;
        ITextureProvider m_floorTextureProvider;
        INameProvider m_ceilNameProvider;
        ITextureProvider m_ceilTextureProvider;

        Label m_minValue;
        Label m_maxValue;
        NumberStepperField m_floorHgtStepper;
        NumberStepperField m_ceilHgtStepper;

        DropdownField m_nameDropdown;
        TextField m_nameNewEntry;

        NameTextureSlot m_floorSlot;
        NameTextureSlot m_ceilSlot;

        Region m_current;

        public RegionManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once providers are ready. regionNameProvider backs Region.Name;
        /// floor/ceil pairs back their slots' texture displays (unwired name for now).</summary>
        public void SetProviders(INameProvider regionNameProvider,
            INameProvider floorNameProvider, ITextureProvider floorTextureProvider,
            INameProvider ceilNameProvider, ITextureProvider ceilTextureProvider)
        {
            m_regionNameProvider = regionNameProvider;
            m_floorNameProvider = floorNameProvider;
            m_floorTextureProvider = floorTextureProvider;
            m_ceilNameProvider = ceilNameProvider;
            m_ceilTextureProvider = ceilTextureProvider;
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

        // Region's own identity Name - standalone, same picker markup as
        // NameTextureSlot's Name section but not paired with any texture.
        void BuildNameField(VisualElement container)
        {
            var title = new Label("Name");
            title.AddToClassList("manip-section-title");
            container.Add(title);

            var row = new VisualElement();
            row.AddToClassList("manip-picker-row");

            m_nameDropdown = new DropdownField();
            m_nameDropdown.AddToClassList("manip-picker-dropdown");
            m_nameDropdown.formatListItemCallback = DisplayLowercase;
            m_nameDropdown.formatSelectedValueCallback = DisplayLowercase;
            row.Add(m_nameDropdown);

            var newButton = new Button { text = "+" };
            newButton.AddToClassList("manip-picker-new-button");
            row.Add(newButton);

            container.Add(row);

            m_nameNewEntry = new TextField { isDelayed = false };
            m_nameNewEntry.AddToClassList("manip-picker-new-entry");
            container.Add(m_nameNewEntry);

            newButton.clicked += () => ShowNewEntry(m_nameNewEntry);
            m_nameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNewRegionName();
                    evt.StopPropagation();
                }
            });
        }

        static void ShowNewEntry(TextField entry)
        {
            entry.style.display = DisplayStyle.Flex;
            entry.SetValueWithoutNotify(string.Empty);
            entry.Focus();
        }

        static string DisplayLowercase(string value) => string.IsNullOrEmpty(value) ? value : value.ToLowerInvariant();

        void CommitNewRegionName()
        {
            string sanitized = NameSanitizer.Sanitize(m_nameNewEntry.value);
            m_nameNewEntry.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(sanitized)) return;

            if (m_regionNameProvider != null && m_regionNameProvider.TryCreateName(sanitized))
            {
                RefreshRegionNameChoices();
                m_nameDropdown.value = sanitized;
            }
        }

        void RefreshRegionNameChoices()
        {
            if (m_regionNameProvider == null) return;
            var names = new List<string>(m_regionNameProvider.GetNames());
            m_nameDropdown.schedule.Execute(() => m_nameDropdown.choices = names);
        }

        void BuildSlots(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-slots-row");

            m_floorSlot = new NameTextureSlot();
            m_floorSlot.AddToClassList("manip-slot-first");
            WireSlot(m_floorSlot, isFloor: true);
            row.Add(m_floorSlot);

            m_ceilSlot = new NameTextureSlot();
            WireSlot(m_ceilSlot, isFloor: false);
            row.Add(m_ceilSlot);

            container.Add(row);
        }

        // Slot Name dropdown here is the texture-pairing name, not Region.Name.
        void WireSlot(NameTextureSlot slot, bool isFloor)
        {
            slot.NameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNewSlotName(slot, isFloor);
                    evt.StopPropagation();
                }
            });

            slot.TextureSelectButton.clicked += () =>
            {
                var provider = TextureProviderFor(isFloor);
                var names = provider != null ? provider.GetTextureNames() : (IReadOnlyList<string>)new List<string>();
                Debug.Log($"TODO: open texture selection menu. Available: {string.Join(", ", names)}");
            };
        }

        void CommitNewSlotName(NameTextureSlot slot, bool isFloor)
        {
            string sanitized = NameSanitizer.Sanitize(slot.NameNewEntry.value);
            slot.NameNewEntry.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(sanitized)) return;

            var provider = SlotNameProviderFor(isFloor);
            if (provider != null && provider.TryCreateName(sanitized))
            {
                RefreshSlotNameChoices(slot, isFloor);
                slot.NameDropdown.value = sanitized;
            }
        }

        INameProvider SlotNameProviderFor(bool isFloor) => isFloor ? m_floorNameProvider : m_ceilNameProvider;
        ITextureProvider TextureProviderFor(bool isFloor) => isFloor ? m_floorTextureProvider : m_ceilTextureProvider;

        void RefreshSlotNameChoices(NameTextureSlot slot, bool isFloor)
        {
            var provider = SlotNameProviderFor(isFloor);
            if (provider == null) return;
            var names = new List<string>(provider.GetNames());
            slot.schedule.Execute(() => slot.SetNameChoices(names));
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

            RefreshRegionNameChoices();
            m_nameNewEntry.style.display = DisplayStyle.None;
            m_nameDropdown.SetValueWithoutNotify(copy.Name);

            RefreshSlotNameChoices(m_floorSlot, isFloor: true);
            m_floorSlot.NameNewEntry.style.display = DisplayStyle.None;
            m_floorSlot.OffsetStepper.Step = CurrentLinearStep;
            LoadTextureInfo(m_floorSlot, copy, isFloor: true);
            LoadSlot(m_floorSlot, copy, isFloor: true);

            RefreshSlotNameChoices(m_ceilSlot, isFloor: false);
            m_ceilSlot.NameNewEntry.style.display = DisplayStyle.None;
            m_ceilSlot.OffsetStepper.Step = CurrentLinearStep;
            LoadTextureInfo(m_ceilSlot, copy, isFloor: false);
            LoadSlot(m_ceilSlot, copy, isFloor: false);
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
            target.Name = m_nameDropdown.value;
            WriteBackSlot(target, m_floorSlot, isFloor: true);
            WriteBackSlot(target, m_ceilSlot, isFloor: false);
        }

        // ---- Extension points (texture-pairing name, not Region.Name) ----

        protected virtual void LoadTextureInfo(NameTextureSlot slot, Region copy, bool isFloor)
        {
            slot.TextureHintValue.text = isFloor ? "Floor" : "Ceiling";
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }

        /// <summary>No confirmed Floor/Ceiling name property on Region yet - unwired, like Segment's slot 2.</summary>
        protected virtual void LoadSlot(NameTextureSlot slot, Region copy, bool isFloor)
        {
            slot.NameDropdown.SetValueWithoutNotify(string.Empty);
            slot.OffsetStepper.Value = Vector2.zero;
        }

        /// <summary>No-op until Region exposes a Floor/Ceiling texture name property.</summary>
        protected virtual void WriteBackSlot(Region target, NameTextureSlot slot, bool isFloor)
        {
        }
    }
}
