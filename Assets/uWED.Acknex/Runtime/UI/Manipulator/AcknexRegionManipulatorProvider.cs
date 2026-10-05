using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// uWED.Acknex's IRegionManipulatorProvider - registered with ServiceLocator by
    /// AcknexEditorBootstrap so ManipulatorBinder opens an AcknexRegionManipulator for every Region
    /// instead of the plain base RegionManipulator. Unconditional, not per-Region: every Region in an
    /// Acknex-configured project resolves to a RegionTemplate via TemplateResolver&lt;RegionTemplate&gt;
    /// (which creates a default one on the spot if nothing matches yet), so there is nothing to branch on
    /// here.
    /// </summary>
    public class AcknexRegionManipulatorProvider : IRegionManipulatorProvider
    {
        readonly TemplateRegistry<RegionTemplate> m_templateRegistry;
        readonly TemplateResolver<RegionTemplate> m_templateResolver;

        /// <summary>Every AcknexRegionManipulator this provider creates shares these same live collaborators.</summary>
        public AcknexRegionManipulatorProvider(TemplateRegistry<RegionTemplate> templateRegistry, TemplateResolver<RegionTemplate> templateResolver)
        {
            m_templateRegistry = templateRegistry;
            m_templateResolver = templateResolver;
        }

        /// <summary>Returns an AcknexRegionManipulator wired to this provider's TemplateRegistry and TemplateResolver.</summary>
        public RegionManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings)
            => new AcknexRegionManipulator(uxml, settings, m_templateRegistry, m_templateResolver);
    }
}
