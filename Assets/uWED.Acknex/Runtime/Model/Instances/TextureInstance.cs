using System;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.Model.Instances
{
    /// <summary>
    /// Texture instance - extends uWED core's NameTextureSlot-display helper Texture with a reference
    /// to the TextureTemplate carrying its Acknex-specific properties. Unlike Wall/Thing/Actor/Region,
    /// where the connection between a placed Instance and its Template is a stateless by-Name lookup
    /// (see Registry/TemplateResolver.cs) because the map format only ever holds plain base types, a
    /// Texture reference is carried directly by whatever field points to it (see TextureTemplate.Attach),
    /// so a TextureInstance is free to be an ordinary data-carrying object. A plain class like its base
    /// Texture, not a ScriptableObject - fields referencing a
    /// TextureInstance use [SerializeReference], not [SerializeField], since plain-class fields would
    /// otherwise be embedded by value (an inline copy per occurrence) rather than as a shared,
    /// graph/cycle-safe reference. Has no Manipulator wiring of its own - Texture's editing UI is built
    /// separately from the other Template types' shared Manipulator popup (see texture.md).
    /// </summary>
    [Serializable]
    public class TextureInstance : uWED.Runtime.UI.Manipulator.Texture
    {
        [SerializeField] TextureTemplate m_template;

        /// <summary>The Template carrying this instance's Acknex-specific properties.</summary>
        public TextureTemplate Template { get => m_template; set => m_template = value; }

        /// <summary>Name, used by GenericComboBoxField&lt;TextureInstance&gt; for its search/display text.</summary>
        public override string ToString() => Name;
    }
}
