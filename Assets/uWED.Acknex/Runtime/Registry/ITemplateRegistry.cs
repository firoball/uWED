using System;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.Registry
{
    /// <summary>
    /// Type-independent view of a TemplateRegistry&lt;T&gt;: the change notifications and Reload, for hosts
    /// that don't know the Template type (e.g. the Inspector base class).
    /// </summary>
    public interface ITemplateRegistry
    {
        /// <summary>Raised after a Template's values changed and were saved, or reported through NotifyChanged.</summary>
        event Action<TemplateAsset> TemplateChanged;

        /// <summary>Raised when Templates were added, removed or renamed, so the registry's Choices changed.</summary>
        event Action ListChanged;

        /// <summary>Re-reads every Template of the type from storage and raises ListChanged.</summary>
        void Reload();

        /// <summary>Reports that template's values changed, without saving it. For a host that persists the
        /// asset by its own means.</summary>
        void NotifyChanged(TemplateAsset template);
    }
}
