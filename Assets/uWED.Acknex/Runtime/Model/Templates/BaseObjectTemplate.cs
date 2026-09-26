using UnityEngine;
using uWED.Acknex.Runtime.Model.Instances;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Common base for Wall/Thing/Actor Templates - mirrors AcknexCSApi's BaseObject, which Wall/Thing/
    /// Actor all derive from as empty type-tag subclasses. Carries every property and flag legal on all
    /// three (see 00-conventions.md/wall.md/thing.md's "WTA" scope tag). ThingTemplate/WallTemplate add
    /// their own type-specific properties and flags on top of this.
    /// </summary>
    public abstract class BaseObjectTemplate : MapObjectTemplate
    {
        [SerializeReference] TextureInstance m_texture; // plain-class reference - see TextureInstance's own doc comment
        [SerializeReference] TextureInstance m_attach; // plain-class reference - see TextureInstance's own doc comment
        [SerializeField] int m_mapColor;
        [SerializeField] float m_dist;
        [SerializeField] string m_ifNear;
        [SerializeField] string m_ifFar;
        [SerializeField] string m_ifHit;
        [SerializeField] string m_ifArrived;
        [SerializeField] string m_eachCycle;
        [SerializeField] string m_eachTick;

        /// <summary>Texture this object renders with. Parallel to, not a replacement for, uWED core's own convenience texture-preview slot.</summary>
        public TextureInstance Texture { get => m_texture; set => m_texture = value; }
        /// <summary>Another Texture this object's Texture attaches to.</summary>
        public TextureInstance Attach { get => m_attach; set => m_attach = value; }
        /// <summary>Map/palette color index.</summary>
        public int Map_color { get => m_mapColor; set => m_mapColor = value; }
        /// <summary>Interaction distance.</summary>
        public float Dist { get => m_dist; set => m_dist = value; }
        /// <summary>WDL function/label name invoked when something comes near.</summary>
        public string If_near { get => m_ifNear; set => m_ifNear = value; }
        /// <summary>WDL function/label name invoked when something moves far away.</summary>
        public string If_far { get => m_ifFar; set => m_ifFar = value; }
        /// <summary>WDL function/label name invoked on hit.</summary>
        public string If_hit { get => m_ifHit; set => m_ifHit = value; }
        /// <summary>WDL function/label name invoked on arrival. Meaningful for Actor (movement target arrival); parseable but inert on Wall/Thing.</summary>
        public string If_arrived { get => m_ifArrived; set => m_ifArrived = value; }
        /// <summary>WDL function/label name invoked once per cycle.</summary>
        public string Each_cycle { get => m_eachCycle; set => m_eachCycle = value; }
        /// <summary>WDL function/label name invoked once per engine tick.</summary>
        public string Each_tick { get => m_eachTick; set => m_eachTick = value; }

        /// <summary>Far flag.</summary>
        public bool Far { get => m_flags.IsSet(AcknexFlag.Far); set => m_flags = value ? m_flags.Set(AcknexFlag.Far) : m_flags.Reset(AcknexFlag.Far); }
        /// <summary>Master flag.</summary>
        public bool Master { get => m_flags.IsSet(AcknexFlag.Master); set => m_flags = value ? m_flags.Set(AcknexFlag.Master) : m_flags.Reset(AcknexFlag.Master); }
        /// <summary>Save flag.</summary>
        public bool Save { get => m_flags.IsSet(AcknexFlag.Save); set => m_flags = value ? m_flags.Set(AcknexFlag.Save) : m_flags.Reset(AcknexFlag.Save); }
        /// <summary>Passable flag.</summary>
        public bool Passable { get => m_flags.IsSet(AcknexFlag.Passable); set => m_flags = value ? m_flags.Set(AcknexFlag.Passable) : m_flags.Reset(AcknexFlag.Passable); }
        /// <summary>Impassable flag.</summary>
        public bool Impassable { get => m_flags.IsSet(AcknexFlag.Impassable); set => m_flags = value ? m_flags.Set(AcknexFlag.Impassable) : m_flags.Reset(AcknexFlag.Impassable); }
        /// <summary>Invisible flag.</summary>
        public bool Invisible { get => m_flags.IsSet(AcknexFlag.Invisible); set => m_flags = value ? m_flags.Set(AcknexFlag.Invisible) : m_flags.Reset(AcknexFlag.Invisible); }
        /// <summary>Berkeley flag.</summary>
        public bool Berkeley { get => m_flags.IsSet(AcknexFlag.Berkeley); set => m_flags = value ? m_flags.Set(AcknexFlag.Berkeley) : m_flags.Reset(AcknexFlag.Berkeley); }
        /// <summary>Seen flag.</summary>
        public bool Seen { get => m_flags.IsSet(AcknexFlag.Seen); set => m_flags = value ? m_flags.Set(AcknexFlag.Seen) : m_flags.Reset(AcknexFlag.Seen); }
        /// <summary>Fragile flag.</summary>
        public bool Fragile { get => m_flags.IsSet(AcknexFlag.Fragile); set => m_flags = value ? m_flags.Set(AcknexFlag.Fragile) : m_flags.Reset(AcknexFlag.Fragile); }
        /// <summary>Sensitive flag.</summary>
        public bool Sensitive { get => m_flags.IsSet(AcknexFlag.Sensitive); set => m_flags = value ? m_flags.Set(AcknexFlag.Sensitive) : m_flags.Reset(AcknexFlag.Sensitive); }
        /// <summary>Immaterial flag.</summary>
        public bool Immaterial { get => m_flags.IsSet(AcknexFlag.Immaterial); set => m_flags = value ? m_flags.Set(AcknexFlag.Immaterial) : m_flags.Reset(AcknexFlag.Immaterial); }
        /// <summary>Liber flag.</summary>
        public bool Liber { get => m_flags.IsSet(AcknexFlag.Liber); set => m_flags = value ? m_flags.Set(AcknexFlag.Liber) : m_flags.Reset(AcknexFlag.Liber); }
    }
}
