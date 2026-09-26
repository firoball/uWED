using System;

namespace uWED.Acknex.Runtime.Model
{
    /// <summary>
    /// Named bits shared across every flag-backed WDL type (Wall/Thing/Actor/Region/Texture/Palette/...).
    /// Mirrors AcknexCSApi's A3Flags: one 32-bit space where the same bit can mean something different
    /// per type. Each flag-backed class keeps exactly one uint32 field for every flag it exposes -
    /// including its inherited Flag1..Flag8 slots, where it has them - and exposes named bool properties
    /// for whichever bits are legal on it, via the IsSet/Set/Reset extension methods below.
    ///
    /// A numeric value below is reused across different groups only where those groups' classes never
    /// combine into the same physical field (e.g. Palette.Hard and MapObjectTemplate.Flag1 both happen
    /// to be bit 0 - harmless, since Palette isn't MapObjectTemplate-derived and keeps its own separate
    /// field). Within a single field - MapObjectTemplate's Flag1..8 plus BaseObjectTemplate's own flags
    /// plus whichever concrete type's (Wall/Thing/Actor) own flags, all together, since they share one
    /// physical field per concrete Template instance - every bit is unique, never reused. Region's own
    /// field (Flag1..8 plus Region's own flags) is likewise internally unique but entirely separate from
    /// the Wall/Thing/Actor field. A value is reused for the *same* name across groups (Save) exactly
    /// where that name denotes one shared concept legal on several types, per the WDL data. Bit
    /// positions are not verified against Acknex's real WDL Property Reach bit layout - treat any given
    /// position as an internal implementation detail, not something to rely on for interop with real
    /// Acknex data.
    /// </summary>
    [Flags]
    public enum AcknexFlag : uint
    {
        /// <summary>No bits set.</summary>
        None = 0,

        // Palette.m_flags
        /// <summary>Palette.Hard.</summary>
        Hard = 1u << 0,
        /// <summary>Palette.Autorange.</summary>
        Autorange = 1u << 1,
        /// <summary>Palette.Blur.</summary>
        Blur = 1u << 2,

        // TextureTemplate.m_flags. Oneshot has no bit here - it's hidden/runtime-only, not a Template property.
        /// <summary>TextureTemplate.Ghost.</summary>
        Ghost = 1u << 3,
        /// <summary>TextureTemplate.Diaphanous.</summary>
        Diaphanous = 1u << 4,
        /// <summary>TextureTemplate.Behind.</summary>
        Behind = 1u << 5,
        /// <summary>TextureTemplate.Shadow.</summary>
        Shadow = 1u << 6,
        /// <summary>TextureTemplate.Lightmap.</summary>
        Lightmap = 1u << 7,
        /// <summary>TextureTemplate.Sky.</summary>
        Sky = 1u << 8,
        /// <summary>TextureTemplate.Wire.</summary>
        Wire = 1u << 9,
        /// <summary>TextureTemplate.Cluster.</summary>
        Cluster = 1u << 10,
        /// <summary>TextureTemplate.No_clip.</summary>
        No_clip = 1u << 11,
        /// <summary>TextureTemplate.Clip.</summary>
        Clip = 1u << 12,
        /// <summary>TextureTemplate.Sloop.</summary>
        Sloop = 1u << 13,
        /// <summary>TextureTemplate.Condensed.</summary>
        Condensed = 1u << 14,
        /// <summary>TextureTemplate.Narrow.</summary>
        Narrow = 1u << 15,
        /// <summary>Shared "Save" concept: TextureTemplate, BaseObjectTemplate (Wall/Thing/Actor), and RegionTemplate each expose their own Save property backed by this same bit, in their own field.</summary>
        Save = 1u << 16,

        // MapObjectTemplate.m_flags, bits 0-7 - the generic Flag1..Flag8 slots. This is the SAME field
        // every MapObjectTemplate-derived class (Wall/Thing/Actor/Region) uses for its own named flags
        // too, one field per concrete class - see the Wall/Thing/Actor group and the Region group below,
        // both of which continue numbering from bit 8 onward for exactly this reason.
        /// <summary>MapObjectTemplate.Flag1.</summary>
        Flag1 = 1u << 0,
        /// <summary>MapObjectTemplate.Flag2.</summary>
        Flag2 = 1u << 1,
        /// <summary>MapObjectTemplate.Flag3.</summary>
        Flag3 = 1u << 2,
        /// <summary>MapObjectTemplate.Flag4.</summary>
        Flag4 = 1u << 3,
        /// <summary>MapObjectTemplate.Flag5.</summary>
        Flag5 = 1u << 4,
        /// <summary>MapObjectTemplate.Flag6.</summary>
        Flag6 = 1u << 5,
        /// <summary>MapObjectTemplate.Flag7.</summary>
        Flag7 = 1u << 6,
        /// <summary>MapObjectTemplate.Flag8.</summary>
        Flag8 = 1u << 7,

        // BaseObjectTemplate's own flags, continuing in the SAME field as Flag1..8 above (bits 8+) -
        // shared by Wall/Thing/Actor, each in its own field instance. Far and Liber are included despite
        // each being absent from one of wall.md's/thing.md's own flag tables - both docs describe them
        // as legal on Wall, Thing, and Actor alike, so the WTA scope tag is treated as authoritative
        // over which single table happened to list them.
        /// <summary>BaseObjectTemplate.Far.</summary>
        Far = 1u << 8,
        /// <summary>BaseObjectTemplate.Master.</summary>
        Master = 1u << 9,
        /// <summary>BaseObjectTemplate.Passable.</summary>
        Passable = 1u << 10,
        /// <summary>BaseObjectTemplate.Impassable.</summary>
        Impassable = 1u << 11,
        /// <summary>BaseObjectTemplate.Invisible.</summary>
        Invisible = 1u << 12,
        /// <summary>BaseObjectTemplate.Berkeley.</summary>
        Berkeley = 1u << 13,
        /// <summary>BaseObjectTemplate.Seen.</summary>
        Seen = 1u << 14,
        /// <summary>BaseObjectTemplate.Fragile.</summary>
        Fragile = 1u << 15,
        // bit 16 is Save (declared above, reused here as the same concept).
        /// <summary>BaseObjectTemplate.Sensitive.</summary>
        Sensitive = 1u << 17,
        /// <summary>BaseObjectTemplate.Immaterial.</summary>
        Immaterial = 1u << 18,
        /// <summary>BaseObjectTemplate.Liber.</summary>
        Liber = 1u << 19,

        // WallTemplate's own flags, continuing in the same field as Flag1..8 + BaseObjectTemplate's
        // above (bits 20+, within Wall's own field instance).
        /// <summary>WallTemplate.Transparent.</summary>
        Transparent = 1u << 20,
        /// <summary>WallTemplate.Curtain.</summary>
        Curtain = 1u << 21,
        /// <summary>WallTemplate.Portcullis.</summary>
        Portcullis = 1u << 22,
        /// <summary>WallTemplate.Fence.</summary>
        Fence = 1u << 23,

        // ThingTemplate's own flags, continuing in the same field as Flag1..8 + BaseObjectTemplate's
        // above (bits 20+, within Thing's - and, inherited, Actor's - own field instance). Numerically
        // overlapping WallTemplate's bits above is intentional - Wall and Thing never share a field.
        /// <summary>ThingTemplate.Ground.</summary>
        Ground = 1u << 20,
        /// <summary>ThingTemplate.Candelaber.</summary>
        Candelaber = 1u << 21,
        /// <summary>ThingTemplate.Flat.</summary>
        Flat = 1u << 22,

        // ActorTemplate's own flag, continuing in the same field as Flag1..8 + BaseObjectTemplate's +
        // ThingTemplate's above (Actor : Thing, so Actor's field contains all three groups already).
        /// <summary>ActorTemplate.Carefully.</summary>
        Carefully = 1u << 23,

        // RegionTemplate's own flags, continuing in the SAME field as Flag1..8 above (bits 8+, within
        // Region's own field instance - Region doesn't derive from BaseObjectTemplate, so none of the
        // Wall/Thing/Actor bits above apply here). Floor_ascend/Floor_lifted share one bit, and
        // Ceil_descend/Ceil_lifted share another - each pair is two WDL names for the same physical
        // flag, not two independent flags. Lifted has no bit of its own: it's a compound flag that
        // reads/writes Floor_lifted and Ceil_ascend together (never Ceil_lifted), so it isn't listed
        // here - see RegionTemplate.Lifted, which composes it from the two bits below directly.
        /// <summary>RegionTemplate.Floor_ascend - the same bit as RegionTemplate.Floor_lifted.</summary>
        Floor_ascend = 1u << 8,
        /// <summary>RegionTemplate.Floor_lifted - the same bit as RegionTemplate.Floor_ascend.</summary>
        Floor_lifted = 1u << 8,
        /// <summary>RegionTemplate.Ceil_ascend.</summary>
        Ceil_ascend = 1u << 9,
        /// <summary>RegionTemplate.Floor_descend.</summary>
        Floor_descend = 1u << 10,
        /// <summary>RegionTemplate.Ceil_descend - the same bit as RegionTemplate.Ceil_lifted.</summary>
        Ceil_descend = 1u << 11,
        /// <summary>RegionTemplate.Ceil_lifted - the same bit as RegionTemplate.Ceil_descend.</summary>
        Ceil_lifted = 1u << 11,
        /// <summary>RegionTemplate.Save_all.</summary>
        Save_all = 1u << 12,
        // bit 16 is Save (declared above, reused here as the same concept).
        /// <summary>RegionTemplate.Sticky.</summary>
        Sticky = 1u << 13,
        /// <summary>RegionTemplate.Base.</summary>
        Base = 1u << 14,
    }

    /// <summary>
    /// IsSet/Set/Reset over a plain uint32 flags field - mirrors uWED core's A3Object/A3Flags pattern
    /// without depending on it directly, since uWED.Acknex's flag space (AcknexFlag) is its own.
    /// </summary>
    public static class AcknexFlagExtensions
    {
        /// <summary>True if every bit of <paramref name="flag"/> is set in <paramref name="flags"/>.</summary>
        public static bool IsSet(this uint flags, AcknexFlag flag) => (flags & (uint)flag) != 0;

        /// <summary>Returns <paramref name="flags"/> with <paramref name="flag"/>'s bit(s) set.</summary>
        public static uint Set(this uint flags, AcknexFlag flag) => flags | (uint)flag;

        /// <summary>Returns <paramref name="flags"/> with <paramref name="flag"/>'s bit(s) cleared.</summary>
        public static uint Reset(this uint flags, AcknexFlag flag) => flags & ~(uint)flag;
    }
}
