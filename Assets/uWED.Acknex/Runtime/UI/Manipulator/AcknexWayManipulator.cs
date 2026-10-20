using UI.Controls;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// WayManipulator whose Name is chosen from the WayTemplate picker instead of a plain rename field: an
    /// existing Template links the Way to it, a new name typed into the picker and confirmed with "+"
    /// creates a new WayTemplate. A Way has no Template properties, so there are no Template tabs. An
    /// unnamed Way resolves to the default WayTemplate, whose name is committed into the Way.
    /// </summary>
    public class AcknexWayManipulator : WayManipulator
    {
        readonly TemplateRegistry<WayTemplate> m_templateRegistry;
        readonly TemplateResolver<WayTemplate> m_templateResolver;
        GenericComboBoxField<WayTemplate> m_templateCombo;

        // Resolved once per Open() in LoadValues, valid until the next LoadValues/WriteBack.
        WayTemplate m_currentTemplate;

        /// <summary>Builds the window; the picker's choices and "+" read from templateRegistry,
        /// templateResolver maps a Way's name to its Template.</summary>
        public AcknexWayManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings, TemplateRegistry<WayTemplate> templateRegistry, TemplateResolver<WayTemplate> templateResolver)
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

            m_templateCombo = new GenericComboBoxField<WayTemplate>
            {
                Choices = m_templateRegistry.Choices,
                ItemFactory = m_templateRegistry.ItemFactory,
                Sanitizer = m_templateRegistry.Sanitizer,
                VisibleRowCount = 6,
                AllowDelete = false,
            };
            m_templateCombo.RegisterValueChangedCallback(OnTemplateComboValueChanged);
            m_templateCombo.AddToClassList("manip-picker-dropdown");
            section.Add(m_templateCombo);
            section.RegisterCallback<AttachToPanelEvent>(_ => m_templateRegistry.ListChanged += m_templateCombo.Refresh);
            section.RegisterCallback<DetachFromPanelEvent>(_ => m_templateRegistry.ListChanged -= m_templateCombo.Refresh);

            // Placed where the hidden Name section sits: the base class's only ComboBoxField is its Name combo.
            var content = this.Q<VisualElement>("manip-content-container");
            content.Insert(content.IndexOf(content.Q<ComboBoxField>().parent), section);
        }

        /// <summary>An ordinary row selection of a registered Template just becomes current; a confirmed
        /// "+" (a candidate not yet registered) is persisted as a clone of the current Template.</summary>
        void OnTemplateComboValueChanged(ChangeEvent<WayTemplate> evt)
        {
            var newValue = evt.newValue;
            if (newValue == null || m_templateRegistry.ByName.ContainsKey(newValue.Name))
            {
                m_currentTemplate = newValue;
                return;
            }

            m_templateRegistry.Choices.Remove(newValue);
            m_currentTemplate = m_templateRegistry.Clone(m_currentTemplate, newValue.Name);
            m_templateCombo.SetValueWithoutNotify(m_currentTemplate);
            m_templateCombo.Refresh();
        }

        /// <summary>nameProvider.Choices carries every Way's current Name in the map; each one gets its
        /// WayTemplate resolved (created if missing).</summary>
        public override void SetProviders(IGenericNameProvider<string> nameProvider)
        {
            base.SetProviders(nameProvider);
            if (nameProvider != null)
                m_templateResolver.ResolveAll(nameProvider.Choices);
        }

        /// <summary>Shows the Template the Way's name resolves to. An unnamed Way gets the default
        /// Template, which is committed immediately.</summary>
        protected override void LoadValues(Way copy)
        {
            bool wasUnnamed = string.IsNullOrEmpty(copy.Name);
            m_currentTemplate = m_templateResolver.Resolve(copy.Name);

            base.LoadValues(copy);

            m_templateCombo.Refresh();
            m_templateCombo.SetValueWithoutNotify(m_currentTemplate);

            if (wasUnnamed)
                ApplyNow();
        }

        /// <summary>The Way's name is the name of the selected Template.</summary>
        protected override void WriteBack(Way target, Way editedCopy)
        {
            base.WriteBack(target, editedCopy);

            if (m_templateCombo.value != null)
                target.Name = m_templateCombo.value.Name;
        }
    }
}
