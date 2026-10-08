using System.Collections.Generic;
using UnityEngine;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Common base for every Template type whose Instance is placed map geometry (Wall/Thing/Actor/
    /// Region) - mirrors AcknexCSApi's MapObject&lt;T&gt;, which gives BaseObject/Region/Way eight
    /// generic Flag1..Flag8 slots identically. Skill1..Skill8, the other half of MapObject&lt;T&gt;'s
    /// shared surface, are not modeled - see 00-conventions.md's Skill deferral note.
    ///
    /// m_flags is declared here, not in each subclass, because every flag a concrete Template exposes -
    /// Flag1..8 here plus whatever named flags BaseObjectTemplate/RegionTemplate add - packs into one
    /// physical uint32 per instance, mirroring how AcknexCSApi's real object holds them, rather than one
    /// field per base class in the hierarchy.
    /// </summary>
    public abstract class MapObjectTemplate : TemplateAsset
    {
        [SerializeField] protected uint m_flags;

        /// <summary>Names of the WDL function/label callback properties (If_*, Each_*) currently set on
        /// this Template and meaningful for its type - empty if none. Backs HasActionProperties and the
        /// action-indicator icon's tooltip.</summary>
        public abstract IReadOnlyList<string> PopulatedActionProperties { get; }

        /// <summary>True if at least one callback property is set (see PopulatedActionProperties) - drives
        /// the action-indicator icon on a Template picker's detail row.</summary>
        public bool HasActionProperties => PopulatedActionProperties.Count > 0;

        /// <summary>Returns the names of those entries whose value is not null/empty, in the given order.</summary>
        protected static List<string> CollectPopulated(params (string name, string value)[] properties)
        {
            var populated = new List<string>();
            foreach (var (name, value) in properties)
            {
                if (!string.IsNullOrEmpty(value))
                    populated.Add(name);
            }
            return populated;
        }

        /// <summary>Generic user-assignable flag slot 1, independent of every named flag on this type.</summary>
        public bool Flag1 { get => m_flags.IsSet(AcknexFlag.Flag1); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag1) : m_flags.Reset(AcknexFlag.Flag1); }
        /// <summary>Generic user-assignable flag slot 2, independent of every named flag on this type.</summary>
        public bool Flag2 { get => m_flags.IsSet(AcknexFlag.Flag2); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag2) : m_flags.Reset(AcknexFlag.Flag2); }
        /// <summary>Generic user-assignable flag slot 3, independent of every named flag on this type.</summary>
        public bool Flag3 { get => m_flags.IsSet(AcknexFlag.Flag3); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag3) : m_flags.Reset(AcknexFlag.Flag3); }
        /// <summary>Generic user-assignable flag slot 4, independent of every named flag on this type.</summary>
        public bool Flag4 { get => m_flags.IsSet(AcknexFlag.Flag4); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag4) : m_flags.Reset(AcknexFlag.Flag4); }
        /// <summary>Generic user-assignable flag slot 5, independent of every named flag on this type.</summary>
        public bool Flag5 { get => m_flags.IsSet(AcknexFlag.Flag5); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag5) : m_flags.Reset(AcknexFlag.Flag5); }
        /// <summary>Generic user-assignable flag slot 6, independent of every named flag on this type.</summary>
        public bool Flag6 { get => m_flags.IsSet(AcknexFlag.Flag6); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag6) : m_flags.Reset(AcknexFlag.Flag6); }
        /// <summary>Generic user-assignable flag slot 7, independent of every named flag on this type.</summary>
        public bool Flag7 { get => m_flags.IsSet(AcknexFlag.Flag7); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag7) : m_flags.Reset(AcknexFlag.Flag7); }
        /// <summary>Generic user-assignable flag slot 8, independent of every named flag on this type.</summary>
        public bool Flag8 { get => m_flags.IsSet(AcknexFlag.Flag8); set => m_flags = value ? m_flags.Set(AcknexFlag.Flag8) : m_flags.Reset(AcknexFlag.Flag8); }
    }
}
