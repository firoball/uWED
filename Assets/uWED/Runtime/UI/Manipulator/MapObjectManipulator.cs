using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Controls;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// MapObject tab. Position is a Vertex (X/Y readonly, double), shown
    /// Vertex-style; Z is editable the same way VertexManipulator edits Z -
    /// MapObject's ctor only takes a Vector2, so Clone() can't carry Z through
    /// the constructor, and it's restored from OriginalTarget after cloning.
    /// Angle is editable via a NumberStepperField snapped to the shared Angle
    /// Step. Region is read-only ("{Name} #{Index}"). Name is a plain rename
    /// field (ComboBoxField, string-typed, via IGenericNameProvider&lt;string&gt;) -
    /// same shape as Region/Way/Segment's Name fields. One NameTextureSlot for
    /// texture display only (Offset section hidden - no confirmed per-object
    /// offset property).
    ///
    /// See docs/MapObjectManipulator.md for the variant/template picker
    /// extension pattern (not base-class behavior).
    /// </summary>
    public class MapObjectManipulator : ManipulatorWindowBase<MapObject>
    {
        protected override string TypeLabel => "Object";

        IGenericNameProvider<string> m_nameProvider = new SimpleGenericNameProvider(new List<string>());
        ITextureProvider m_textureProvider;

        Label m_posXValue;
        Label m_posYValue;
        NumberStepperField m_zStepper;
        Label m_regionValue;
        NumberStepperField m_angleStepper;

        VisualElement m_nameFieldContainer;
        ComboBoxField m_nameCombo;

        NameTextureSlot m_slot;

        MapObject m_current;

        public MapObjectManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
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
        /// docs/MapObjectManipulator.md).</summary>
        protected void SetNameFieldVisible(bool visible)
        {
            m_nameFieldContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildZField(container);
            BuildAngleField(container);
            BuildNameField(container);
            BuildSlot(container);
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
            m_slot.SetOffsetSectionVisible(false); // no confirmed per-object offset property

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

            m_nameCombo.Refresh(); // in case Choices was mutated externally since the last Open()
            m_nameCombo.SetValueWithoutNotify(copy.Name);

            LoadTextureInfo(m_slot, copy);
        }

        static string FormatRegionRef(Region region)
        {
            return region == null ? "-" : $"{region.Name} #{region.Index}";
        }

        protected override void WriteBack(MapObject target, MapObject editedCopy)
        {
            target.Position.Z = editedCopy.Position.Z;
            target.Angle = m_angleStepper.Value;
            target.Name = m_nameCombo.value;
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
