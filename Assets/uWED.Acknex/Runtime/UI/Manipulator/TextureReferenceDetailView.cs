using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Instances;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Texture thumbnail/Name/Size/Scale rendering shared by every GenericComboBoxField&lt;TextureInstance&gt;
    /// Texture-reference property picker and every Template picker's DetailViewBuilder (WallManipulator
    /// etc.) that shows a Template's Texture. Pure UnityEngine.UIElements, no UnityEditor dependency -
    /// usable from both the Manipulator popup and a Unity Inspector CreateInspectorGUI(). Reuses uWED
    /// core's existing "manip-texture-preview"/"manip-texture-preview-label" USS classes (see
    /// NameTextureSlot) rather than duplicating them.
    /// </summary>
    public static class TextureReferenceDetailView
    {
        /// <summary>Square preview box for texture, sized to size pixels - shows the placeholder state
        /// (no image) for a null texture or one with no Value assigned yet.</summary>
        public static VisualElement BuildThumbnail(TextureInstance texture, float size = 64)
        {
            var thumb = new VisualElement();
            thumb.style.width = size;
            thumb.style.height = size;
            thumb.AddToClassList("manip-texture-preview");

            // UnityEngine.Texture itself has no StyleBackground conversion - only Texture2D/
            // RenderTexture do (mirrors NameTextureSlot.SetTexture's own pattern for the same reason).
            Background? background = texture?.Value switch
            {
                Texture2D tex2D => Background.FromTexture2D(tex2D),
                RenderTexture renderTex => Background.FromRenderTexture(renderTex),
                _ => null, // no Value, or a type UI Toolkit can't show as a flat preview (e.g. Cubemap/Texture3D)
            };

            if (background.HasValue)
            {
                thumb.style.backgroundImage = new StyleBackground(background.Value);
                thumb.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain); // keeps aspect ratio within the fixed box
            }
            else
            {
                var placeholder = new Label("No preview");
                placeholder.AddToClassList("manip-texture-preview-label");
                thumb.Add(placeholder);
            }

            return thumb;
        }

        /// <summary>Scale X/Y label, reading "-" for a null texture or one with no Template assigned.</summary>
        public static VisualElement BuildScaleLabel(TextureInstance texture)
        {
            return texture?.Template == null
                ? new Label("Scale X/Y: -")
                : new Label($"Scale X/Y: {texture.Template.Scale_x:0.###} / {texture.Template.Scale_y:0.###}");
        }

        /// <summary>Pixel-dimensions label, reading "-" for a null texture or one with no Value assigned.
        /// Reads UnityEngine.Texture.width/height directly, so it applies to Texture2D and RenderTexture
        /// alike - distinct from Scale, which is the WDL Scale_x/Scale_y render scale, not pixel size.</summary>
        public static VisualElement BuildSizeLabel(TextureInstance texture)
        {
            return texture?.Value == null
                ? new Label("Size: -")
                : new Label($"Size: {texture.Value.width} x {texture.Value.height}");
        }

        /// <summary>Full thumbnail + Name + Size + Scale row, for the Texture-reference property picker
        /// (e.g. Wall.Texture).</summary>
        public static VisualElement Build(TextureInstance texture)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            row.Add(BuildThumbnail(texture));

            var info = new VisualElement { style = { marginLeft = 6 } };
            info.Add(new Label(texture?.Name ?? "-"));
            info.Add(BuildSizeLabel(texture));
            info.Add(BuildScaleLabel(texture));
            row.Add(info);

            return row;
        }
    }
}
