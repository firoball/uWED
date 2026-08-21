using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Segment tab. See SegmentManipulator.md for extension points.
    /// </summary>
    public class SegmentManipulator : ManipulatorWindowBase<Segment>
    {
        protected override string TypeLabel => "Segment";
        protected override bool UsesAngleStep => false;

        INameProvider m_nameProvider;
        ITextureProvider m_textureProvider;
        INameProvider m_nameProvider2;
        ITextureProvider m_textureProvider2;

        Label m_vertex1Value;
        Label m_vertex2Value;
        Label m_regionLeftValue;
        Label m_regionRightValue;
        Label m_lengthValue;

        NameTextureSlot m_slot1;
        NameTextureSlot m_slot2;

        public SegmentManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once providers are ready (not necessarily at construction).
        /// Second pair is optional and makes slot 2 visible.</summary>
        public void SetProviders(INameProvider nameProvider, ITextureProvider textureProvider,
            INameProvider nameProvider2 = null, ITextureProvider textureProvider2 = null)
        {
            m_nameProvider = nameProvider;
            m_textureProvider = textureProvider;
            m_nameProvider2 = nameProvider2;
            m_textureProvider2 = textureProvider2;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildSlots(container);
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

        void BuildSlots(VisualElement container)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-slots-row");

            m_slot1 = new NameTextureSlot();
            m_slot1.AddToClassList("manip-slot-first");
            WireSlot(m_slot1);
            row.Add(m_slot1);

            m_slot2 = new NameTextureSlot();
            WireSlot(m_slot2);
            m_slot2.AddToClassList("hidden");
            row.Add(m_slot2);

            container.Add(row);
        }

        void WireSlot(NameTextureSlot slot)
        {
            slot.NameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNewName(slot);
                    evt.StopPropagation();
                }
            });

            slot.TextureSelectButton.clicked += () =>
            {
                var provider = TextureProviderFor(slot);
                var names = provider != null ? provider.GetTextureNames() : (IReadOnlyList<string>)new List<string>();
                Debug.Log($"TODO: open texture selection menu. Available: {string.Join(", ", names)}");
            };
        }

        void CommitNewName(NameTextureSlot slot)
        {
            string sanitized = NameSanitizer.Sanitize(slot.NameNewEntry.value);
            slot.NameNewEntry.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(sanitized)) return;

            var provider = NameProviderFor(slot);
            if (provider != null && provider.TryCreateName(sanitized))
            {
                RefreshNameChoices(slot);
                slot.NameDropdown.value = sanitized;
            }
        }

        INameProvider NameProviderFor(NameTextureSlot slot) => slot == m_slot1 ? m_nameProvider : m_nameProvider2;
        ITextureProvider TextureProviderFor(NameTextureSlot slot) => slot == m_slot1 ? m_textureProvider : m_textureProvider2;

        // schedule.Execute defers one frame - avoids a DropdownField popup
        // measuring against a not-yet-laid-out panel (blank rows until scrolled).
        void RefreshNameChoices(NameTextureSlot slot)
        {
            var provider = NameProviderFor(slot);
            if (provider == null) return;
            var names = new List<string>(provider.GetNames());
            slot.schedule.Execute(() => slot.SetNameChoices(names));
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

            RefreshNameChoices(m_slot1);
            m_slot1.NameNewEntry.style.display = DisplayStyle.None;
            m_slot1.OffsetStepper.Step = CurrentLinearStep;
            LoadTextureInfo(m_slot1, copy);
            LoadSlot1(m_slot1, copy);

            bool hasSecondSlot = m_nameProvider2 != null || m_textureProvider2 != null;
            if (hasSecondSlot) m_slot2.RemoveFromClassList("hidden");
            else m_slot2.AddToClassList("hidden");

            RefreshNameChoices(m_slot2);
            m_slot2.NameNewEntry.style.display = DisplayStyle.None;
            m_slot2.OffsetStepper.Step = CurrentLinearStep;
            LoadTextureInfo(m_slot2, copy);
            LoadSlot2(m_slot2, copy);
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
            WriteBackSlot1(target, m_slot1);
            WriteBackSlot2(target, m_slot2);
        }

        // ---- Extension points - see SegmentManipulator.md ----

        /// <summary>Texture Name/Scale for a slot. Override once real texture data exists.</summary>
        protected virtual void LoadTextureInfo(NameTextureSlot slot, Segment copy)
        {
            slot.TextureHintValue.text = "Hint";
            slot.TextureNameValue.text = "-";
            slot.ScaleValue.text = "Scale X/Y: -";
        }

        /// <summary>Slot 1's Name/Offset display. Override to change what backs it.</summary>
        protected virtual void LoadSlot1(NameTextureSlot slot1, Segment copy)
        {
            slot1.NameDropdown.SetValueWithoutNotify(copy.Name);
            slot1.OffsetStepper.Value = copy.Offset;
        }

        /// <summary>Write slot 1 back onto Segment.Name/Offset. Override to change what it writes to.</summary>
        protected virtual void WriteBackSlot1(Segment target, NameTextureSlot slot1)
        {
            target.Name = slot1.NameDropdown.value;
            target.Offset = slot1.OffsetStepper.Value;
        }

        /// <summary>Slot 2's Name/Offset display. Override once Segment exposes a second one.</summary>
        protected virtual void LoadSlot2(NameTextureSlot slot2, Segment copy)
        {
            slot2.NameDropdown.SetValueWithoutNotify(string.Empty);
            slot2.OffsetStepper.Value = Vector2.zero;
        }

        /// <summary>Write slot 2 back. No-op until Segment exposes a second Name/Offset.</summary>
        protected virtual void WriteBackSlot2(Segment target, NameTextureSlot slot2)
        {
        }
    }
}
