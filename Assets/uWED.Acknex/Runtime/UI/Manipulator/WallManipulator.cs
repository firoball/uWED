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
    /// Wall's SegmentManipulator extension - hides the base Name field and replaces it with a
    /// GenericComboBoxField&lt;WallTemplate&gt; Template picker in the same spot, the pattern every
    /// other Acknex-extended Manipulator (Thing/Actor/Region) follows with its own Template type.
    /// Segment itself carries no Template reference at all - the map (WMP) only ever holds plain
    /// Segments, and the link to a WallTemplate is purely by matching Name, resolved (creating a default
    /// if nothing matches yet) via TemplateResolver&lt;WallTemplate&gt;. No Clone override is needed - the
    /// base SegmentManipulator.Clone is already correct, since there's no extra field to carry over.
    /// </summary>
    public class WallManipulator : SegmentManipulator
    {
        readonly TemplateRegistry<WallTemplate> m_templateRegistry;
        readonly TemplateResolver<WallTemplate> m_templateResolver;
        GenericComboBoxField<WallTemplate> m_templateCombo;

        // Resolved once per Open() in LoadValues, valid until the next LoadValues/WriteBack - avoids a
        // second by-name lookup in WriteBack and lets LoadTextureInfo (called from inside
        // base.LoadValues, before this override's own code runs) see it already resolved.
        WallTemplate m_currentTemplate;

        /// <summary>Builds the window against the given collaborators - Choices and Clone (the picker's
        /// "+") read from templateRegistry directly; Segment→Template resolution (including the
        /// no-match-yet default policy) goes through templateResolver instead, which wraps that same
        /// registry.</summary>
        public WallManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings, TemplateRegistry<WallTemplate> templateRegistry, TemplateResolver<WallTemplate> templateResolver)
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

            m_templateCombo = new GenericComboBoxField<WallTemplate>
            {
                // Choices/ItemFactory/Sanitizer read straight off the registry, which implements
                // IGenericNameProvider<WallTemplate> for exactly this purpose - the registry's ItemFactory
                // stays in-memory only (see TemplateRegistry.BuildTransientCandidate); the real persist-on-Add
                // is committed separately in OnTemplateComboValueChanged via TemplateRegistry.Clone.
                Choices = m_templateRegistry.Choices,
                ItemFactory = m_templateRegistry.ItemFactory,
                Sanitizer = m_templateRegistry.Sanitizer,
                AllowDetailMode = true,
                DetailViewBuilder = BuildDetailView,
                VisibleRowCount = 6,
                // Detail rows are taller than brief-mode rows (thumbnail + info column), so a smaller cap
                // keeps the open popup a similar height to brief mode instead of growing tall.
                DetailVisibleRowCount = 3,
                // A WallTemplate can be shared by many Segments (resolved by Name), and deletion needs a
                // deliberate design for that - not implemented, so the picker's own remove action stays off.
                AllowDelete = false,
            };
            m_templateCombo.RegisterValueChangedCallback(OnTemplateComboValueChanged);
            m_templateCombo.AddToClassList("manip-picker-dropdown");
            section.Add(m_templateCombo);

            // "manip-content-container" is the base window's own content root (see
            // ManipulatorWindowBase), queried by name rather than through a SegmentManipulator field
            // since the base class exposes no protected access to it. Inserted at index 1 - the slot
            // the now-hidden Name field occupies (index 0 is the read-only Vertex/Region/Length block).
            var content = this.Q<VisualElement>("manip-content-container");
            content.Insert(1, section);
        }

        /// <summary>Template Name on its own top row (with the action icon vertically centered beside it,
        /// right-aligned via the Name label's flex-grow), then a second row with the thumbnail beside a
        /// single compact texture-info line. This is the shared layout language for every Template
        /// picker's detail view (Thing/Actor/Region will reuse the same shape) - Name on its own row rather
        /// than beside the thumbnail is what lets Region's floor+ceiling variant put two thumbnails on the
        /// content row without the Name needing to sit beside either one specifically. The Way picker is
        /// the one exception - Way has no Texture, so its detail view (once built) will be Name-only, no
        /// content row.</summary>
        static VisualElement BuildDetailView(WallTemplate template)
        {
            // Outer row is horizontal so the action icon sits beside the whole Name+content stack and can
            // be centered against its full height - centering it inside the (much shorter) Name row alone
            // left it looking pinned near the top instead of centered on the row as a whole.
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            row.AddToClassList(AcknexManipulatorStyles.PickerDetailRow);

            var stack = new VisualElement { style = { flexDirection = FlexDirection.Column, flexGrow = 1 } };

            var nameLabel = new Label(template.Name) { style = { unityTextAlign = TextAnchor.MiddleLeft } };
            nameLabel.AddToClassList(AcknexManipulatorStyles.PickerTemplateName);
            stack.Add(nameLabel);

            var contentRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 2 } };
            contentRow.Add(TextureReferenceDetailView.BuildThumbnail(template.Texture, size: 40));
            var textureInfoLabel = TextureReferenceDetailView.BuildCompactInfoLabel(template.Texture);
            textureInfoLabel.AddToClassList(AcknexManipulatorStyles.PickerTextureInfo);
            textureInfoLabel.style.marginLeft = 6;
            contentRow.Add(textureInfoLabel);
            stack.Add(contentRow);

            row.Add(stack);

            if (template.HasActionProperties)
                row.Add(BuildActionIcon());

            return row;
        }

        /// <summary>Lightning-bolt indicator shown when a Template has at least one WDL callback set (see
        /// BaseObjectTemplate.HasActionProperties). Explicit alignSelf/unityTextAlign, rather than relying
        /// on the parent row's own alignItems, keeps the glyph centered on its own regardless of how tall
        /// its container ends up next to the Name label's shorter line height.</summary>
        static VisualElement BuildActionIcon()
        {
            var icon = new Label("⚡") { style = { alignSelf = Align.Center, unityTextAlign = TextAnchor.MiddleCenter } };
            icon.AddToClassList(AcknexManipulatorStyles.PickerActionIcon);
            icon.tooltip = "Has one or more WDL action callbacks set";
            return icon;
        }

        /// <summary>Distinguishes an ordinary row selection (already-registered Template) from a
        /// confirmed "+" (a candidate not yet in TemplateRegistry.ByName), and for the latter, persists
        /// and registers it via TemplateRegistry.Clone - sourced from m_currentTemplate, not
        /// evt.previousValue, which does not reliably reflect the Template LoadValues resolved.</summary>
        void OnTemplateComboValueChanged(ChangeEvent<WallTemplate> evt)
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

        protected override void LoadValues(Segment copy)
        {
            bool wasUnnamed = string.IsNullOrEmpty(copy.Name);

            // Resolved before base.LoadValues(copy), which calls LoadTextureInfo internally and needs
            // m_currentTemplate already set.
            m_currentTemplate = m_templateResolver.Resolve(copy.Name);

            base.LoadValues(copy);

            m_templateCombo.Refresh();
            m_templateCombo.SetValueWithoutNotify(m_currentTemplate);

            // An unnamed Segment has no meaningful state on its own - it only makes sense tied to a
            // Template - so the resolved default is committed immediately rather than left pending until
            // the user happens to press OK.
            if (wasUnnamed)
                ApplyNow();
        }

        protected override void WriteBack(Segment target, Segment editedCopy)
        {
            base.WriteBack(target, editedCopy); // sets target.Name from the (hidden, untouched) base Name field

            if (m_templateCombo.value != null)
                target.Name = m_templateCombo.value.Name; // Name is derived from the selected Template, overriding whatever base just set
        }

        /// <summary>Shows the resolved Template's Texture (if any) in the base texture slot, in place of
        /// the base class's permanent placeholder text.</summary>
        protected override void LoadTextureInfo(NameTextureSlot slot, Segment copy)
        {
            var texture = m_currentTemplate?.Texture;
            slot.TextureNameValue.text = texture?.Name ?? "-";
            slot.ScaleValue.text = texture?.Template != null
                ? $"Scale X/Y: {texture.Template.Scale_x:0.###} / {texture.Template.Scale_y:0.###}"
                : "Scale X/Y: -";
        }

        /// <summary>nameProvider.Choices carries every Segment's current Name in the map, handed fresh on
        /// every Manipulator open regardless of which Segment - so the first open after a map load already
        /// sees the full list, and resolving all of them up front means every Wall's WallTemplate exists
        /// (falling back to the shared default where nothing matches yet) before its own Manipulator is ever
        /// opened, not just the one Segment being edited right now. Unrelated to this class's own
        /// IGenericNameProvider&lt;WallTemplate&gt; usage in BuildTemplatePicker - this nameProvider is over
        /// plain Segment Names (T = string, fixed by the base class), never over WallTemplate itself.</summary>
        public override void SetProviders(IGenericNameProvider<string> nameProvider, ITextureProvider textureProvider)
        {
            base.SetProviders(nameProvider, textureProvider);
            m_templateResolver.ResolveAll(nameProvider.Choices);
        }
    }
}
