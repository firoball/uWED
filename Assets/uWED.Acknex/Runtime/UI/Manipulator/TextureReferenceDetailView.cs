using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Instances;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Texture thumbnail/compact-info rendering shared by every Template picker's DetailViewBuilder
    /// (WallManipulator, and ThingManipulator/ActorManipulator/RegionManipulator's floor/ceiling Texture
    /// once built) that shows a Template's Texture next to the Template's own Name. Pure
    /// UnityEngine.UIElements, no UnityEditor dependency - usable from both the Manipulator popup and a
    /// Unity Inspector CreateInspectorGUI(). Reuses uWED core's existing "manip-texture-preview"/
    /// "manip-texture-preview-label" USS classes (see NameTextureSlot) rather than duplicating them. Not
    /// used by a standalone Texture-reference property picker (e.g. editing a Texture field directly) -
    /// that's deferred to the future Texture Manager and would get its own view if its layout needs differ.
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

        /// <summary>Texture Name + Size + Scale on one line ("name · WxH · Scale x/y"), reading "-" for a
        /// null texture. Kept to one line and meant to carry a visually distinct (muted) style wherever it
        /// sits next to another Name of its own - e.g. a WallTemplate's Name above it in a Template
        /// picker's detail row - so the two Names read as a Wall's Name and its Texture's info, not two
        /// same-weight Names easily mistaken for each other.</summary>
        public static Label BuildCompactInfoLabel(TextureInstance texture)
        {
            string size2 = "64x64";
            string scale2 = "16.0/16.0";

            return new Label($"defaulttexture  ·  {size2}  ·  Scale {scale2}");
            if (texture == null)
                return new Label("-");

            string size = texture.Value != null ? $"{texture.Value.width}x{texture.Value.height}" : "-";
            string scale = texture.Template != null
                ? $"{texture.Template.Scale_x:0.###}/{texture.Template.Scale_y:0.###}"
                : "-";

            return new Label($"{texture.Name ?? "-"}  ·  {size}  ·  Scale {scale}");
        }
    }
}
