using UnityEngine;
using UnityEngine.UIElements;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Loads and caches uWED.Acknex's own Manipulator stylesheet (Resources/AcknexManipulator.uss,
    /// alongside this class) - separate from uWED core's, since these USS classes (Template picker
    /// detail rows, the action-indicator icon) are Acknex-specific and core has no reason to carry
    /// them. Shared by every Acknex-extended Manipulator (Wall/Thing/Actor/Region), loaded once and
    /// applied per instance via ApplyTo.
    /// </summary>
    public static class AcknexManipulatorStyles
    {
        const string ResourcePath = "AcknexManipulator";

        /// <summary>A Template picker's detail-mode row (thumbnail + info + action icon).</summary>
        public const string PickerDetailRow = "acknex-picker-detail-row";
        /// <summary>The info column (Template Name + Texture-info line) next to a detail row's thumbnail.</summary>
        public const string PickerDetailInfo = "acknex-picker-detail-info";
        /// <summary>The Template's own Name label within a detail row's info column - full-weight, to stand
        /// apart from the muted Texture-info line below it (see PickerTextureInfo).</summary>
        public const string PickerTemplateName = "acknex-picker-template-name";
        /// <summary>The compact Texture Name/Size/Scale line within a detail row's info column - muted, so
        /// it doesn't compete with the Template's own Name above it (see PickerTemplateName).</summary>
        public const string PickerTextureInfo = "acknex-picker-texture-info";
        /// <summary>The action-indicator icon shown when a Template has a WDL callback set.</summary>
        public const string PickerActionIcon = "acknex-picker-action-icon";

        static StyleSheet s_styleSheet;

        /// <summary>Adds this stylesheet to target's styleSheets, loading and caching it from Resources on
        /// first use. Safe to call once per Manipulator instance - re-adding the same cached StyleSheet is
        /// explicitly guarded against.</summary>
        public static void ApplyTo(VisualElement target)
        {
            s_styleSheet ??= Resources.Load<StyleSheet>(ResourcePath);
            if (s_styleSheet == null)
            {
                Debug.LogWarning($"AcknexManipulatorStyles: could not load '{ResourcePath}' from a Resources folder.");
                return;
            }

            if (!target.styleSheets.Contains(s_styleSheet))
                target.styleSheets.Add(s_styleSheet);
        }
    }
}
