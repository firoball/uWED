using UnityEngine;

namespace uWED.Acknex.Runtime.Model.Assets
{
    /// <summary>Bitmap font asset - adds overall glyph cell Width/Height to Bitmap's sub-region fields.</summary>
    public class Font : Bitmap
    {
        [SerializeField] int m_width;
        [SerializeField] int m_height;

        /// <summary>Glyph cell width.</summary>
        public int Width { get => m_width; set => m_width = value; }
        /// <summary>Glyph cell height.</summary>
        public int Height { get => m_height; set => m_height = value; }
    }
}
