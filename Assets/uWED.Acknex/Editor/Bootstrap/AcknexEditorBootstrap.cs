using UnityEditor;
using uWED.Editor.Bootstrap;
using uWED.Runtime.Platform;
using uWED.Acknex.Runtime.Registry;
using uWED.Acknex.Editor.Platform;

namespace uWED.Acknex.Editor.Bootstrap
{
    /// <summary>
    /// Registers uWED.Acknex's Editor-side services with ServiceLocator.
    /// </summary>
    [InitializeOnLoad]
    public static class AcknexEditorBootstrap
    {
        static AcknexEditorBootstrap()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Calls EditorBootstrap.EnsureInitialized() first, guaranteeing core's registration has run
        /// regardless of which [InitializeOnLoad] class Unity constructs first, then registers Acknex's
        /// own services the same self-healing way: by checking whether they're actually still present
        /// in ServiceLocator rather than a one-shot flag, so a later ServiceLocator.Clear() from
        /// anywhere gets repaired the next time this runs. Safe to call repeatedly.
        /// </summary>
        public static void EnsureInitialized()
        {
            EditorBootstrap.EnsureInitialized();

            if (ServiceLocator.TryGet<IAssetStorageService>(out _))
                return;

            ServiceLocator.Register<IAssetStorageService>(new EditorAssetStorageService());
        }
    }
}
