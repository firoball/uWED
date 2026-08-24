using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Region tab. Min/Max read-only. FloorHgt/CeilHgt editable steppers.
    /// Standalone Name picker (the ONE Region.Name - separate from textures).
    /// Two NameTextureSlots (Floor/Ceiling) for texture display only - Name
    /// dropdown and Offset edit fields are hidden on both (textures are
    /// unrelated to Region.Name, and Region has no per-slot offset property);
    /// both slots share one ITextureProvider.
    /// </summary>
    public class RegionManipulator : ManipulatorWindowBase<Region>
    {
        protected override string TypeLabel => "Region";
        protected override bool UsesAngleStep => false;

        INameProvider m_regionNameProvider;
        ITextureProvider m_textureProvider;

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
        /// textureProvider is shared by both Floor and Ceiling slots.</summary>
        public void SetProviders(INameProvider regionNameProvider, ITextureProvider textureProvider)
        {
            m_regionNameProvider = regionNameProvider;
            m_textureProvider = textureProvider;
            RefreshRegionNameChoices();
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

        // Region's own identity Name - standalone, separate from either
        // texture slot.
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

            m_regionNameProvider?.TryCreateName(sanitized);
            RefreshRegionNameChoices();
            m_nameDropdown.value = sanitized; // always applied, regardless of provider outcome
        }

        void RefreshRegionNameChoices()
        {
            if (m_regionNameProvider == null || m_nameDropdown == null) return;
            var names = new List<string>(m_regionNameProvider.GetNames());
            string currentValue = m_nameDropdown.value;
            m_nameDropdown.schedule.Execute(() =>
            {
                m_nameDropdown.choices = names;
                m_nameDropdown.SetValueWithoutNotify(currentValue);
            });
        }

        void BuildSlots(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-slots-row");

            m_floorSlot = new NameTextureSlot();
            m_floorSlot.AddToClassList("manip-slot-first");
            SetupTextureOnlySlot(m_floorSlot, hint: "Floor");
            row.Add(m_floorSlot);

            m_ceilSlot = new NameTextureSlot();
            SetupTextureOnlySlot(m_ceilSlot, hint: "Ceiling");
            row.Add(m_ceilSlot);

            container.Add(row);
        }

        void SetupTextureOnlySlot(NameTextureSlot slot, string hint)
        {
            slot.SetNameSectionVisible(false);
            slot.SetOffsetSectionVisible(false);
            slot.TextureHintValue.text = hint;

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

            RefreshRegionNameChoices();
            m_nameNewEntry.style.display = DisplayStyle.None;
            m_nameDropdown.SetValueWithoutNotify(copy.Name);
            RefreshRegionNameChoices(); // captures the just-set value, applies it after deferred choices assignment

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
            CommitPendingRegionNameIfAny();
            target.FloorHgt = m_floorHgtStepper.Value;
            target.CeilHgt = m_ceilHgtStepper.Value;
            target.Name = m_nameDropdown.value;
        }

        void CommitPendingRegionNameIfAny()
        {
            if (m_nameNewEntry.style.display == DisplayStyle.Flex && !string.IsNullOrEmpty(m_nameNewEntry.value))
                CommitNewRegionName();
        }

        // ---- Extension point ----

        /// <summary>Texture Name/Scale for a slot (placeholder - textures do have names,
        /// just no real data wired yet). Override once real texture data exists.</summary>
        protected virtual void LoadTextureInfo(NameTextureSlot slot, Region copy, bool isFloor)
        {
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }
    }
}
