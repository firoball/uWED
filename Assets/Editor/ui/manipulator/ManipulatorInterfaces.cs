using System.Collections.Generic;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Read/write access to the shared step values shown in the settings bar.
    /// Implement this against EditorPrefs (or wherever settings should persist) and
    /// pass a single shared instance into every Manipulator window - all tabs/types
    /// use the same step values.
    /// </summary>
    public interface IManipulatorSettings
    {
        float LinearStep { get; set; }
        float AngleStep { get; set; }
    }

    /// <summary>
    /// Supplies the read-only pool of existing texture names, and eventually actual
    /// texture lookup. Placeholder for now - texture display is informational only
    /// (name + scale X/Y), no real Texture2D resolution wired up yet.
    /// </summary>
    public interface ITextureProvider
    {
        IReadOnlyList<string> GetTextureNames();
        bool TryCreateTextureName(string sanitizedName);
    }

}
