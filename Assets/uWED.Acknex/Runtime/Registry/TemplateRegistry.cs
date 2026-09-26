using System.Collections.Generic;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.Registry
{
    /// <summary>
    /// One per Template type. Keyed by Name (the WDL identifier, not the
    /// backing .asset's Unity filename - see TemplateAsset). Choices is a
    /// live, mutable list handed directly to a GenericComboBoxField&lt;T&gt;
    /// Template picker - that control's own +/- mutate it in place, so this
    /// must be the same list instance every time, never a fresh snapshot.
    /// </summary>
    public class TemplateRegistry<T> where T : TemplateAsset
    {
        readonly IAssetStorageService m_storage;
        readonly Dictionary<string, T> m_byName = new();
        readonly List<T> m_choices = new();

        /// <summary>Every registered T, keyed by Name.</summary>
        public IReadOnlyDictionary<string, T> ByName => m_byName;

        /// <summary>Live, mutable view of every registered T - the same list instance across calls.</summary>
        public IList<T> Choices => m_choices;

        /// <summary>Creates the registry and immediately loads its current contents from storage.</summary>
        public TemplateRegistry(IAssetStorageService storage)
        {
            m_storage = storage;
            Reload();
        }

        /// <summary>Looks up a registered T by Name, or null if none is registered under that name.</summary>
        public T Get(string name) => m_byName.TryGetValue(name, out var template) ? template : null;

        /// <summary>Re-reads every T from storage, rebuilding both views from scratch.</summary>
        public void Reload()
        {
            m_byName.Clear();
            m_choices.Clear();

            foreach (var template in m_storage.LoadAll<T>())
            {
                m_byName[template.Name] = template;
                m_choices.Add(template);
            }
        }

        /// <summary>
        /// Clones source under newName (sanitized; auto-generated as
        /// "&lt;source.Name&gt;__copy", "__copy2", ... if null/empty), registers
        /// the clone, and mutates Choices in place so an open picker sees it
        /// immediately. This is the "Make Unique" action.
        /// </summary>
        public T Clone(T source, string newName)
        {
            string sanitized = string.IsNullOrEmpty(newName)
                ? UniqueCopyName(source.Name)
                : NameSanitizer.Sanitize(newName);

            var clone = m_storage.Clone(source, sanitized);
            clone.Name = sanitized;

            m_byName[sanitized] = clone;
            m_choices.Add(clone);
            return clone;
        }

        string UniqueCopyName(string baseName)
        {
            string candidate = NameSanitizer.Sanitize($"{baseName}__copy");
            int suffix = 2;
            while (m_byName.ContainsKey(candidate))
                candidate = NameSanitizer.Sanitize($"{baseName}__copy{suffix++}");
            return candidate;
        }
    }
}
