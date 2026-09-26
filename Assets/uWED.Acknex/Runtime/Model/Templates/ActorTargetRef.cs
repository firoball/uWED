using System;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// The subset of AcknexCSApi's ActorTarget enum selectable directly as an ActorTargetRef's Mode.
    /// The remaining ActorTarget values (Null/Move/Bullet/Drop/Stick/Straight/Vertex/Place) are
    /// runtime/command-driven states, not selectable at definition time.
    /// </summary>
    public enum ActorTargetMode
    {
        Follow,
        Repel,
        Node0,
        Node1,
        Hold,
    }

    /// <summary>Which of ActorTargetRef's three fields is the active one.</summary>
    public enum ActorTargetKind
    {
        /// <summary>Mode is the active field.</summary>
        Mode,
        /// <summary>WayName is the active field.</summary>
        Way,
        /// <summary>ObjectName is the active field.</summary>
        Object,
    }

    /// <summary>
    /// An Actor's Target - a three-way variant (a fixed Mode, a specific Way by name, or a specific
    /// Thing/Actor by name), mirroring AcknexCSApi's own tagged-union BaseObject.Target with an explicit
    /// Kind discriminator instead of a runtime-type-switched object, so it serializes as a plain
    /// [Serializable] value without needing [SerializeReference].
    /// </summary>
    [Serializable]
    public class ActorTargetRef
    {
        /// <summary>Which field below is active.</summary>
        public ActorTargetKind Kind;
        /// <summary>The selected mode. Meaningful when Kind == Mode.</summary>
        public ActorTargetMode Mode;
        /// <summary>Name of the targeted Way. Meaningful when Kind == Way.</summary>
        public string WayName;
        /// <summary>Name of the targeted Thing/Actor Template. Meaningful when Kind == Object.</summary>
        public string ObjectName;
    }
}
