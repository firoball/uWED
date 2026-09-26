using UnityEngine;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Acknex Actor properties, on top of everything ThingTemplate already provides (Actor : Thing at
    /// the Template level, mirroring how the design treats Actor as a specialization of Thing - in
    /// AcknexCSApi itself Wall/Thing/Actor are flat siblings). `Node` is not modeled - its resulting
    /// type is unresolved in the source data. `Waypoint`'s underlying type is ambiguous in the source
    /// (float or int) - modeled as float, matching every other Var-typed property.
    /// </summary>
    public class ActorTemplate : ThingTemplate
    {
        [SerializeField] ActorTargetRef m_target;
        [SerializeField] float m_waypoint;
        [SerializeField] float m_targetX;
        [SerializeField] float m_targetY;
        [SerializeField] float m_speed;
        [SerializeField] float m_vspeed;
        [SerializeField] float m_aspeed;

        /// <summary>Movement target - a fixed mode, a specific Way, or a specific Thing/Actor, never more than one at a time.</summary>
        public ActorTargetRef Target { get => m_target; set => m_target = value; }
        /// <summary>Waypoint index/value.</summary>
        public float Waypoint { get => m_waypoint; set => m_waypoint = value; }
        /// <summary>Target horizontal position.</summary>
        public float Target_x { get => m_targetX; set => m_targetX = value; }
        /// <summary>Target vertical position.</summary>
        public float Target_y { get => m_targetY; set => m_targetY = value; }
        /// <summary>Movement speed.</summary>
        public float Speed { get => m_speed; set => m_speed = value; }
        /// <summary>Vertical movement speed.</summary>
        public float Vspeed { get => m_vspeed; set => m_vspeed = value; }
        /// <summary>Angular/turning speed.</summary>
        public float Aspeed { get => m_aspeed; set => m_aspeed = value; }

        /// <summary>Carefully flag.</summary>
        public bool Carefully { get => m_flags.IsSet(AcknexFlag.Carefully); set => m_flags = value ? m_flags.Set(AcknexFlag.Carefully) : m_flags.Reset(AcknexFlag.Carefully); }
    }
}
