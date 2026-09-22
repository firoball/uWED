using UnityEngine.UIElements;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Texture Offset + Texture "slot": Texture Offset stepper, then a Texture
    /// card (square preview with Hint overlaid top-left, read-only Texture Name
    /// label below the preview - not a dropdown, matching InfoPanel - Scale
    /// X/Y below that, and a placeholder "..." button for a future real
    /// texture-asset picker).
    ///
    /// Every Name/variant picker across all manipulators is now a standalone
    /// field built directly by the manipulator (see IGenericNameProvider&lt;T&gt;
    /// / ComboBoxField / GenericComboBoxField&lt;T&gt;) - this slot no longer
    /// hosts its own Name section, so it is uniformly texture-display-only
    /// wherever it's used (MapObject, Region x2, Segment).
    ///
    /// A container hosts one or more of these side by side ("manip-slots-row" +
    /// "manip-slot") - a single visible slot naturally fills the row via
    /// flex-grow, no extra layout logic needed when a second is hidden.
    ///
    /// The texture Hint is a plain, non-focusable Label (not backed by a
    /// provider/registry) sized to its own text, like InfoPanel - it's just a
    /// free display label for telling multiple textures apart when more than
    /// one is visible. Not editable yet (no backing property to write to).
    /// </summary>
    public class NameTextureSlot : VisualElement
    {
        public Vector2StepperField OffsetStepper { get; }

        public VisualElement TexturePreview { get; }
        public Label TextureHintValue { get; }
        public Label TextureNameValue { get; }
        public Button TextureSelectButton { get; }
        public Label ScaleValue { get; }

        readonly Label m_offsetTitle;

        public NameTextureSlot()
        {
            AddToClassList("manip-slot");

            // --- Texture Offset ---
            m_offsetTitle = new Label("Texture Offset");
            m_offsetTitle.AddToClassList("manip-section-title");
            Add(m_offsetTitle);

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

        /// <summary>Hides the Texture Offset title/stepper, for object types
        /// with no per-slot offset property.</summary>
        public void SetOffsetSectionVisible(bool visible)
        {
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            m_offsetTitle.style.display = display;
            OffsetStepper.style.display = display;
        }
    }
}
