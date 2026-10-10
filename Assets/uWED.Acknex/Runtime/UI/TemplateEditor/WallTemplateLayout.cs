using System.Collections.Generic;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// Which WallTemplate properties are shown, and how they are grouped. Shared by every host of the
    /// Template fields (Manipulator tabs, Inspector). If_arrived is left out: it has no meaning on a Wall.
    /// </summary>
    public static class WallTemplateLayout
    {
        /// <summary>The property groups in display order.</summary>
        public static readonly IReadOnlyList<TemplatePropertyGroup> Groups = new[]
        {
            new TemplatePropertyGroup("Properties", new[] { "Map_color", "Dist" }),
            new TemplatePropertyGroup("Flags", new[]
            {
                "Flag1", "Flag2", "Flag3", "Flag4", "Flag5", "Flag6", "Flag7", "Flag8",
                "Far", "Master", "Save", "Passable", "Impassable", "Invisible", "Berkeley", "Seen",
                "Fragile", "Sensitive", "Immaterial", "Liber", "Transparent", "Curtain", "Portcullis", "Fence",
            }, columns: 3, clearAllButton: true),
            new TemplatePropertyGroup("Actions", new[] { "If_near", "If_far", "If_hit", "Each_cycle", "Each_tick" }),
        };
    }
}
