using UI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Type-erased view of an ObjectTemplatePicker&lt;T&gt;, so AcknexMapObjectManipulator can drive the
    /// Thing and Actor pickers the same way.
    /// </summary>
    internal interface IObjectTemplatePicker
    {
        /// <summary>Root element (section title + picker) to place in a Manipulator and show/hide as a whole.</summary>
        VisualElement Root { get; }

        /// <summary>The Template currently selected, or null before the first Select.</summary>
        BaseObjectTemplate Current { get; }

        /// <summary>Resolves name to a Template (null/empty = the type's default Template, created if
        /// missing) and shows it as the selection without raising a change notification.</summary>
        void Select(string name);
    }

    /// <summary>
    /// Template picker for one Template type (Thing or Actor): a "Template" section title above a
    /// GenericComboBoxField&lt;T&gt; wired to that type's TemplateRegistry, with the shared detail-row
    /// layout (Template Name, Texture thumbnail + compact info, action icon).
    /// </summary>
    /// <typeparam name="T">The Template type this picker selects.</typeparam>
    internal class ObjectTemplatePicker<T> : IObjectTemplatePicker where T : BaseObjectTemplate
    {
        readonly TemplateRegistry<T> m_registry;
        readonly TemplateResolver<T> m_resolver;
        readonly System.Action m_selectionChanged;
        readonly GenericComboBoxField<T> m_combo;
        T m_current;

        /// <inheritdoc/>
        public VisualElement Root { get; }

        /// <inheritdoc/>
        public BaseObjectTemplate Current => m_current;

        /// <summary>Builds the picker against registry (Choices and the "+" clone) and resolver (Name to
        /// Template). selectionChanged is invoked after the user picks or adds a Template.</summary>
        public ObjectTemplatePicker(TemplateRegistry<T> registry, TemplateResolver<T> resolver, System.Action selectionChanged)
        {
            m_registry = registry;
            m_resolver = resolver;
            m_selectionChanged = selectionChanged;

            Root = new VisualElement();

            var title = new Label("Template");
            title.AddToClassList("manip-section-title");
            Root.Add(title);

            m_combo = new GenericComboBoxField<T>
            {
                Choices = registry.Choices,
                ItemFactory = registry.ItemFactory,
                Sanitizer = registry.Sanitizer,
                AllowDetailMode = true,
                DetailViewBuilder = BuildDetailView,
                VisibleRowCount = 6,
                DetailVisibleRowCount = 3,
                // A Template can be shared by many objects (resolved by Name); deletion is not supported.
                AllowDelete = false,
            };
            m_combo.RegisterValueChangedCallback(OnComboValueChanged);
            m_combo.AddToClassList("manip-picker-dropdown");
            Root.Add(m_combo);
        }

        /// <inheritdoc/>
        public void Select(string name)
        {
            m_current = m_resolver.Resolve(name);
            m_combo.Refresh();
            m_combo.SetValueWithoutNotify(m_current);
        }

        /// <summary>Ordinary selection of a registered Template just becomes current; a confirmed "+"
        /// (a candidate not yet in the registry) is persisted as a clone of the current Template.</summary>
        void OnComboValueChanged(ChangeEvent<T> evt)
        {
            var newValue = evt.newValue;
            if (newValue == null || m_registry.ByName.ContainsKey(newValue.Name))
            {
                m_current = newValue;
            }
            else
            {
                m_registry.Choices.Remove(newValue);
                m_current = m_registry.Clone(m_current, newValue.Name);
                m_combo.SetValueWithoutNotify(m_current);
                m_combo.Refresh();
            }

            m_selectionChanged?.Invoke();
        }

        /// <summary>Template Name on top, Texture thumbnail with its compact info line below, action icon
        /// centered against the whole row (same layout as the Wall picker).</summary>
        static VisualElement BuildDetailView(T template)
        {
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
            {
                var icon = new Label("⚡") { style = { alignSelf = Align.Center, unityTextAlign = TextAnchor.MiddleCenter } };
                icon.AddToClassList(AcknexManipulatorStyles.PickerActionIcon);
                icon.tooltip = "Has one or more WDL action callbacks set";
                row.Add(icon);
            }

            return row;
        }
    }
}
