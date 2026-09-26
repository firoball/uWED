using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.Model.Assets
{
    /// <summary>
    /// Palette asset - AcknexCSApi models this as its own hierarchy (A3Object), not an Asset/Bitmap
    /// subtype, so it derives from TemplateAsset directly rather than ClassicAsset (no File field).
    /// Range and Anicolor are not modeled - their original Var[,]/Var[,,,] shapes are unresolved in the
    /// source data (see classic-assets.md's Palette table).
    /// </summary>
    public class Palette : TemplateAsset
    {
        [SerializeField] string m_palfile;
        [SerializeField] string m_anifile;
        [SerializeField] float m_cycle;
        [SerializeField] uint m_flags;

        /// <summary>Palette source file.</summary>
        public string Palfile { get => m_palfile; set => m_palfile = value; }
        /// <summary>Animation source file for palette color-cycling.</summary>
        public string Anifile { get => m_anifile; set => m_anifile = value; }
        /// <summary>Color-cycling speed.</summary>
        public float Cycle { get => m_cycle; set => m_cycle = value; }

        /// <summary>Hard palette flag.</summary>
        public bool Hard
        {
            get => m_flags.IsSet(AcknexFlag.Hard);
            set => m_flags = value ? m_flags.Set(AcknexFlag.Hard) : m_flags.Reset(AcknexFlag.Hard);
        }

        /// <summary>Autorange palette flag.</summary>
        public bool Autorange
        {
            get => m_flags.IsSet(AcknexFlag.Autorange);
            set => m_flags = value ? m_flags.Set(AcknexFlag.Autorange) : m_flags.Reset(AcknexFlag.Autorange);
        }

        /// <summary>Blur palette flag.</summary>
        public bool Blur
        {
            get => m_flags.IsSet(AcknexFlag.Blur);
            set => m_flags = value ? m_flags.Set(AcknexFlag.Blur) : m_flags.Reset(AcknexFlag.Blur);
        }
    }
}
