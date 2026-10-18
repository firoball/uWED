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
    /// <summary>Unity Inspector for an ActorTemplate, showing the groups of ActorTemplateLayout.</summary>
    [CustomEditor(typeof(ActorTemplate))]
    public class ActorTemplateInspector : TemplateInspector
    {
        /// <summary>The Actor layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => ActorTemplateLayout.Groups;

        /// <summary>Actors are MapObjects.</summary>
        protected override Type HighlightedDataType => typeof(MapObject);

        /// <summary>Matches MapObject names resolving to the inspected ActorTemplate, by the same rule the
        /// resolver uses (empty name = default Template, other names sanitized).</summary>
        protected override Func<string, bool> CreateNameMatcher()
        {
            if (!ServiceLocator.TryGet<TemplateResolver<ActorTemplate>>(out var resolver))
                return null;

            var template = (ActorTemplate)target;
            return name => resolver.Matches(name, template);
        }
    }
}
