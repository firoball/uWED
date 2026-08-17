using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Segment tab: Vertices and Left/Right Region are locked (shown read-only,
    /// same fields/naming as InfoPanel). Editable: Name, Offset (Vector2, one
    /// Vector2StepperField with per-axis Auto Align toggles), and a texture
    /// placeholder.
    ///
    /// Vertex1/Vertex2 are read-only on Segment - only settable through its
    /// constructor, so Clone() rebuilds via `new Segment(vertex1, vertex2)`
    /// rather than an object initializer.
    ///
    /// Auto Align does not persist anything yet - the actual offset auto-calc
    /// routine will be closely tied to textures and isn't available yet. While
    /// checked, that axis is simply left untouched on Apply.
    ///
    /// Contour is intentionally not shown - internal-only, not meant for the user.
    ///
    /// TODO: LeftRegion/RightRegion property names below are still a guess as to
    /// exact casing/type - adjust to match InfoPanel's actual Segment binding.
    /// </summary>
    public class SegmentManipulator : ManipulatorWindowBase<Segment>
    {
        protected override string TypeLabel => "Segment";
        protected override bool UsesAngleStep => false;

        INameProvider m_nameProvider;
        ITextureProvider m_textureProvider;

        Label m_vertex1Value;
        Label m_vertex2Value;
        Label m_regionLeftValue;
        Label m_regionRightValue;


        DropdownField m_nameDropdown;
        Button m_nameNewButton;
        TextField m_nameNewEntry;

        Vector2StepperField m_offsetStepper;

        VisualElement m_textureSlot1;
        VisualElement m_textureSlot2;
        DropdownField m_texture1Dropdown;
        Button m_texture1NewButton;
        TextField m_texture1NewEntry;
        Label m_texture1ScaleValue;

        Segment m_current;

        public SegmentManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>
        /// Providers aren't necessarily ready when the window itself is
        /// constructed (e.g. built during static setup, before your name/texture
        /// registries exist) - call this once they are, any time before Open().
        /// </summary>
        public void SetProviders(INameProvider nameProvider, ITextureProvider textureProvider)
        {
            m_nameProvider = nameProvider;
            m_textureProvider = textureProvider;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildReadonlyBlock(container);
            BuildNameRow(container);
            BuildOffsetRows(container);
            BuildTextureSlots(container);
        }

        void BuildReadonlyBlock(VisualElement container)
        {
            var block = new VisualElement();
            block.AddToClassList("manip-readonly-block");

            m_vertex1Value = AddReadonlyRow(block, "Vertex 1");
            m_vertex2Value = AddReadonlyRow(block, "Vertex 2");
            m_regionLeftValue = AddReadonlyRow(block, "Left Region");
            m_regionRightValue = AddReadonlyRow(block, "Right Region");

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

        void BuildNameRow(VisualElement container)
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
            m_nameDropdown.RegisterValueChangedCallback(evt =>
            {
                if (m_current != null)
                    m_current.Name = evt.newValue;
            });
            row.Add(m_nameDropdown);

            m_nameNewButton = new Button(ShowNameNewEntry) { text = "+" };
            m_nameNewButton.AddToClassList("manip-picker-new-button");
            row.Add(m_nameNewButton);

            container.Add(row);

            m_nameNewEntry = new TextField { isDelayed = false };
            m_nameNewEntry.AddToClassList("manip-picker-new-entry");
            m_nameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == UnityEngine.KeyCode.Return || evt.keyCode == UnityEngine.KeyCode.KeypadEnter)
                {
                    CommitNewName();
                    evt.StopPropagation(); // don't also trigger the window's own OK
                }
            });
            container.Add(m_nameNewEntry);
        }

        static string DisplayLowercase(string value) => string.IsNullOrEmpty(value) ? value : value.ToLowerInvariant();

        void ShowNameNewEntry()
        {
            m_nameNewEntry.style.display = DisplayStyle.Flex;
            m_nameNewEntry.SetValueWithoutNotify(string.Empty);
            m_nameNewEntry.Focus();
        }

        void CommitNewName()
        {
            string sanitized = NameSanitizer.Sanitize(m_nameNewEntry.value);
            m_nameNewEntry.style.display = DisplayStyle.None;

            if (string.IsNullOrEmpty(sanitized))
                return;

            if (m_nameProvider != null && m_nameProvider.TryCreateName(sanitized))
            {
                RefreshNameChoices();
                m_nameDropdown.value = sanitized;
            }
        }

        void RefreshNameChoices()
        {
            if (m_nameProvider == null) return;
            var names = new List<string>(m_nameProvider.GetNames());
            m_nameDropdown.choices = names;
        }

        void BuildOffsetRows(VisualElement container)
        {
            var title = new Label("Offset");
            title.AddToClassList("manip-section-title");
            container.Add(title);

            // One widget handles both axes (Segment.Offset is a single Vector2)
            // with each Auto Align toggle built in right next to its own axis -
            // no extra row label needed, the section title above already says
            // "Offset".
            m_offsetStepper = new Vector2StepperField(showAutoAlignToggles: true) { Step = CurrentLinearStep };
            m_offsetStepper.ValueChanged += v =>
            {
                if (m_current != null) m_current.Offset = v;
            };
            container.Add(m_offsetStepper);
        }

        void BuildTextureSlots(VisualElement container)
        {
            var title = new Label("Texture");
            title.AddToClassList("manip-section-title");
            container.Add(title);

            m_textureSlot1 = BuildTextureSlot("Texture", out m_texture1Dropdown, out m_texture1NewButton,
                out m_texture1NewEntry, out m_texture1ScaleValue);
            container.Add(m_textureSlot1);

            // Second slot - same layout, hidden until a second texture actually
            // exists (Segment currently only has one texture slot in the data).
            m_textureSlot2 = BuildTextureSlot("Texture 2", out _, out _, out _, out _);
            m_textureSlot2.AddToClassList("hidden");
            container.Add(m_textureSlot2);
        }

        VisualElement BuildTextureSlot(string label, out DropdownField dropdown, out Button newButton,
            out TextField newEntry, out Label scaleValue)
        {
            var slot = new VisualElement();
            slot.AddToClassList("manip-texture-slot");

            var preview = new VisualElement();
            preview.AddToClassList("manip-texture-preview");
            var previewLabel = new Label("No preview\n(placeholder)");
            previewLabel.AddToClassList("manip-texture-preview-label");
            preview.Add(previewLabel);
            slot.Add(preview);

            var info = new VisualElement();
            info.AddToClassList("manip-texture-info");

            var pickerRow = new VisualElement();
            pickerRow.AddToClassList("manip-picker-row");

            // Built as locals throughout (including inside the lambda below) -
            // 'out' parameters can't be captured by a lambda closure (CS1628),
            // so they're only assigned to dropdown/newButton/newEntry/scaleValue
            // at the very end, once we're done using them directly.
            var dropdownLocal = new DropdownField();
            dropdownLocal.AddToClassList("manip-picker-dropdown");
            dropdownLocal.formatListItemCallback = DisplayLowercase;
            dropdownLocal.formatSelectedValueCallback = DisplayLowercase;
            pickerRow.Add(dropdownLocal);

            var newButtonLocal = new Button { text = "+" };
            newButtonLocal.AddToClassList("manip-picker-new-button");
            pickerRow.Add(newButtonLocal);
            info.Add(pickerRow);

            var newEntryLocal = new TextField { isDelayed = false };
            newEntryLocal.AddToClassList("manip-picker-new-entry");
            info.Add(newEntryLocal);

            newButtonLocal.clicked += () =>
            {
                newEntryLocal.style.display = DisplayStyle.Flex;
                newEntryLocal.SetValueWithoutNotify(string.Empty);
                newEntryLocal.Focus();
            };

            var scaleLabelLocal = new Label("Scale X/Y: -");
            scaleLabelLocal.AddToClassList("manip-texture-info-label");
            info.Add(scaleLabelLocal);

            slot.Add(info);

            dropdown = dropdownLocal;
            newButton = newButtonLocal;
            newEntry = newEntryLocal;
            scaleValue = scaleLabelLocal;

            return slot;
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
                // displayed straight from the original source in LoadValues.
            };
        }

        protected override void LoadValues(Segment copy)
        {
            m_current = copy;

            m_vertex1Value.text = FormatVertex(copy.Vertex1);
            m_vertex2Value.text = FormatVertex(copy.Vertex2);
            // TODO: wire actual LeftRegion/RightRegion source once property names are confirmed.
            m_regionLeftValue.text = "-";
            m_regionRightValue.text = "-";

            RefreshNameChoices();
            m_nameDropdown.SetValueWithoutNotify(copy.Name);
            m_nameNewEntry.style.display = DisplayStyle.None;

            m_offsetStepper.Step = CurrentLinearStep;
            m_offsetStepper.Value = copy.Offset;
            m_offsetStepper.AutoAlignXToggle.SetValueWithoutNotify(false);
            m_offsetStepper.AutoAlignYToggle.SetValueWithoutNotify(false);
            m_offsetStepper.SetXEnabled(true);
            m_offsetStepper.SetYEnabled(true);

            m_texture1Dropdown.choices = m_textureProvider != null
                ? new List<string>(m_textureProvider.GetTextureNames())
                : new List<string>();
            m_texture1NewEntry.style.display = DisplayStyle.None;
            m_texture1ScaleValue.text = "Scale X/Y: -"; // informational only, not wired yet

            // Second texture slot stays hidden until Segment actually exposes one.
            m_textureSlot2.AddToClassList("hidden");
        }

        static string FormatVertex(Vertex v)
        {
            if (v == null) return "-";
            string x = v.X.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string y = v.Y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            return $"({x}, {y})";
        }

        protected override void WriteBack(Segment target, Segment editedCopy)
        {
            target.Name = editedCopy.Name;

            // Auto Align has no calculation behind it yet (future work, tied to
            // textures) - while checked there is no computed value to write, so
            // that axis is simply left as-is rather than overwritten with a
            // stale/manual one. Offset is a single Vector2 property, so a
            // partial (one-axis) update means reading the target's current
            // value first and only overwriting the axis that was actually edited.
            Vector2 result = target.Offset;
            if (!m_offsetStepper.AutoAlignXToggle.value)
                result.x = editedCopy.Offset.x;
            if (!m_offsetStepper.AutoAlignYToggle.value)
                result.y = editedCopy.Offset.y;
            target.Offset = result;

            // Vertices and Left/Right Region are read-only - left untouched.
        }
    }
}
