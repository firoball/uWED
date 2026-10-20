using System;
using System.Collections.Generic;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.Registry
{
    /// <summary>
    /// One per Template type. Keyed by Name (the WDL identifier, not the
    /// backing .asset's Unity filename - see TemplateAsset). Implements
    /// IGenericNameProvider&lt;T&gt; so a GenericComboBoxField&lt;T&gt; Template picker can be
    /// wired directly off a registry instead of duplicating Choices/ItemFactory/Sanitizer by
    /// hand - unrelated to any base Manipulator's own IGenericNameProvider&lt;string&gt; (that one
    /// is over plain Segment/Instance Names, this one is over T itself). Choices is a live,
    /// mutable list kept sorted alphabetically by Name - that control's own +/- mutate it in
    /// place, so this must be the same list instance every time, never a fresh snapshot.
    /// </summary>
    public class TemplateRegistry<T> : IGenericNameProvider<T>, ITemplateRegistry where T : TemplateAsset
    {
        readonly IAssetStorageService m_storage;
        readonly Dictionary<string, T> m_byName = new();
        readonly List<T> m_choices = new();

        /// <inheritdoc/>
        public event Action<TemplateAsset> TemplateChanged;

        /// <inheritdoc/>
        public event Action ListChanged;

        /// <summary>Every registered T, keyed by Name.</summary>
        public IReadOnlyDictionary<string, T> ByName => m_byName;

        /// <summary>Live, mutable view of every registered T - the same list instance across calls.</summary>
        public IList<T> Choices => m_choices;

        /// <summary>Builds an unpersisted, unregistered in-memory copy of previous (or a blank
        /// instance if previous is null) named text. Never touches storage or ByName/Choices - a
        /// GenericComboBoxField&lt;T&gt; calls this on every keystroke to test whether Add should be
        /// enabled, so it must stay side-effect-free; a confirmed Add is committed separately via Clone.</summary>
        public Func<string, T, T> ItemFactory { get; } = BuildTransientCandidate;

        /// <summary>Normalizes typed text into a legal WDL identifier before Add/selection logic sees it.</summary>
        public Func<string, string> Sanitizer { get; } = NameSanitizer.Sanitize;

        /// <summary>Creates the registry and immediately loads its current contents from storage.</summary>
        public TemplateRegistry(IAssetStorageService storage)
        {
            m_storage = storage;
            Reload();
        }

        /// <summary>Looks up a registered T by Name, or null if none is registered under that name. If the
        /// entry's backing asset was deleted outside this registry (e.g. via the Project window, between
        /// this session's last Reload and now), the stored reference is a destroyed Unity Object - null in
        /// every way that matters (Unity's own != null check already treats it as such) except that it
        /// would otherwise keep sitting in ByName/Choices forever, showing up as a second, "missing" copy
        /// alongside whatever a caller creates next under the same Name. Evicting it here, lazily, on the
        /// first lookup that notices, means a caller never has to force a full Reload just to keep this
        /// registry accurate between deletions.</summary>
        public T Get(string name)
        {
            if (!m_byName.TryGetValue(name, out var template))
                return null;

            if (template != null)
                return template;

            m_byName.Remove(name);
            m_choices.Remove(template);
            return null;
        }

        /// <summary>Persists in-memory changes made to a registered template via the storage service, then
        /// raises TemplateChanged.</summary>
        public void MarkDirty(T template)
        {
            m_storage.MarkDirty(template);
            NotifyChanged(template);
        }

        /// <summary>Raises TemplateChanged for template without saving it.</summary>
        public void NotifyChanged(T template) => TemplateChanged?.Invoke(template);

        void ITemplateRegistry.NotifyChanged(TemplateAsset template) => NotifyChanged((T)template);

        /// <summary>Starts a batch on the underlying storage if it supports IBatchableAssetStorage
        /// (e.g. many Create calls in a row - see TemplateResolver.ResolveAll), otherwise a no-op.</summary>
        public void BeginBatch() => (m_storage as IBatchableAssetStorage)?.BeginBatch();

        /// <summary>Ends a batch started by BeginBatch - see there.</summary>
        public void EndBatch() => (m_storage as IBatchableAssetStorage)?.EndBatch();

        /// <summary>
        /// Creates and registers a brand-new blank T under name (sanitized into a legal WDL identifier
        /// via NameSanitizer, same as Clone) via storage, mutating ByName/Choices in place. Unconditional
        /// - always creates, even if name is already registered - so this is purely the mechanical "make
        /// one and register it" operation; whether a fresh T should be created at all for a given name
        /// (e.g. "only if none exists yet, because nothing matched this Instance's Name") is a per-type
        /// policy decision that belongs to that type's own resolver (see TemplateResolver), not to this
        /// generic registry.
        /// </summary>
        public T Create(string name)
        {
            var template = Register(m_storage.Create<T>(NameSanitizer.Sanitize(name)));
            ListChanged?.Invoke();
            return template;
        }

        /// <summary>Re-reads every T from storage, rebuilding both views from scratch and raising
        /// ListChanged. If several stored Templates share a Name, the first one loaded is registered and
        /// the others are skipped with a warning.</summary>
        public void Reload()
        {
            m_byName.Clear();
            m_choices.Clear();

            foreach (var template in m_storage.LoadAll<T>())
            {
                if (m_byName.ContainsKey(template.Name))
                {
                    Debug.LogWarning($"TemplateRegistry<{typeof(T).Name}>: more than one Template is named '{template.Name}'; '{template.name}' is ignored.", template);
                    continue;
                }

                Register(template);
            }

            ListChanged?.Invoke();
        }

        /// <summary>Gets the Name template is registered under, or false if it is not registered. This can
        /// differ from template.Name if the Name was changed after registration.</summary>
        public bool TryGetRegisteredName(T template, out string name)
        {
            foreach (var entry in m_byName)
            {
                if (entry.Value == template)
                {
                    name = entry.Key;
                    return true;
                }
            }

            name = null;
            return false;
        }

        /// <summary>Returns baseName (sanitized) if no registered T has that Name, otherwise baseName with
        /// the first free number appended ("name2", "name3", ...).</summary>
        public string UniqueName(string baseName)
        {
            string sanitized = NameSanitizer.Sanitize(baseName);
            string candidate = sanitized;
            int suffix = 2;
            while (m_byName.ContainsKey(candidate))
                candidate = NameSanitizer.Sanitize($"{sanitized}{suffix++}");
            return candidate;
        }

        /// <summary>Gives template the (sanitized) Name newName through storage, which also renames its
        /// backing entry, and registers it under that Name. The caller guarantees the Name is free
        /// (see UniqueName). Raises ListChanged.</summary>
        public void Rename(T template, string newName)
        {
            if (m_byName.TryGetValue(template.Name, out var registered) && registered == template)
                m_byName.Remove(template.Name);
            m_choices.Remove(template);

            m_storage.Rename(template, NameSanitizer.Sanitize(newName));
            Register(template);
            ListChanged?.Invoke();
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
                ? UniqueName($"{source.Name}__copy")
                : NameSanitizer.Sanitize(newName);

            var clone = m_storage.Clone(source, sanitized);
            clone.Name = sanitized;

            Register(clone);
            ListChanged?.Invoke();
            return clone;
        }

        /// <summary>Adds template to ByName (keyed by its current Name) and inserts it into Choices at its
        /// alphabetical position by Name, in place.</summary>
        T Register(T template)
        {
            m_byName[template.Name] = template;

            int index = m_choices.FindIndex(t => string.CompareOrdinal(t.Name, template.Name) > 0);
            if (index < 0)
                m_choices.Add(template);
            else
                m_choices.Insert(index, template);

            return template;
        }

        static T BuildTransientCandidate(string text, T previous)
        {
            var candidate = previous != null ? UnityEngine.Object.Instantiate(previous) : ScriptableObject.CreateInstance<T>();
            candidate.Name = text;
            return candidate;
        }
    }
}
