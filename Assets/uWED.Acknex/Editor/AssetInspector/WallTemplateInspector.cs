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
    /// <summary>Unity Inspector for a WallTemplate, showing the groups of WallTemplateLayout.</summary>
    [CustomEditor(typeof(WallTemplate))]
    public class WallTemplateInspector : TemplateInspector
    {
        /// <summary>The Wall layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => WallTemplateLayout.Groups;

        /// <summary>Walls are Segments.</summary>
        protected override Type HighlightedDataType => typeof(Segment);

        /// <summary>Matches Segment names resolving to the inspected WallTemplate, by the same rule the
        /// resolver uses (empty name = default Template, other names sanitized).</summary>
        protected override Func<string, bool> CreateNameMatcher()
        {
            if (!ServiceLocator.TryGet<TemplateResolver<WallTemplate>>(out var resolver))
                return null;

            var template = (WallTemplate)target;
            return name => resolver.Matches(name, template);
        }
    }
}
