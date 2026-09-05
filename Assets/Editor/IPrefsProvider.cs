namespace Runtime.Platform
{
    /// <summary>
    /// Abstraction over a simple persistent key/value store (EditorPrefs on the
    /// Editor platform; whatever equivalent standalone builds use elsewhere).
    /// Implementations are resolved once per session/build and handed to
    /// consumers via IPrefsPersistable.SetPrefsProvider - no direct EditorPrefs
    /// usage should remain outside the Editor-only implementation of this
    /// interface.
    /// </summary>
    public interface IPrefsProvider
    {
        bool HasKey(string key);

        float GetFloat(string key, float defaultValue = 0f);
        void SetFloat(string key, float value);

        bool GetBool(string key, bool defaultValue = false);
        void SetBool(string key, bool value);
    }
}
