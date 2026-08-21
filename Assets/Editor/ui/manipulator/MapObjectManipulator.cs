using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// MapObject tab. Position is read-only (shown Vertex-style, X/Y only - a
    /// MapObject's Position is a Vector2, no Z), Angle is editable via a
    /// NumberStepperField snapped to the shared Angle Step, Region is
    /// read-only ("{Name} #{Index}", matches Segment's Left/Right formatting).
    /// One NameTextureSlot, wired the same way as Segment's slot 1.
    ///
    /// MapObject(Vector2 position, float angle, Region region, string name) is
    /// the only ctor that carries every field; MapObject(Vector2 position)
    /// forwards to it with angle 0 / region null / name null and isn't used
    /// here since Clone() always has a full source object to copy from.
    /// </summary>
    public class MapObjectManipulator : ManipulatorWindowBase<MapObject>
    {
        protected override string TypeLabel => "Object";

        INameProvider m_objectNameProvider;
        INameProvider m_slotNameProvider;
        ITextureProvider m_textureProvider;

        Label m_posXValue;
        Label m_posYValue;
        Label m_regionValue;
        NumberStepperField m_angleStepper;

        DropdownField m_nameDropdown;
        TextField m_nameNewEntry;

        NameTextureSlot m_slot;

        MapObject m_current;

        public MapObjectManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once providers are ready. objectNameProvider backs MapObject.Name;
        /// slotNameProvider/textureProvider back the texture slot (unwired name for now).</summary>
        public void SetProviders(INameProvider objectNameProvider, INameProvider slotNameProvider, ITextureProvider textureProvider)
        {
            m_objectNameProvider = objectNameProvider;
            m_slotNameProvider = slotNameProvider;
            m_textureProvider = textureProvider;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildAngleField(container);
            BuildNameField(container);
            BuildSlot(container);
        }

        // MapObject's own identity Name - standalone, separate from the
        // texture slot's Name dropdown.
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
                    CommitNewObjectName();
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

        void CommitNewObjectName()
        {
            string sanitized = NameSanitizer.Sanitize(m_nameNewEntry.value);
            m_nameNewEntry.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(sanitized)) return;

            if (m_objectNameProvider != null && m_objectNameProvider.TryCreateName(sanitized))
            {
                RefreshObjectNameChoices();
                m_nameDropdown.value = sanitized;
            }
        }

        void RefreshObjectNameChoices()
        {
            if (m_objectNameProvider == null) return;
            var names = new List<string>(m_objectNameProvider.GetNames());
            m_nameDropdown.schedule.Execute(() => m_nameDropdown.choices = names);
        }

        void BuildReadonlyBlock(VisualElement container)
        {
            var block = new VisualElement();
            block.AddToClassList("manip-readonly-block");

            m_posXValue = AddReadonlyRow(block, "X");
            m_posYValue = AddReadonlyRow(block, "Y");
            m_regionValue = AddReadonlyRow(block, "Region");

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

        void BuildAngleField(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-field-row");

            var label = new Label("Angle");
            label.AddToClassList("manip-field-label");
            row.Add(label);

            m_angleStepper = new NumberStepperField { Step = CurrentAngleStep };
            m_angleStepper.ValueChanged += v =>
            {
                if (m_current != null)
                    m_current.Angle = v;
            };
            row.Add(m_angleStepper);

            container.Add(row);
        }

        void BuildSlot(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-slots-row");

            m_slot = new NameTextureSlot();
            m_slot.AddToClassList("manip-slot-first");
            WireSlot(m_slot);
            row.Add(m_slot);

            container.Add(row);
        }

        // Slot Name dropdown here is the texture-pairing name, not MapObject.Name.
        void WireSlot(NameTextureSlot slot)
        {
            slot.NameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNewSlotName(slot);
                    evt.StopPropagation();
                }
            });

            slot.TextureSelectButton.clicked += () =>
            {
                var names = m_textureProvider != null
                    ? m_textureProvider.GetTextureNames()
                    : (IReadOnlyList<string>)new List<string>();
                Debug.Log($"TODO: open texture selection menu. Available: {string.Join(", ", names)}");
            };
        }

        void CommitNewSlotName(NameTextureSlot slot)
        {
            string sanitized = NameSanitizer.Sanitize(slot.NameNewEntry.value);
            slot.NameNewEntry.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(sanitized)) return;

            if (m_slotNameProvider != null && m_slotNameProvider.TryCreateName(sanitized))
            {
                RefreshSlotNameChoices(slot);
                slot.NameDropdown.value = sanitized;
            }
        }

        // schedule.Execute defers one frame - avoids a DropdownField popup
        // measuring against a not-yet-laid-out panel (blank rows until scrolled).
        void RefreshSlotNameChoices(NameTextureSlot slot)
        {
            if (m_slotNameProvider == null) return;
            var names = new List<string>(m_slotNameProvider.GetNames());
            slot.schedule.Execute(() => slot.SetNameChoices(names));
        }

        protected override MapObject Clone(MapObject source)
        {
            // Position is exposed as a Vector2 (internally backed by a Vertex,
            // but the ctor only ever takes the raw Vector2 - never the Vertex itself).
            return new MapObject(source.Position, source.Angle, source.Region, source.Name);
        }

        protected override void LoadValues(MapObject copy)
        {
            m_current = copy;

            m_posXValue.text = copy.Position.X.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            m_posYValue.text = copy.Position.Y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            m_regionValue.text = FormatRegionRef(OriginalTarget.Region);

            m_angleStepper.Step = CurrentAngleStep;
            m_angleStepper.Value = copy.Angle;

            RefreshObjectNameChoices();
            m_nameNewEntry.style.display = DisplayStyle.None;
            m_nameDropdown.SetValueWithoutNotify(copy.Name);

            RefreshSlotNameChoices(m_slot);
            m_slot.NameNewEntry.style.display = DisplayStyle.None;
            m_slot.OffsetStepper.Step = CurrentLinearStep;
            LoadTextureInfo(m_slot, copy);
            LoadSlot(m_slot, copy);
        }

        static string FormatRegionRef(Region region)
        {
            return region == null ? "-" : $"{region.Name} #{region.Index}";
        }

        protected override void WriteBack(MapObject target, MapObject editedCopy)
        {
            target.Angle = m_angleStepper.Value;
            target.Name = m_nameDropdown.value;
            WriteBackSlot(target, m_slot);
        }

        // ---- Extension points (texture-pairing name, not MapObject.Name) ----

        /// <summary>Texture Name/Scale for the slot. Override once real texture data exists.</summary>
        protected virtual void LoadTextureInfo(NameTextureSlot slot, MapObject copy)
        {
            slot.TextureHintValue.text = "Hint";
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }

        /// <summary>No confirmed texture-pairing name/offset property on MapObject yet - unwired, like Segment's slot 2.</summary>
        protected virtual void LoadSlot(NameTextureSlot slot, MapObject copy)
        {
            slot.NameDropdown.SetValueWithoutNotify(string.Empty);
            slot.OffsetStepper.Value = Vector2.zero;
        }

        /// <summary>No-op until MapObject exposes a texture-pairing name property.</summary>
        protected virtual void WriteBackSlot(MapObject target, NameTextureSlot slot)
        {
        }
    }
}
