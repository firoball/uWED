using System;
using System.Collections.Generic;
using UnityEditor;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.Platform;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>Unity Inspector for a RegionTemplate, showing the groups of RegionTemplateLayout.</summary>
    [CustomEditor(typeof(RegionTemplate))]
    public class RegionTemplateInspector : TemplateInspector
    {
        /// <summary>The Region layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => RegionTemplateLayout.Groups;

        /// <summary>The map data using RegionTemplates.</summary>
        protected override Type HighlightedDataType => typeof(Region);

        /// <summary>Matches Region names resolving to the inspected RegionTemplate, by the same rule the
        /// resolver uses (empty name = default Template, other names sanitized).</summary>
        protected override Func<string, bool> CreateNameMatcher()
        {
            if (!ServiceLocator.TryGet<TemplateResolver<RegionTemplate>>(out var resolver))
                return null;

            var template = (RegionTemplate)target;
            return name => resolver.Matches(name, template);
        }
    }
}
