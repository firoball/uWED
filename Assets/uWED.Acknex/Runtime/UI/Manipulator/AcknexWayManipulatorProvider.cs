using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>uWED.Acknex's IWayManipulatorProvider: opens an AcknexWayManipulator for every Way.</summary>
    public class AcknexWayManipulatorProvider : IWayManipulatorProvider
    {
        readonly TemplateRegistry<WayTemplate> m_templateRegistry;
        readonly TemplateResolver<WayTemplate> m_templateResolver;

        /// <summary>Every AcknexWayManipulator this provider creates shares these same live collaborators.</summary>
        public AcknexWayManipulatorProvider(TemplateRegistry<WayTemplate> templateRegistry, TemplateResolver<WayTemplate> templateResolver)
        {
            m_templateRegistry = templateRegistry;
            m_templateResolver = templateResolver;
        }

        /// <summary>Returns an AcknexWayManipulator wired to this provider's registry and resolver.</summary>
        public WayManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings)
            => new AcknexWayManipulator(uxml, settings, m_templateRegistry, m_templateResolver);
    }
}
