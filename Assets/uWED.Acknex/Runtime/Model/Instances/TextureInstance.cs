using System;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;
using Texture = uWED.Runtime.UI.Manipulator.Texture;

namespace uWED.Acknex.Runtime.Model.Instances
{
    /// <summary>
    /// Texture instance - extends uWED core's base Texture with a reference to the TextureTemplate
    /// carrying its Acknex-specific properties, mirroring the WallInstance/ThingInstance/etc. pattern.
    /// A plain class like its base Texture, not a ScriptableObject - fields referencing a TextureInstance
    /// use [SerializeReference], not [SerializeField], since plain-class fields would otherwise be
    /// embedded by value (an inline copy per occurrence) rather than as a shared, graph/cycle-safe
    /// reference - see TextureTemplate.Attach. Has no Manipulator wiring - Texture's editing UI is built
    /// separately from the other Template types' shared Manipulator popup (see texture.md).
    /// </summary>
    [Serializable]
    public class TextureInstance : Texture
    {
        [SerializeField] TextureTemplate m_template;

        /// <summary>The Template carrying this instance's Acknex-specific properties.</summary>
        public TextureTemplate Template { get => m_template; set => m_template = value; }

        /// <summary>Name, used by GenericComboBoxField&lt;TextureInstance&gt; for its search/display text.</summary>
        public override string ToString() => Name;
    }
}
