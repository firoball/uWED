using UnityEditor;
using Runtime.Platform;

namespace Editor.Platform
{
    /// <summary>
    /// EditorPrefs-backed IPrefsProvider. This is the one deliberate
    /// UnityEditor dependency in the whole prefs abstraction - EditorPrefs has
    /// no engine-side equivalent. Replace with a standalone-specific
    /// implementation once the project's platform/service abstraction exists.
    /// </summary>
    public class EditorPrefsProvider : IPrefsProvider
    {
        public bool HasKey(string key) => EditorPrefs.HasKey(key);

        public float GetFloat(string key, float defaultValue = 0f) =>
            EditorPrefs.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value) =>
            EditorPrefs.SetFloat(key, value);

        public bool GetBool(string key, bool defaultValue = false) =>
            EditorPrefs.GetBool(key, defaultValue);

        public void SetBool(string key, bool value) =>
            EditorPrefs.SetBool(key, value);
    }
}
