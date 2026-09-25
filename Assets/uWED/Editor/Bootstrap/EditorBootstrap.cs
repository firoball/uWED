using UnityEditor;
using uWED.Editor.Platform;
using uWED.Runtime.Platform;

namespace uWED.Editor.Bootstrap
{
    /// <summary>
    /// Registers uWED core's Editor-side services with ServiceLocator.
    /// EnsureInitialized() is public so any extension assembly (uWED.Acknex or others) can call it from
    /// its own [InitializeOnLoad] static constructor before registering its own services - Unity gives
    /// no ordering guarantee between different classes' static constructors, so an extension cannot
    /// assume this one has already run on its own.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorBootstrap
    {
        static EditorBootstrap()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Registers uWED core's services if they aren't already present. Checks ServiceLocator's
        /// actual current state rather than a one-shot flag, so it self-heals if something clears
        /// ServiceLocator after the initial registration - safe to call repeatedly, from anywhere.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (ServiceLocator.TryGet<IPrefsProvider>(out _))
                return;

            ServiceLocator.Clear();

            ServiceLocator.Register<IPrefsProvider>(new EditorPrefsProvider());
            ServiceLocator.Register<IFileDialog>(new EditorFileDialog());
            ServiceLocator.Register<IDefaultsProvider>(new EditorDefaultsProvider());
        }
    }
}
