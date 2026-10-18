using System.Collections.Generic;
using System.Linq;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// Which ActorTemplate properties are shown, and how they are grouped: everything of
    /// ThingTemplateLayout plus the Actor's own properties, flag and If_arrived. Shared by every host of
    /// the Template fields (Manipulator tabs, Inspector). Texture, Attach and Target are references and
    /// shown as disabled placeholders.
    /// </summary>
    public static class ActorTemplateLayout
    {
        /// <summary>The property groups in display order.</summary>
        public static readonly IReadOnlyList<TemplatePropertyGroup> Groups = new[]
        {
            new TemplatePropertyGroup("Properties 1", ThingTemplateLayout.NumericProperties
                .Concat(new[] { "Waypoint", "Target_x", "Target_y", "Speed", "Vspeed", "Aspeed" }).ToArray()),
            new TemplatePropertyGroup("Properties 2", System.Array.Empty<string>(),
                placeholders: ThingTemplateLayout.ReferencePlaceholders.Concat(new[] { "Target" }).ToArray()),
            new TemplatePropertyGroup("Flags", ThingTemplateLayout.FlagProperties.Concat(new[] { "Carefully" }).ToArray(),
                columns: 3, clearAllButton: true),
            new TemplatePropertyGroup("Actions", new[] { "If_near", "If_far", "If_hit", "If_arrived", "Each_cycle", "Each_tick" }),
        };
    }
}
