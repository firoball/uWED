using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// uWED.Acknex's ISegmentManipulatorProvider - registered with ServiceLocator by
    /// AcknexEditorBootstrap so ManipulatorBinder opens a WallManipulator for every Segment instead of
    /// the plain base SegmentManipulator. Unconditional, not per-Segment: every Segment in an
    /// Acknex-configured project resolves to a WallTemplate via TemplateResolver&lt;WallTemplate&gt;
    /// (which creates a default one on the spot if nothing matches yet), so there is nothing to branch on
    /// here.
    /// </summary>
    public class AcknexSegmentManipulatorProvider : ISegmentManipulatorProvider
    {
        readonly TemplateRegistry<WallTemplate> m_templateRegistry;
        readonly TemplateResolver<WallTemplate> m_templateResolver;

        /// <summary>Every WallManipulator this provider creates shares these same live collaborators.</summary>
        public AcknexSegmentManipulatorProvider(TemplateRegistry<WallTemplate> templateRegistry, TemplateResolver<WallTemplate> templateResolver)
        {
            m_templateRegistry = templateRegistry;
            m_templateResolver = templateResolver;
        }

        /// <summary>Returns a WallManipulator wired to this provider's TemplateRegistry and TemplateResolver.</summary>
        public SegmentManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings)
            => new WallManipulator(uxml, settings, m_templateRegistry, m_templateResolver);
    }
}
