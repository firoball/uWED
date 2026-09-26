using UnityEngine;

namespace uWED.Acknex.Runtime.Model.Assets
{
    /// <summary>
    /// Common base for Bmap/Font/Ovly - mirrors AcknexCSApi's abstract Bitmap: a sub-region (X/Y
    /// offset, Dx/Dy size) within the source file referenced by ClassicAsset.File.
    /// </summary>
    public abstract class Bitmap : ClassicAsset
    {
        [SerializeField] int m_x;
        [SerializeField] int m_y;
        [SerializeField] int m_dx;
        [SerializeField] int m_dy;

        /// <summary>Sub-region X offset within the source file.</summary>
        public int X { get => m_x; set => m_x = value; }
        /// <summary>Sub-region Y offset within the source file.</summary>
        public int Y { get => m_y; set => m_y = value; }
        /// <summary>Sub-region width.</summary>
        public int Dx { get => m_dx; set => m_dx = value; }
        /// <summary>Sub-region height.</summary>
        public int Dy { get => m_dy; set => m_dy = value; }
    }
}
