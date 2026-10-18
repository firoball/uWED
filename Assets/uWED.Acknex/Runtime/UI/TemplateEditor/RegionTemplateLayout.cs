using System;
using System.Collections.Generic;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// Which RegionTemplate properties are shown, and how they are grouped. Shared by every host of the
    /// Template fields (Manipulator tabs, Inspector). Object references (Below, Genius, textures) are
    /// shown as disabled placeholders. The alias flags Floor_lifted and Ceil_lifted and the compound flag
    /// Lifted share bits with listed flags and are left out, so no two fields ever show the same bit; the
    /// Flags note explains the aliases.
    /// </summary>
    public static class RegionTemplateLayout
    {
        /// <summary>The property groups in display order.</summary>
        public static readonly IReadOnlyList<TemplatePropertyGroup> Groups = new[]
        {
            new TemplatePropertyGroup("Properties 1", new[]
            {
                "Floor_hgt", "Ceil_hgt", "Floor_angle", "Ceil_angle",
                "Floor_offs_x", "Floor_offs_y", "Ceil_offs_x", "Ceil_offs_y",
                "Ambient", "Clip_dist",
            }),
            new TemplatePropertyGroup("Properties 2", Array.Empty<string>(),
                placeholders: new[] { "Below", "Genius", "Floor_tex", "Ceil_tex", "Texture1", "Texture2", "Texture3", "Texture4" }),
            new TemplatePropertyGroup("Flags", new[]
            {
                "Flag1", "Flag2", "Flag3", "Flag4", "Flag5", "Flag6", "Flag7", "Flag8",
                "Floor_ascend", "Ceil_ascend", "Floor_descend", "Ceil_descend",
                "Save", "Save_all", "Sticky", "Base",
            }, columns: 3, clearAllButton: true,
                note: "WDL aliases: Floor_lifted = Floor_ascend, Ceil_lifted = Ceil_descend, Lifted = Floor_ascend + Ceil_ascend."),
            new TemplatePropertyGroup("Actions", new[] { "If_enter", "If_leave", "If_dive", "If_arise", "Each_cycle", "Each_tick" }),
        };
    }
}
