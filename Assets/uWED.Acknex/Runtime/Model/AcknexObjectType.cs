namespace uWED.Acknex.Runtime.Model
{
    /// <summary>
    /// The kinds of map object a uWED MapObject can represent in Acknex. The numeric values are what is
    /// stored in IndexedData.TypeId; Thing is 0 so an object whose TypeId was never set is a Thing.
    /// </summary>
    public enum AcknexObjectType
    {
        /// <summary>Static or interactive map object, backed by a ThingTemplate.</summary>
        Thing = 0,

        /// <summary>Moving map object, backed by an ActorTemplate.</summary>
        Actor = 1,

        /// <summary>The player start. Has no Template; its Name is always "player".</summary>
        Player = 2,
    }
}
