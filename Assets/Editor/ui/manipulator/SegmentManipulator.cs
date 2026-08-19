using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Segment tab: Vertices, Left/Right Region and Length are locked (shown
    /// read-only, same fields/naming as InfoPanel). Editable: one or two
    /// Name+Texture-Offset+Texture "slots" (see NameTextureSlot) - Name first,
    /// then Texture Offset, then the Texture card.
    ///
    /// Standard case is a single slot. Slot 2 becomes visible once a second
    /// name provider is supplied via SetProviders() - side by side with the
    /// first (manip-slots-row). It's browsable at that point, but not yet
    /// wired to any Segment data (see TODO below) since there's no confirmed
    /// second-name/second-offset property on Segment to read/write.
    ///
    /// Vertex1/Vertex2 are read-only on Segment - only settable through its
    /// constructor, so Clone() rebuilds via `new Segment(vertex1, vertex2)`
    /// rather than an object initializer. LeftRegion/RightRegion aren't part
    /// of that constructor either, so their display reads from OriginalTarget
    /// (the real object), not the local edit copy - see ManipulatorWindowBase.
    ///
    /// Contour is intentionally not shown - internal-only, not meant for the user.
    /// Texture itself has no editable picker - Texture Name is a read-only
    /// label (like InfoPanel), only ever set via the "..." Select button
    /// (currently a placeholder, not implemented).
    ///
    /// TODO: Length isn't on Segment yet ("will soon have" it, read-only) -
    /// the row exists already but always shows "-" until it does.
    /// TODO: once Segment exposes a second name/offset/texture, mirror slot
    /// 1's wiring in LoadValues/WriteBack for slot 2 (currently nothing typed
    /// into slot 2 persists on Apply, even while it's visible/browsable).
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

        Segment m_current;

        public SegmentManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>
        /// Providers aren't necessarily ready when the window itself is
        /// constructed (e.g. built during static setup, before your name/texture
        /// registries exist) - call this once they are, any time before Open().
        ///
        /// The second pair is optional and fills the already-built (but hidden)
        /// slot 2 - passing either one makes slot 2 visible on the next Open().
        /// Slot 2 is still not wired to any Segment data (see class TODO), so
        /// it's browsable but nothing typed into it persists on Apply.
        /// </summary>
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
            WireSlot(m_slot1);
            row.Add(m_slot1);

            // Slot 2: built so the side-by-side layout already works once real
            // data exists, but hidden until a second provider is supplied -
            // see SetProviders() and the class-level TODO.
            m_slot2 = new NameTextureSlot();
            WireSlot(m_slot2);
            m_slot2.AddToClassList("hidden");
            row.Add(m_slot2);

            container.Add(row);
        }

        void WireSlot(NameTextureSlot slot)
        {
            slot.NameDropdown.RegisterValueChangedCallback(evt =>
            {
                if (slot == m_slot1 && m_current != null)
                    m_current.Name = evt.newValue;
                // slot2 has nothing to write to yet (see class TODO).
            });

            slot.NameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNewName(slot);
                    evt.StopPropagation(); // don't also trigger the window's own OK
                }
            });

            slot.OffsetStepper.ValueChanged += v =>
            {
                if (slot == m_slot1 && m_current != null)
                    m_current.Offset = v;
                // slot2 has nothing to write to yet (see class TODO).
            };

            slot.TextureSelectButton.clicked += () =>
            {
                var provider = TextureProviderFor(slot);
                var names = provider != null ? provider.GetTextureNames() : (IReadOnlyList<string>)new List<string>();
                Debug.Log($"TODO: open texture selection menu (not implemented yet). Available: {string.Join(", ", names)}");
            };
        }

        void CommitNewName(NameTextureSlot slot)
        {
            string sanitized = NameSanitizer.Sanitize(slot.NameNewEntry.value);
            slot.NameNewEntry.style.display = DisplayStyle.None;

            if (string.IsNullOrEmpty(sanitized))
                return;

            var provider = NameProviderFor(slot);
            if (provider != null && provider.TryCreateName(sanitized))
            {
                RefreshNameChoices(slot);
                slot.NameDropdown.value = sanitized;
            }
        }

        INameProvider NameProviderFor(NameTextureSlot slot) => slot == m_slot1 ? m_nameProvider : m_nameProvider2;
        ITextureProvider TextureProviderFor(NameTextureSlot slot) => slot == m_slot1 ? m_textureProvider : m_textureProvider2;

        // Choice assignment is deferred one frame via schedule.Execute(): a
        // DropdownField's popup measures its row heights against the current
        // layout. Populating "choices" in the very same frame the panel just
        // became display:Flex can race UI Toolkit's (also deferred) layout
        // pass, leaving stale/blank rows in the popup until something else
        // forces a relayout (e.g. a manual scroll). Deferring guarantees a
        // real layout has happened first.
        void RefreshNameChoices(NameTextureSlot slot)
        {
            var provider = NameProviderFor(slot);
            if (provider == null) return;
            var names = new List<string>(provider.GetNames());
            slot.schedule.Execute(() => slot.SetNameChoices(names));
        }

        protected override Segment Clone(Segment source)
        {
            // Vertex1/Vertex2 are read-only - only settable through this
            // constructor, not via object-initializer assignment.
            return new Segment(source.Vertex1, source.Vertex2)
            {
                Name = source.Name,
                Offset = source.Offset
                // LeftRegion / RightRegion intentionally not copied - read-only,
                // displayed straight from OriginalTarget in LoadValues, not
                // from this copy.
            };
        }

        protected override void LoadValues(Segment copy)
        {
            m_current = copy;

            m_vertex1Value.text = FormatVertex(copy.Vertex1);
            m_vertex2Value.text = FormatVertex(copy.Vertex2);
            m_regionLeftValue.text = FormatRegionRef(OriginalTarget.Left);
            m_regionRightValue.text = FormatRegionRef(OriginalTarget.Right);
            m_lengthValue.text = FormatSegmentLength(OriginalTarget.Length);

            RefreshNameChoices(m_slot1);
            m_slot1.NameDropdown.SetValueWithoutNotify(copy.Name);
            m_slot1.NameNewEntry.style.display = DisplayStyle.None;

            m_slot1.OffsetStepper.Step = CurrentLinearStep;
            m_slot1.OffsetStepper.Value = copy.Offset;

            m_slot1.TextureHintValue.text = "Hint";
            m_slot1.TextureNameValue.text = "-"; // informational only, not wired yet
            m_slot1.ScaleValue.text = "Scale X/Y: -";

            // Slot 2 becomes visible once a second name provider was supplied
            // via SetProviders() - browsable at that point, but still not
            // wired to any Segment data (see class TODO).
            bool hasSecondSlot = m_nameProvider2 != null || m_textureProvider2 != null;
            if (hasSecondSlot) m_slot2.RemoveFromClassList("hidden");
            else m_slot2.AddToClassList("hidden");

            RefreshNameChoices(m_slot2);
            m_slot2.NameDropdown.SetValueWithoutNotify(string.Empty);
            m_slot2.NameNewEntry.style.display = DisplayStyle.None;

            m_slot2.OffsetStepper.Step = CurrentLinearStep;
            m_slot2.OffsetStepper.Value = Vector2.zero;

            m_slot2.TextureHintValue.text = "Hint";
            m_slot2.TextureNameValue.text = "-";
            m_slot2.ScaleValue.text = "Scale X/Y: -";
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
            target.Name = editedCopy.Name;
            target.Offset = editedCopy.Offset;
            // Vertices, Left/Right Region and Length are read-only - left untouched.
            // Slot 2 has nothing to write yet (see class TODO).
        }
    }
}
