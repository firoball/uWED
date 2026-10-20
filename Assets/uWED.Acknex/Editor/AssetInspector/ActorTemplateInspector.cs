using System;
using System.Collections.Generic;
using UnityEditor;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>Unity Inspector for an ActorTemplate, showing the groups of ActorTemplateLayout.</summary>
    [CustomEditor(typeof(ActorTemplate))]
    public class ActorTemplateInspector : TemplateInspector<ActorTemplate>
    {
        /// <summary>The Actor layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => ActorTemplateLayout.Groups;

        /// <summary>Actors are MapObjects.</summary>
        protected override Type HighlightedDataType => typeof(MapObject);
    }
}
