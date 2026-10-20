using System;
using System.Collections.Generic;
using UnityEditor;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>Unity Inspector for a WallTemplate, showing the groups of WallTemplateLayout.</summary>
    [CustomEditor(typeof(WallTemplate))]
    public class WallTemplateInspector : TemplateInspector<WallTemplate>
    {
        /// <summary>The Wall layout shared with the Manipulator tabs.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => WallTemplateLayout.Groups;

        /// <summary>Walls are Segments.</summary>
        protected override Type HighlightedDataType => typeof(Segment);
    }
}
