using UI.Controls;
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
                // Keeps the open popup short - the picker is for quick switching between a handful of
                // Templates, not browsing the full list at a glance.
                VisibleRowCount = 3,
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

        /// <summary>Thumbnail first (Name alone reads as messy without it), then Name/Size/Scale, then the
        /// lightning-bolt action-indicator when any WDL callback is set. Styling (the row's bottom-border
        /// separator, the info column, the icon) comes from AcknexManipulatorStyles' USS classes.</summary>
        static VisualElement BuildDetailView(WallTemplate template)
        {
            var row = new VisualElement();
            row.AddToClassList(AcknexManipulatorStyles.PickerDetailRow);
            row.Add(TextureReferenceDetailView.BuildThumbnail(template.Texture));

            var info = new VisualElement();
            info.AddToClassList(AcknexManipulatorStyles.PickerDetailInfo);
            info.Add(new Label(template.Name));
            info.Add(TextureReferenceDetailView.BuildSizeLabel(template.Texture));
            info.Add(TextureReferenceDetailView.BuildScaleLabel(template.Texture));
            row.Add(info);

            if (template.HasActionProperties)
                row.Add(BuildActionIcon());

            return row;
        }

        /// <summary>Lightning-bolt indicator shown when a Template has at least one WDL callback set
        /// (see BaseObjectTemplate.HasActionProperties).</summary>
        static VisualElement BuildActionIcon()
        {
            var icon = new Label("⚡");
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
            // Resolved before base.LoadValues(copy), which calls LoadTextureInfo internally and needs
            // m_currentTemplate already set.
            m_currentTemplate = m_templateResolver.Resolve(copy.Name);

            base.LoadValues(copy);

            m_templateCombo.Refresh();
            m_templateCombo.SetValueWithoutNotify(m_currentTemplate);
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
