using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// One Name + Texture Offset + Texture "slot": Name picker (dropdown +
    /// create) first, then the Texture Offset stepper, then a Texture card
    /// (square preview with Hint overlaid top-left, read-only Texture Name
    /// label below the preview - not a dropdown, matching InfoPanel - Scale
    /// X/Y below that, and a placeholder "..." button for a future real
    /// texture-asset picker).
    ///
    /// This is the basic repeatable unit for object types that can have more
    /// than one Name/Offset/Texture pairing (Segment: up to 2 today). A
    /// container hosts one or more of these side by side ("manip-slots-row" +
    /// "manip-slot") - a single visible slot naturally fills the row via
    /// flex-grow, no extra layout logic needed when the second is hidden.
    ///
    /// The texture Hint is a plain, non-focusable Label (not backed by a
    /// provider/registry) sized to its own text, like InfoPanel - it's just a
    /// free display label for telling multiple textures apart when more than
    /// one is visible. Not editable yet (no backing property to write to).
    /// </summary>
    public class NameTextureSlot : VisualElement
    {
        public DropdownField NameDropdown { get; }
        public Button NameNewButton { get; }
        public TextField NameNewEntry { get; }

        public Vector2StepperField OffsetStepper { get; }

        public VisualElement TexturePreview { get; }
        public Label TextureHintValue { get; }
        public Label TextureNameValue { get; }
        public Button TextureSelectButton { get; }
        public Label ScaleValue { get; }

        readonly Label m_nameTitle;
        readonly VisualElement m_nameRow;
        readonly Label m_offsetTitle;

        public NameTextureSlot()
        {
            AddToClassList("manip-slot");

            // --- Name ---
            var nameTitle = new Label("Name");
            nameTitle.AddToClassList("manip-section-title");
            Add(nameTitle);
            m_nameTitle = nameTitle;

            var nameRow = new VisualElement();
            nameRow.AddToClassList("manip-picker-row");
            m_nameRow = nameRow;

            NameDropdown = new DropdownField();
            NameDropdown.AddToClassList("manip-picker-dropdown");
            NameDropdown.formatListItemCallback = DisplayLowercase;
            NameDropdown.formatSelectedValueCallback = DisplayLowercase;
            nameRow.Add(NameDropdown);

            NameNewButton = new Button { text = "+" };
            NameNewButton.AddToClassList("manip-picker-new-button");
            nameRow.Add(NameNewButton);

            Add(nameRow);

            NameNewEntry = new TextField { isDelayed = false };
            NameNewEntry.AddToClassList("manip-picker-new-entry");
            Add(NameNewEntry);

            NameNewButton.clicked += () => ShowNewEntry(NameNewEntry);

            // --- Texture Offset ---
            var offsetTitle = new Label("Texture Offset");
            offsetTitle.AddToClassList("manip-section-title");
            Add(offsetTitle);
            m_offsetTitle = offsetTitle;

            OffsetStepper = new Vector2StepperField();
            Add(OffsetStepper);

            // --- Texture ---
            var textureTitle = new Label("Texture");
            textureTitle.AddToClassList("manip-section-title");
            Add(textureTitle);

            var textureCard = new VisualElement();
            textureCard.AddToClassList("manip-texture-card");

            TexturePreview = new VisualElement();
            TexturePreview.AddToClassList("manip-texture-preview");
            var previewLabel = new Label("No preview\n(placeholder)");
            previewLabel.AddToClassList("manip-texture-preview-label");
            TexturePreview.Add(previewLabel);

            TextureHintValue = new Label("Hint");
            TextureHintValue.AddToClassList("manip-texture-hint-overlay");
            TexturePreview.Add(TextureHintValue); // overlaid via USS position:absolute, top-left

            textureCard.Add(TexturePreview);

            var textureNameRow = new VisualElement();
            textureNameRow.AddToClassList("manip-texture-name-row");

            TextureNameValue = new Label("-");
            TextureNameValue.AddToClassList("manip-texture-info-label");
            textureNameRow.Add(TextureNameValue);

            TextureSelectButton = new Button { text = "..." };
            TextureSelectButton.AddToClassList("manip-texture-select-button");
            TextureSelectButton.tooltip = "Open texture selection (not implemented yet)";
            textureNameRow.Add(TextureSelectButton);

            textureCard.Add(textureNameRow);

            var scaleLabel = new Label("Scale X/Y: -");
            scaleLabel.AddToClassList("manip-texture-info-label");
            ScaleValue = scaleLabel;
            textureCard.Add(scaleLabel);

            Add(textureCard);
        }

        static void ShowNewEntry(TextField entry)
        {
            entry.style.display = DisplayStyle.Flex;
            entry.SetValueWithoutNotify(string.Empty);
            entry.Focus();
        }

        static string DisplayLowercase(string value) => string.IsNullOrEmpty(value) ? value : value.ToLowerInvariant();

        public void SetNameChoices(IReadOnlyList<string> names) => NameDropdown.choices = new List<string>(names);

        /// <summary>Hides/shows the Name title, dropdown row, and new-entry field together.
        /// Used where a slot's texture has no separate name (e.g. Region, MapObject) - unlike
        /// Segment, whose slot Name is wired to Segment.Name.</summary>
        public void SetNameSectionVisible(bool visible)
        {
            m_nameTitle.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            m_nameRow.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) NameNewEntry.style.display = DisplayStyle.None;
        }

        /// <summary>Hides/shows the Texture Offset title and stepper together.</summary>
        public void SetOffsetSectionVisible(bool visible)
        {
            m_offsetTitle.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            OffsetStepper.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
