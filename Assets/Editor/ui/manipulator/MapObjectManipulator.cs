using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// MapObject tab. Position is a Vertex (X/Y readonly, double), shown
    /// Vertex-style; Z is editable the same way VertexManipulator edits Z -
    /// MapObject's ctor only takes a Vector2, so Clone() can't carry Z through
    /// the constructor, and it's restored from OriginalTarget after cloning.
    /// Angle is editable via a NumberStepperField snapped to the shared Angle
    /// Step. Region is read-only ("{Name} #{Index}"). Standalone Name picker
    /// (the ONE MapObject.Name - separate from the texture slot). One
    /// NameTextureSlot for texture display only - Name/Offset sections hidden
    /// (texture unrelated to MapObject.Name, no confirmed per-object offset).
    /// </summary>
    public class MapObjectManipulator : ManipulatorWindowBase<MapObject>
    {
        protected override string TypeLabel => "Object";

        INameProvider m_objectNameProvider;
        ITextureProvider m_textureProvider;

        Label m_posXValue;
        Label m_posYValue;
        NumberStepperField m_zStepper;
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
        /// textureProvider backs the texture slot's "..." select (placeholder).</summary>
        public void SetProviders(INameProvider objectNameProvider, ITextureProvider textureProvider)
        {
            m_objectNameProvider = objectNameProvider;
            m_textureProvider = textureProvider;
            RefreshObjectNameChoices();
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildZField(container);
            BuildAngleField(container);
            BuildNameField(container);
            BuildSlot(container);
        }

        void BuildZField(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-field-row");

            var label = new Label("Z");
            label.AddToClassList("manip-field-label");
            row.Add(label);

            m_zStepper = new NumberStepperField { Step = CurrentLinearStep };
            m_zStepper.ValueChanged += v =>
            {
                if (m_current != null)
                    m_current.Position.Z = v;
            };
            row.Add(m_zStepper);

            container.Add(row);
        }

        // MapObject's own identity Name - standalone, separate from the
        // texture slot's (hidden) Name dropdown.
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

            m_objectNameProvider?.TryCreateName(sanitized);
            RefreshObjectNameChoices();
            m_nameDropdown.value = sanitized; // always applied, regardless of provider outcome
        }

        void RefreshObjectNameChoices()
        {
            if (m_objectNameProvider == null || m_nameDropdown == null) return;
            var names = new List<string>(m_objectNameProvider.GetNames());
            string currentValue = m_nameDropdown.value;
            // choices + value set together in the same deferred frame - setting
            // value immediately then choices a frame later (previous version) let
            // the deferred choices assignment clobber the just-set value.
            m_nameDropdown.schedule.Execute(() =>
            {
                m_nameDropdown.choices = names;
                m_nameDropdown.SetValueWithoutNotify(currentValue);
            });
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

            m_slot.SetNameSectionVisible(false);
            m_slot.SetOffsetSectionVisible(false);

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

        protected override MapObject Clone(MapObject source)
        {
            // Ctor only takes a Vector2 - Position.Z can't be carried through
            // construction, restored manually in LoadValues from OriginalTarget.
            return new MapObject(new Vector2((float)source.Position.X, (float)source.Position.Y), source.Angle, source.Region, source.Name);
        }

        protected override void LoadValues(MapObject copy)
        {
            m_current = copy;
            copy.Position.Z = OriginalTarget.Position.Z; // restore what Clone() couldn't carry through the ctor

            m_posXValue.text = copy.Position.X.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            m_posYValue.text = copy.Position.Y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            m_regionValue.text = FormatRegionRef(OriginalTarget.Region);

            m_zStepper.Step = CurrentLinearStep;
            m_zStepper.Value = (float)copy.Position.Z;

            m_angleStepper.Step = CurrentAngleStep;
            m_angleStepper.Value = copy.Angle;

            m_nameDropdown.SetValueWithoutNotify(copy.Name);
            m_nameNewEntry.style.display = DisplayStyle.None;
            RefreshObjectNameChoices(); // captures the just-set value, applies it after deferred choices assignment

            m_slot.OffsetStepper.Value = Vector2.zero;
            LoadTextureInfo(m_slot, copy);
        }
        static string FormatRegionRef(Region region)
        {
            return region == null ? "-" : $"{region.Name} #{region.Index}";
        }

        protected override void WriteBack(MapObject target, MapObject editedCopy)
        {
            CommitPendingObjectNameIfAny();
            target.Position.Z = editedCopy.Position.Z;
            target.Angle = m_angleStepper.Value;
            target.Name = m_nameDropdown.value;
        }

        void CommitPendingObjectNameIfAny()
        {
            if (m_nameNewEntry.style.display == DisplayStyle.Flex && !string.IsNullOrEmpty(m_nameNewEntry.value))
                CommitNewObjectName();
        }

        // ---- Extension point ----

        /// <summary>Texture Name/Scale for the slot (placeholder - textures do have names,
        /// just no real data wired yet). Override once real texture data exists.</summary>
        protected virtual void LoadTextureInfo(NameTextureSlot slot, MapObject copy)
        {
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }
    }
}
