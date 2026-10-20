using System;
using System.Collections.Generic;
using UnityEditor;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>Unity Inspector for a RegionTemplate, showing the groups of RegionTemplateLayout.</summary>
    [CustomEditor(typeof(RegionTemplate))]
    public class RegionTemplateInspector : TemplateInspector<RegionTemplate>
    {
        /// <summary>The Region layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => RegionTemplateLayout.Groups;

        /// <summary>The map data using RegionTemplates.</summary>
        protected override Type HighlightedDataType => typeof(Region);
    }
}
