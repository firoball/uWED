using System.Collections.Generic;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.Registry
{
    /// <summary>
    /// Resolves the Template matching a placed Instance's Name against a TemplateRegistry&lt;T&gt;,
    /// creating and registering a new blank one first if nothing matches yet. The map (WMP) format only
    /// ever holds plain base uWED types, never an Acknex subclass, so a placed Instance (a Segment,
    /// MapObject, etc.) is never anything more than its own Name string - the Template it maps to is
    /// found purely by matching that Name, which fails to match only because the WDL defining it hasn't
    /// been parsed yet. An Instance with no Name of its own yet (e.g. a Wall Segment just drawn in the
    /// editor, before it's ever been assigned one) resolves to the reserved DefaultName Template instead
    /// - the same shared fallback every such not-yet-named Instance of this type gets, auto-created under
    /// that reserved Name the first time it's needed, exactly like any other unmatched Name. DefaultName
    /// is per-type ("__defaultwall", "__defaultregion", ...), not a single shared "__default" - the map
    /// editor itself could tell identical Names on different node types apart (Wall/Region/etc. each get
    /// their own TemplateRegistry&lt;T&gt;), but WDL's own name space is flat across every node type, so a
    /// bare "__default" would collide the moment more than one type needed a fallback. Stateless and
    /// reusable as-is for every Template type - TemplateRegistry&lt;T&gt; stays pure storage with no
    /// opinion on when a T should be created; this class is where that "no match -&gt; create a default"
    /// policy lives instead, so a Template type whose resolution policy ever needs to differ (e.g. warn
    /// instead of auto-create) gets its own resolver in its place, without TemplateRegistry or any other
    /// type's resolver having to change.
    /// </summary>
    public class TemplateResolver<T> where T : TemplateAsset
    {
        const string TemplateTypeNameSuffix = "Template";

        /// <summary>The reserved Name of this type's fallback Template, used whenever an Instance has no
        /// Name of its own yet - "__default" plus T's own name with its "Template" suffix stripped and
        /// lowercased (e.g. WallTemplate -&gt; "__defaultwall"), then run through NameSanitizer like every
        /// other Name this resolver produces, so the reserved fallback is always a legal WDL identifier too.
        /// Lowercased and built up front rather than left to NameSanitizer alone, so every Template type's
        /// fallback Name is guaranteed both non-colliding in WDL's flat name space and, via its leading
        /// underscores, sorted ahead of every ordinary Name in an alphabetically-ordered Template picker.</summary>
        public static readonly string DefaultName = NameSanitizer.Sanitize("__default" + StripTemplateSuffix(typeof(T).Name).ToLowerInvariant());

        readonly TemplateRegistry<T> m_registry;
        bool m_loggedEmptyNameFallback;

        /// <summary>Resolves against this one registry for the lifetime of this resolver.</summary>
        public TemplateResolver(TemplateRegistry<T> registry)
        {
            m_registry = registry;
        }

        /// <summary>Looks up the T matching name, creating and registering a new blank one first if none
        /// exists yet. A null or empty name resolves to DefaultName instead of an unnamed Template. name
        /// is sanitized before the lookup, not just before creating - Get must compare against the same
        /// sanitized form every registered Name is stored under (via Create/Clone), or a not-yet-sanitized
        /// name (e.g. straight off a Segment the map editor never enforced WDL syntax on) would never
        /// match an existing entry and this would call Create over and over, one duplicate per call. Logs
        /// an info message the first time this resolver falls back to DefaultName (once per resolver
        /// instance, not once per empty-named Instance - a map can carry many, and repeating the same line
        /// for each one is noise, not information) and every time no match exists yet (a new T is about to
        /// be created - this one already only fires once per distinct name, since Get succeeds on every
        /// call after the first).</summary>
        public T Resolve(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (!m_loggedEmptyNameFallback)
                {
                    Debug.Log($"TemplateResolver<{typeof(T).Name}>: empty name resolved to the default Template '{DefaultName}'.");
                    m_loggedEmptyNameFallback = true;
                }
                name = DefaultName;
            }
            else
            {
                name = NameSanitizer.Sanitize(name);
            }

            var existing = m_registry.Get(name);
            if (existing != null)
                return existing;

            Debug.Log($"TemplateResolver<{typeof(T).Name}>: no Template found for '{name}' - creating a new one.");
            return m_registry.Create(name);
        }

        /// <summary>Resolves every name in names, creating and registering a default T for any that
        /// don't match one yet. Used to populate the registry up front from a full Instance-Name list
        /// (e.g. every Segment's Name in a freshly loaded map) rather than waiting for each one to be
        /// resolved lazily as its own Manipulator is opened. Wrapped in a single registry batch (a no-op on
        /// a storage backend that doesn't support one) rather than paying each Create's own per-operation
        /// cost individually - the difference between one deferred save and dozens of immediate ones over
        /// a whole map's worth of unresolved names.</summary>
        public void ResolveAll(IEnumerable<string> names)
        {
            m_registry.BeginBatch();
            try
            {
                foreach (var name in names)
                    Resolve(name);
            }
            finally
            {
                m_registry.EndBatch();
            }
        }

        static string StripTemplateSuffix(string typeName) => typeName.EndsWith(TemplateTypeNameSuffix)
            ? typeName.Substring(0, typeName.Length - TemplateTypeNameSuffix.Length)
            : typeName;
    }
}
