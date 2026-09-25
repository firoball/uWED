using UnityEngine;
using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Texture-display slot: an Offset stepper, then a Texture card (square
    /// preview with a Hint label overlaid top-left, read-only Name label
    /// below, Scale X/Y below that, and a placeholder "..." button for a
    /// future texture-asset picker). Purely display-only - Name/variant
    /// selection lives in its own field elsewhere (GenericComboBoxField),
    /// not in this slot.
    ///
    /// A container hosts one or more side by side ("manip-slots-row" +
    /// "manip-slot"); a single visible slot fills the row via flex-grow.
    ///
    /// Hint is a free-standing Label (no backing property) for telling
    /// multiple visible textures apart - not user-editable.
    ///
    /// Holds no texture data of its own. The caller (e.g. a uWED.Acknex
    /// Manipulator reading its Template's Texture reference) calls
    /// SetTexture() after LoadValues(). Scale X/Y and the "..." select
    /// button aren't wired by SetTexture() - both remain placeholders.
    /// </summary>
    public class NameTextureSlot : VisualElement
    {
        /// <summary>The Texture Offset X/Y stepper.</summary>
        public Vector2StepperField OffsetStepper { get; }

        /// <summary>Square preview image area - background image is set via SetTexture().</summary>
        public VisualElement TexturePreview { get; }
        /// <summary>Overlay label distinguishing this slot among several visible ones.</summary>
        public Label TextureHintValue { get; }
        /// <summary>Read-only label showing the texture's Name.</summary>
        public Label TextureNameValue { get; }
        /// <summary>Placeholder button for a future texture-asset picker; not wired up yet.</summary>
        public Button TextureSelectButton { get; }
        /// <summary>Label showing Scale X/Y; not written to by SetTexture().</summary>
        public Label ScaleValue { get; }

        readonly Label m_offsetTitle;
        readonly Label m_previewPlaceholderLabel;

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

            m_previewPlaceholderLabel = new Label("No preview\n(placeholder)");
            m_previewPlaceholderLabel.AddToClassList("manip-texture-preview-label");
            TexturePreview.Add(m_previewPlaceholderLabel);

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

        /// <summary>
        /// Displays the texture's preview image and Name, or resets to the
        /// placeholder state if <paramref name="texture"/> or its Value is
        /// null. Doesn't touch ScaleValue or TextureSelectButton.
        /// </summary>
        public void SetTexture(Texture texture)
        {
            Background? background = texture?.Value switch
            {
                Texture2D tex2D => Background.FromTexture2D(tex2D),
                RenderTexture renderTex => Background.FromRenderTexture(renderTex),
                _ => null, // no Value, or a type UI Toolkit can't show as a flat preview (e.g. Cubemap/Texture3D)
            };

            bool hasPreview = background.HasValue;
            TexturePreview.style.backgroundImage = hasPreview ? new StyleBackground(background.Value) : null;
            m_previewPlaceholderLabel.style.display = hasPreview ? DisplayStyle.None : DisplayStyle.Flex;

            TextureNameValue.text = texture != null ? texture.Name : "-";
        }
    }
}
