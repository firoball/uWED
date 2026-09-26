using UnityEngine;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Acknex Thing properties, on top of the Wall/Thing/Actor-shared set on BaseObjectTemplate (see
    /// thing.md). `Overlay` is not modeled - its resulting type (a UI-namespace Overlay reference) is
    /// unresolved in the source data. `Height` has no property of its own - AcknexCSApi's `Height` and
    /// `Thing_hgt` are aliases for the same value, modeled here as the single Thing_hgt property.
    /// </summary>
    public class ThingTemplate : BaseObjectTemplate
    {
        [SerializeField] float m_thingHgt;
        [SerializeField] float m_sizeY;

        /// <summary>Thing height. Same value as AcknexCSApi's Height alias.</summary>
        public float Thing_hgt { get => m_thingHgt; set => m_thingHgt = value; }
        /// <summary>Vertical size.</summary>
        public float Size_y { get => m_sizeY; set => m_sizeY = value; }

        /// <summary>Ground flag.</summary>
        public bool Ground { get => m_flags.IsSet(AcknexFlag.Ground); set => m_flags = value ? m_flags.Set(AcknexFlag.Ground) : m_flags.Reset(AcknexFlag.Ground); }
        /// <summary>Candelaber flag.</summary>
        public bool Candelaber { get => m_flags.IsSet(AcknexFlag.Candelaber); set => m_flags = value ? m_flags.Set(AcknexFlag.Candelaber) : m_flags.Reset(AcknexFlag.Candelaber); }
        /// <summary>Flat flag.</summary>
        public bool Flat { get => m_flags.IsSet(AcknexFlag.Flat); set => m_flags = value ? m_flags.Set(AcknexFlag.Flat) : m_flags.Reset(AcknexFlag.Flat); }
    }
}
