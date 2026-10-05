using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// uWED.Acknex's IMapObjectManipulatorProvider - registered with ServiceLocator by
    /// AcknexEditorBootstrap so ManipulatorBinder opens an AcknexMapObjectManipulator for every
    /// MapObject instead of the plain base MapObjectManipulator.
    /// </summary>
    public class AcknexMapObjectManipulatorProvider : IMapObjectManipulatorProvider
    {
        readonly TemplateRegistry<ThingTemplate> m_thingRegistry;
        readonly TemplateResolver<ThingTemplate> m_thingResolver;
        readonly TemplateRegistry<ActorTemplate> m_actorRegistry;
        readonly TemplateResolver<ActorTemplate> m_actorResolver;

        /// <summary>Every AcknexMapObjectManipulator this provider creates shares these same live collaborators.</summary>
        public AcknexMapObjectManipulatorProvider(
            TemplateRegistry<ThingTemplate> thingRegistry,
            TemplateResolver<ThingTemplate> thingResolver,
            TemplateRegistry<ActorTemplate> actorRegistry,
            TemplateResolver<ActorTemplate> actorResolver)
        {
            m_thingRegistry = thingRegistry;
            m_thingResolver = thingResolver;
            m_actorRegistry = actorRegistry;
            m_actorResolver = actorResolver;
        }

        /// <summary>Returns an AcknexMapObjectManipulator wired to this provider's registries and resolvers.</summary>
        public MapObjectManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings)
            => new AcknexMapObjectManipulator(uxml, settings, m_thingRegistry, m_thingResolver, m_actorRegistry, m_actorResolver);
    }
}
