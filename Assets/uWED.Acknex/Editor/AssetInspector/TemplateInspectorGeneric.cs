using System;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.Platform;

namespace uWED.Acknex.Editor.AssetInspector
{
    /// <summary>
    /// TemplateInspector for one Template type: finds the type's registry and builds the name matcher of
    /// the map highlight from its resolver. A concrete Inspector only names its layout groups and the kind
    /// of map data using the Template.
    /// </summary>
    /// <typeparam name="T">The inspected Template type.</typeparam>
    public abstract class TemplateInspector<T> : TemplateInspector where T : TemplateAsset
    {
        /// <summary>The registry of T from the ServiceLocator, or null if none is registered.</summary>
        protected override ITemplateRegistry Registry
            => ServiceLocator.TryGet<TemplateRegistry<T>>(out var registry) ? registry : null;

        /// <summary>Matches map data names resolving to the inspected Template, by the same rule the resolver
        /// uses (empty name = default Template, other names sanitized).</summary>
        protected override Func<string, bool> CreateNameMatcher()
        {
            if (!ServiceLocator.TryGet<TemplateResolver<T>>(out var resolver))
                return null;

            var template = (T)target;
            return name => resolver.Matches(name, template);
        }
    }
}
