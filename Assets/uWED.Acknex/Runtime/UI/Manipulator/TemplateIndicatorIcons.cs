using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Indicator icons shown on a Template picker's detail row (action callbacks, stacked Region).
    /// Shared by the Wall, Region and Thing/Actor pickers.
    /// </summary>
    public static class TemplateIndicatorIcons
    {
        /// <summary>Lightning icon for a Template with at least one callback set; its tooltip lists the
        /// populated callback properties (see MapObjectTemplate.PopulatedActionProperties).</summary>
        public static Label BuildActionIcon(MapObjectTemplate template)
        {
            string tooltip = "WDL action callbacks set:\n" + string.Join("\n", template.PopulatedActionProperties);
            return BuildIcon("⚡", AcknexManipulatorStyles.PickerActionIcon, tooltip);
        }

        /// <summary>Glyph label with the given USS class and tooltip. Explicit alignSelf/unityTextAlign,
        /// rather than relying on the parent's alignItems, keeps the glyph centered regardless of how tall
        /// its container ends up next to a Name label's shorter line height.</summary>
        public static Label BuildIcon(string glyph, string className, string tooltip)
        {
            var icon = new Label(glyph) { style = { alignSelf = Align.Center, unityTextAlign = TextAnchor.MiddleCenter } };
            icon.AddToClassList(className);
            icon.tooltip = tooltip;
            return icon;
        }
    }
}
