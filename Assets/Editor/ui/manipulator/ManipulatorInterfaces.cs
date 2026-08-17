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
    /// Supplies the read-only pool of existing names for the Name picker (dropdown +
    /// "create new" button). The returned list is treated as read-only by the
    /// Manipulator - it is never mutated or re-cased in place. Display-only
    /// lowercasing happens purely in the UI layer.
    /// </summary>
    public interface INameProvider
    {
        IReadOnlyList<string> GetNames();

        /// <summary>
        /// Called with an already-sanitized (trimmed, lowercase, [a-z0-9_]) name.
        /// Implementer registers it in the backing store and returns true on
        /// success (e.g. false if duplicate and duplicates aren't allowed).
        /// </summary>
        bool TryCreateName(string sanitizedName);
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
