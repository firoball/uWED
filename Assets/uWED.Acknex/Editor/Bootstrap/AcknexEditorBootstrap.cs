using UnityEditor;
using uWED.Editor.Bootstrap;
using uWED.Runtime.Platform;
using uWED.Runtime.UI.Manipulator;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Acknex.Runtime.UI.Manipulator;
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
        /// anywhere gets repaired the next time this runs. Safe to call repeatedly. The single
        /// IAssetStorageService presence check guards every registration below, not just its own - they
        /// are only ever registered together in this one pass, so one implies the others are current too.
        /// ServiceLocator's registrations, and so each TemplateRegistry&lt;T&gt; instance, persist for the
        /// life of the Editor session - closing and reopening a uWED window runs this method again without
        /// a domain reload, so when everything is already registered this re-syncs the existing registries
        /// from disk (Reload(), in place - the same Choices list instance any already-open picker is bound
        /// to) rather than skipping outright, instead of building fresh ones the UI wouldn't be bound to.
        /// </summary>
        public static void EnsureInitialized()
        {
            EditorBootstrap.EnsureInitialized();

            if (ServiceLocator.TryGet<IAssetStorageService>(out _))
            {
                if (ServiceLocator.TryGet<TemplateRegistry<WallTemplate>>(out var existingWallTemplates))
                    existingWallTemplates.Reload();
                if (ServiceLocator.TryGet<TemplateRegistry<RegionTemplate>>(out var existingRegionTemplates))
                    existingRegionTemplates.Reload();
                return;
            }

            var storage = new EditorAssetStorageService();
            ServiceLocator.Register<IAssetStorageService>(storage);

            var wallTemplates = new TemplateRegistry<WallTemplate>(storage);
            var wallResolver = new TemplateResolver<WallTemplate>(wallTemplates);
            ServiceLocator.Register<TemplateRegistry<WallTemplate>>(wallTemplates);
            ServiceLocator.Register<TemplateResolver<WallTemplate>>(wallResolver);
            ServiceLocator.Register<ISegmentManipulatorProvider>(new AcknexSegmentManipulatorProvider(wallTemplates, wallResolver));

            var regionTemplates = new TemplateRegistry<RegionTemplate>(storage);
            var regionResolver = new TemplateResolver<RegionTemplate>(regionTemplates);
            ServiceLocator.Register<TemplateRegistry<RegionTemplate>>(regionTemplates);
            ServiceLocator.Register<TemplateResolver<RegionTemplate>>(regionResolver);
            ServiceLocator.Register<IRegionManipulatorProvider>(new AcknexRegionManipulatorProvider(regionTemplates, regionResolver));
        }
    }
}
