using System.Collections.Generic;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// Which ThingTemplate properties are shown, and how they are grouped. Shared by every host of the
    /// Template fields (Manipulator tabs, Inspector). If_arrived is left out: it has no meaning on a
    /// Thing. Texture and Attach are references and shown as disabled placeholders. ActorTemplateLayout
    /// builds on the flag and property lists below.
    /// </summary>
    public static class ThingTemplateLayout
    {
        /// <summary>Numeric properties of a Thing.</summary>
        public static readonly IReadOnlyList<string> NumericProperties = new[] { "Map_color", "Dist", "Thing_hgt", "Size_y" };

        /// <summary>Reference properties of a Thing and an Actor, shown as placeholders.</summary>
        public static readonly IReadOnlyList<string> ReferencePlaceholders = new[] { "Texture", "Attach" };

        /// <summary>Flag properties of a Thing.</summary>
        public static readonly IReadOnlyList<string> FlagProperties = new[]
        {
            "Flag1", "Flag2", "Flag3", "Flag4", "Flag5", "Flag6", "Flag7", "Flag8",
            "Far", "Master", "Save", "Passable", "Impassable", "Invisible", "Berkeley", "Seen",
            "Fragile", "Sensitive", "Immaterial", "Liber", "Ground", "Candelaber", "Flat",
        };

        /// <summary>The property groups in display order.</summary>
        public static readonly IReadOnlyList<TemplatePropertyGroup> Groups = new[]
        {
            new TemplatePropertyGroup("Properties", NumericProperties, placeholders: ReferencePlaceholders),
            new TemplatePropertyGroup("Flags", FlagProperties, columns: 3, clearAllButton: true),
            new TemplatePropertyGroup("Actions", new[] { "If_near", "If_far", "If_hit", "Each_cycle", "Each_tick" }),
        };
    }
}
