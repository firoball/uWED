using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Way tab. Only Name is editable (dropdown + create, same picker markup
    /// NameTextureSlot uses for its Name section, built standalone here since
    /// Way has no texture/offset to pair it with). Vertex list is shown
    /// read-only as its Count.
    /// </summary>
    public class WayManipulator : ManipulatorWindowBase<Way>
    {
        protected override string TypeLabel => "Way";
        protected override bool UsesLinearStep => false;
        protected override bool UsesAngleStep => false;

        INameProvider m_nameProvider;

        DropdownField m_nameDropdown;
        Button m_nameNewButton;
        TextField m_nameNewEntry;

        Label m_vertexCountValue;

        public WayManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
            : base(baseUxml, settings)
        {
        }

        /// <summary>Call once the provider is ready (not necessarily at construction).</summary>
        public void SetProviders(INameProvider nameProvider)
        {
            m_nameProvider = nameProvider;
        }

        protected override void PopulateContent(VisualElement container)
        {
            BuildNameField(container);
            BuildReadonlyBlock(container);
        }

        void BuildNameField(VisualElement container)
        {
            var nameTitle = new Label("Name");
            nameTitle.AddToClassList("manip-section-title");
            container.Add(nameTitle);

            var nameRow = new VisualElement();
            nameRow.AddToClassList("manip-picker-row");

            m_nameDropdown = new DropdownField();
            m_nameDropdown.AddToClassList("manip-picker-dropdown");
            m_nameDropdown.formatListItemCallback = DisplayLowercase;
            m_nameDropdown.formatSelectedValueCallback = DisplayLowercase;
            nameRow.Add(m_nameDropdown);

            m_nameNewButton = new Button { text = "+" };
            m_nameNewButton.AddToClassList("manip-picker-new-button");
            nameRow.Add(m_nameNewButton);

            container.Add(nameRow);

            m_nameNewEntry = new TextField { isDelayed = false };
            m_nameNewEntry.AddToClassList("manip-picker-new-entry");
            container.Add(m_nameNewEntry);

            m_nameNewButton.clicked += () => ShowNewEntry(m_nameNewEntry);

            m_nameNewEntry.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNewName();
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

        void CommitNewName()
        {
            string sanitized = NameSanitizer.Sanitize(m_nameNewEntry.value);
            m_nameNewEntry.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(sanitized)) return;

            if (m_nameProvider != null && m_nameProvider.TryCreateName(sanitized))
            {
                RefreshNameChoices();
                m_nameDropdown.value = sanitized;
            }
        }

        // schedule.Execute defers one frame - avoids a DropdownField popup
        // measuring against a not-yet-laid-out panel (blank rows until scrolled).
        void RefreshNameChoices()
        {
            if (m_nameProvider == null) return;
            var names = new List<string>(m_nameProvider.GetNames());
            m_nameDropdown.schedule.Execute(() => m_nameDropdown.choices = names);
        }

        void BuildReadonlyBlock(VisualElement container)
        {
            var block = new VisualElement();
            block.AddToClassList("manip-readonly-block");

            m_vertexCountValue = AddReadonlyRow(block, "Vertices");

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

        protected override Way Clone(Way source)
        {
            // Positions has no public setter beyond the ctor - pass the same
            // list reference through (Way's vertex list is not editable here).
            return new Way(source.Positions, source.Name);
        }

        protected override void LoadValues(Way copy)
        {
            RefreshNameChoices();
            m_nameNewEntry.style.display = DisplayStyle.None;
            m_nameDropdown.SetValueWithoutNotify(copy.Name);

            m_vertexCountValue.text = OriginalTarget.Positions != null
                ? OriginalTarget.Positions.Count.ToString()
                : "0";
        }

        protected override void WriteBack(Way target, Way editedCopy)
        {
            target.Name = m_nameDropdown.value;
        }
    }
}
