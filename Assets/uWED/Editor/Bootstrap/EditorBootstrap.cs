using UnityEditor;
using uWED.Editor.Platform;
using uWED.Runtime.Platform;

namespace uWED.Editor.Bootstrap
{
    /// <summary>
    /// Registers uWED core's Editor-side services with ServiceLocator.
    /// EnsureInitialized() is public and idempotent so any extension
    /// (uWED.Acknex, future ones) can call it from its own [InitializeOnLoad]
    /// static constructor before registering its own services - Unity gives
    /// no ordering guarantee between different classes' static constructors,
    /// so an extension can't assume this one has already run.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorBootstrap
    {
        static EditorBootstrap()
        {
            EnsureInitialized();
        }

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
