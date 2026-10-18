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
    /// <summary>Unity Inspector for a ThingTemplate, showing the groups of ThingTemplateLayout.</summary>
    [CustomEditor(typeof(ThingTemplate))]
    public class ThingTemplateInspector : TemplateInspector
    {
        /// <summary>The Thing layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => ThingTemplateLayout.Groups;

        /// <summary>Things are MapObjects.</summary>
        protected override Type HighlightedDataType => typeof(MapObject);

        /// <summary>Matches MapObject names resolving to the inspected ThingTemplate, by the same rule the
        /// resolver uses (empty name = default Template, other names sanitized).</summary>
        protected override Func<string, bool> CreateNameMatcher()
        {
            if (!ServiceLocator.TryGet<TemplateResolver<ThingTemplate>>(out var resolver))
                return null;

            var template = (ThingTemplate)target;
            return name => resolver.Matches(name, template);
        }
    }
}
