using System;
using System.Collections.Generic;
using UnityEditor;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>Unity Inspector for a ThingTemplate, showing the groups of ThingTemplateLayout.</summary>
    [CustomEditor(typeof(ThingTemplate))]
    public class ThingTemplateInspector : TemplateInspector<ThingTemplate>
    {
        /// <summary>The Thing layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => ThingTemplateLayout.Groups;

        /// <summary>Things are MapObjects.</summary>
        protected override Type HighlightedDataType => typeof(MapObject);
    }
}
