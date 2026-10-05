using UI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Region's RegionManipulator extension - hides the base Name field and replaces it with a
    /// GenericComboBoxField&lt;RegionTemplate&gt; Template picker in the same spot, the same pattern
    /// WallManipulator follows for Segment. Region itself carries no Template reference at all - the map
    /// (WMP) only ever holds plain Regions, and the link to a RegionTemplate is purely by matching Name,
    /// resolved (creating a default if nothing matches yet) via TemplateResolver&lt;RegionTemplate&gt;. No
    /// Clone override is needed - the base RegionManipulator.Clone is already correct, since there's no
    /// extra field to carry over.
    /// </summary>
    public class AcknexRegionManipulator : RegionManipulator
    {
        readonly TemplateRegistry<RegionTemplate> m_templateRegistry;
        readonly TemplateResolver<RegionTemplate> m_templateResolver;
        GenericComboBoxField<RegionTemplate> m_templateCombo;

        // Resolved once per Open() in LoadValues, valid until the next LoadValues/WriteBack - avoids a
        // second by-name lookup in WriteBack and lets LoadTextureInfo (called from inside
        // base.LoadValues, before this override's own code runs) see it already resolved.
        RegionTemplate m_currentTemplate;

        /// <summary>Builds the window against the given collaborators - Choices and Clone (the picker's
        /// "+") read from templateRegistry directly; Segment/Region→Template resolution (including the
        /// no-match-yet default policy) goes through templateResolver instead, which wraps that same
        /// registry.</summary>
        public AcknexRegionManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings, TemplateRegistry<RegionTemplate> templateRegistry, TemplateResolver<RegionTemplate> templateResolver)
            : base(baseUxml, settings)
        {
            m_templateRegistry = templateRegistry;
            m_templateResolver = templateResolver;
            AcknexManipulatorStyles.ApplyTo(this);
            SetNameFieldVisible(false);
            BuildTemplatePicker();
        }

        void BuildTemplatePicker()
        {
            var section = new VisualElement();

            var title = new Label("Template");
            title.AddToClassList("manip-section-title");
            section.Add(title);

            m_templateCombo = new GenericComboBoxField<RegionTemplate>
            {
                // Choices/ItemFactory/Sanitizer read straight off the registry, which implements
                // IGenericNameProvider<RegionTemplate> for exactly this purpose - the registry's
                // ItemFactory stays in-memory only (see TemplateRegistry.BuildTransientCandidate); the real
                // persist-on-Add is committed separately in OnTemplateComboValueChanged via
                // TemplateRegistry.Clone.
                Choices = m_templateRegistry.Choices,
                ItemFactory = m_templateRegistry.ItemFactory,
                Sanitizer = m_templateRegistry.Sanitizer,
                AllowDetailMode = true,
                DetailViewBuilder = BuildDetailView,
                VisibleRowCount = 6,
                DetailVisibleRowCount = 3,
                // A RegionTemplate can be shared by many Regions (resolved by Name), and deletion needs a
                // deliberate design for that - not implemented, so the picker's own remove action stays off.
                AllowDelete = false,
            };
            m_templateCombo.RegisterValueChangedCallback(OnTemplateComboValueChanged);
            m_templateCombo.AddToClassList("manip-picker-dropdown");
            section.Add(m_templateCombo);

            // "manip-content-container" is the base window's own content root (see
            // ManipulatorWindowBase), queried by name rather than through a RegionManipulator field since
            // the base class exposes no protected access to it. Inserted at index 1 - the slot the
            // now-hidden Name field occupies (index 0 is the read-only Min/Max block).
            var content = this.Q<VisualElement>("manip-content-container");
            content.Insert(1, section);
        }

        /// <summary>Template Name on its own top row (action icon vertically centered beside it), then a
        /// content row with both Floor/Ceiling thumbnails side by side followed by their compact info
        /// lines stacked to the right, each prefixed "Floor"/"Ceil" - the prefix is what tells the two
        /// stat lines apart here, since (unlike WallManipulator's single-Texture row) there isn't a
        /// separate row per thumbnail. Same shared layout language as WallManipulator.BuildDetailView
        /// otherwise (see tasks.md's "Decided" note on this layout).</summary>
        static VisualElement BuildDetailView(RegionTemplate template)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            row.AddToClassList(AcknexManipulatorStyles.PickerDetailRow);

            var stack = new VisualElement { style = { flexDirection = FlexDirection.Column, flexGrow = 1 } };

            var nameLabel = new Label(template.Name) { style = { unityTextAlign = TextAnchor.MiddleLeft } };
            nameLabel.AddToClassList(AcknexManipulatorStyles.PickerTemplateName);
            stack.Add(nameLabel);

            var contentRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 2 } };
            contentRow.Add(TextureReferenceDetailView.BuildThumbnail(template.Floor_tex, size: 40));
            contentRow.Add(TextureReferenceDetailView.BuildThumbnail(template.Ceil_tex, size: 40));

            var statColumn = new VisualElement { style = { flexDirection = FlexDirection.Column, marginLeft = 6 } };
            // Heights shown are the Template's own defaults (Floor_hgt/Ceil_hgt), not any individual
            // Region's current value.
            var floorInfoLabel = TextureReferenceDetailView.BuildCompactInfoLabel("Floor", template.Floor_tex, $"Hgt {template.Floor_hgt:0.###}");
            floorInfoLabel.AddToClassList(AcknexManipulatorStyles.PickerTextureInfo);
            statColumn.Add(floorInfoLabel);
            var ceilInfoLabel = TextureReferenceDetailView.BuildCompactInfoLabel("Ceil", template.Ceil_tex, $"Hgt {template.Ceil_hgt:0.###}");
            ceilInfoLabel.AddToClassList(AcknexManipulatorStyles.PickerTextureInfo);
            statColumn.Add(ceilInfoLabel);
            contentRow.Add(statColumn);

            stack.Add(contentRow);
            row.Add(stack);

            var icons = BuildIndicatorIcons(template);
            if (icons != null)
                row.Add(icons);

            return row;
        }

        /// <summary>Horizontal icon list for a detail row, vertically centered against the whole row and
        /// growing rightward as more indicators apply: lightning when any WDL callback is set (see
        /// RegionTemplate.HasActionProperties), stacked when Below is assigned (see
        /// RegionTemplate.IsStacked). Null when neither applies. Explicit alignSelf/unityTextAlign on each
        /// glyph, rather than relying on the parent's alignItems, keeps it centered regardless of how tall
        /// the container ends up next to the Name label's shorter line height.</summary>
        static VisualElement BuildIndicatorIcons(RegionTemplate template)
        {
            if (!template.HasActionProperties && !template.IsStacked)
                return null;

            var icons = new VisualElement();
            icons.AddToClassList(AcknexManipulatorStyles.PickerIconRow);

            if (template.HasActionProperties)
                icons.Add(BuildIcon("⚡", AcknexManipulatorStyles.PickerActionIcon, "Has one or more WDL action callbacks set"));
            if (template.IsStacked)
                icons.Add(BuildIcon("≡", AcknexManipulatorStyles.PickerStackedIcon, $"Stacked: Below is '{template.Below.Name}'"));

            return icons;
        }

        static Label BuildIcon(string glyph, string className, string tooltip)
        {
            var icon = new Label(glyph) { style = { alignSelf = Align.Center, unityTextAlign = TextAnchor.MiddleCenter } };
            icon.AddToClassList(className);
            icon.tooltip = tooltip;
            return icon;
        }

        /// <summary>Distinguishes an ordinary row selection (already-registered Template) from a
        /// confirmed "+" (a candidate not yet in TemplateRegistry.ByName), and for the latter, persists
        /// and registers it via TemplateRegistry.Clone - sourced from m_currentTemplate, not
        /// evt.previousValue, which does not reliably reflect the Template LoadValues resolved.</summary>
        void OnTemplateComboValueChanged(ChangeEvent<RegionTemplate> evt)
        {
            var newValue = evt.newValue;
            if (newValue == null || m_templateRegistry.ByName.ContainsKey(newValue.Name))
            {
                m_currentTemplate = newValue;
                return;
            }

            m_templateRegistry.Choices.Remove(newValue);
            var persisted = m_templateRegistry.Clone(m_currentTemplate, newValue.Name);

            m_currentTemplate = persisted;
            m_templateCombo.SetValueWithoutNotify(persisted);
            m_templateCombo.Refresh();
        }

        protected override void LoadValues(Region copy)
        {
            bool wasUnnamed = string.IsNullOrEmpty(copy.Name);

            // Resolved before base.LoadValues(copy), which calls LoadTextureInfo internally and needs
            // m_currentTemplate already set.
            m_currentTemplate = m_templateResolver.Resolve(copy.Name);

            base.LoadValues(copy);

            m_templateCombo.Refresh();
            m_templateCombo.SetValueWithoutNotify(m_currentTemplate);

            // An unnamed Region has no meaningful state on its own - it only makes sense tied to a
            // Template - so the resolved default is committed immediately rather than left pending until
            // the user happens to press OK.
            if (wasUnnamed)
                ApplyNow();
        }

        protected override void WriteBack(Region target, Region editedCopy)
        {
            base.WriteBack(target, editedCopy); // sets target.Name from the (hidden, untouched) base Name field

            if (m_templateCombo.value != null)
                target.Name = m_templateCombo.value.Name; // Name is derived from the selected Template, overriding whatever base just set
        }

        /// <summary>Shows the resolved Template's Floor/Ceiling Texture (matching isFloor) in the given
        /// slot, in place of the base class's permanent placeholder text. Called twice per LoadValues, once
        /// per slot - isFloor is the base class's own way of telling the two calls apart.</summary>
        protected override void LoadTextureInfo(NameTextureSlot slot, Region copy, bool isFloor)
        {
            slot.TextureHintValue.text = isFloor ? "Floor" : "Ceiling";

            var texture = isFloor ? m_currentTemplate?.Floor_tex : m_currentTemplate?.Ceil_tex;
            slot.TextureNameValue.text = texture?.Name ?? "-";
            slot.ScaleValue.text = texture?.Template != null
                ? $"Scale X/Y: {texture.Template.Scale_x:0.###} / {texture.Template.Scale_y:0.###}"
                : "Scale X/Y: -";
        }

        /// <summary>nameProvider.Choices carries every Region's current Name in the map, handed fresh on
        /// every Manipulator open regardless of which Region - so the first open after a map load already
        /// sees the full list, and resolving all of them up front means every Region's RegionTemplate
        /// exists (falling back to the shared default where nothing matches yet) before its own Manipulator
        /// is ever opened, not just the one Region being edited right now. Unrelated to this class's own
        /// IGenericNameProvider&lt;RegionTemplate&gt; usage in BuildTemplatePicker - this nameProvider is
        /// over plain Region Names (T = string, fixed by the base class), never over RegionTemplate
        /// itself.</summary>
        public override void SetProviders(IGenericNameProvider<string> nameProvider, ITextureProvider textureProvider)
        {
            base.SetProviders(nameProvider, textureProvider);
            m_templateResolver.ResolveAll(nameProvider.Choices);
        }
    }
}
