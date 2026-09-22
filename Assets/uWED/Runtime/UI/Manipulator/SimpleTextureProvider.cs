using System.Collections.Generic;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Minimal in-memory ITextureProvider, matching SimpleNameProvider. Texture
    /// handling is placeholder-only for now (see README), so this just needs to
    /// exist for the dropdown to have something to list - no real Texture2D
    /// resolution happens here.
    /// </summary>
    public class SimpleTextureProvider : ITextureProvider
    {
        readonly List<string> m_textureNames = new List<string>();

        public IReadOnlyList<string> GetTextureNames() => m_textureNames;

        public bool TryCreateTextureName(string sanitizedName)
        {
            if (string.IsNullOrEmpty(sanitizedName) || m_textureNames.Contains(sanitizedName))
                return false;

            m_textureNames.Add(sanitizedName);
            return true;
        }
    }
}
