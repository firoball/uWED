using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>Unity Inspector for a WayTemplate: no property groups, only the highlight buttons and a note.</summary>
    [CustomEditor(typeof(WayTemplate))]
    public class WayTemplateInspector : TemplateInspector<WayTemplate>
    {
        /// <summary>A Way Template has no properties.</summary>
        protected override IReadOnlyList<TemplatePropertyGroup> Groups => Array.Empty<TemplatePropertyGroup>();

        /// <summary>Ways are Ways.</summary>
        protected override Type HighlightedDataType => typeof(Way);

        /// <summary>Adds the note below the (empty) base Inspector.</summary>
        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();
            root.Add(new HelpBox("A Way has no properties. This Template only provides its Name.", HelpBoxMessageType.Info));
            return root;
        }
    }
}
