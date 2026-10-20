using System;
using System.Collections.Generic;
using System.Linq;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Acknex.Runtime.UI.TemplateEditor;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Type-erased view of a TemplateTabSet&lt;T&gt;, so a Manipulator can drive the sets of several
    /// Template types the same way.
    /// </summary>
    internal interface ITemplateTabSet
    {
        /// <summary>The tabs of the set, in display order.</summary>
        IReadOnlyList<TemplateTab> Tabs { get; }

        /// <summary>Raised when a tab's "Clone &amp; Edit" button is pressed.</summary>
        event Action CloneRequested;

        /// <summary>Raised with true when editing begins and false when it ends or the set is rebound.</summary>
        event Action<bool> EditingChanged;

        /// <summary>Raised when the saved values of the bound Template changed: after Apply, or because
        /// something else (e.g. the Inspector) edited it.</summary>
        event Action Changed;

        /// <summary>Starts editing the bound Template. No effect without one.</summary>
        void BeginEdit();

        /// <summary>Applies pending edits to the bound Template. No effect when not editing.</summary>
        void Apply();

        /// <summary>Drops pending edits. No effect when not editing.</summary>
        void Discard();
    }

    /// <summary>
    /// The Template tabs of one Template type, built from a layout and bound to one edit session at a
    /// time: the one on the Template currently selected in the Manipulator's picker. Binding starts a
    /// fresh session (discarding pending edits of the previous one) and writes the usage text
    /// ("used by N objects") into the tabs. When the bound Template is changed elsewhere the tabs reload
    /// their fields, unless they are editing it.
    /// </summary>
    /// <typeparam name="T">The Template type the tabs edit.</typeparam>
    internal class TemplateTabSet<T> : ITemplateTabSet where T : TemplateAsset
    {
        readonly TemplateRegistry<T> m_registry;
        readonly TemplateResolver<T> m_resolver;
        readonly string m_usageNoun;
        readonly TemplateTab[] m_tabs;
        TemplateEditSession m_session;

        /// <inheritdoc/>
        public IReadOnlyList<TemplateTab> Tabs => m_tabs;

        /// <inheritdoc/>
        public event Action CloneRequested;

        /// <inheritdoc/>
        public event Action<bool> EditingChanged;

        /// <inheritdoc/>
        public event Action Changed;

        /// <summary>Builds one tab per group. registry persists applied edits, resolver and usageNoun
        /// produce the usage text.</summary>
        public TemplateTabSet(TemplateRegistry<T> registry, TemplateResolver<T> resolver, IReadOnlyList<TemplatePropertyGroup> groups, string usageNoun)
        {
            m_registry = registry;
            m_resolver = resolver;
            m_usageNoun = usageNoun;

            m_tabs = groups.Select(group => new TemplateTab(typeof(T), group)).ToArray();
            foreach (var tab in m_tabs)
                tab.CloneRequested += () => CloneRequested?.Invoke();

            registry.TemplateChanged += OnTemplateChanged;
        }

        /// <summary>Binds the tabs to a new session on template (null unbinds). openName is the name of the
        /// instance whose window is open and countByName the usage table of its kind (null if unknown);
        /// linearStep is the step of the float fields.</summary>
        public void Bind(T template, string openName, IReadOnlyDictionary<string, int> countByName, float linearStep)
        {
            Discard();

            m_session = template != null ? new TemplateEditSession(template, asset => m_registry.MarkDirty((T)asset)) : null;
            if (m_session != null)
            {
                m_session.StateChanged += () => EditingChanged?.Invoke(m_session != null && m_session.IsEditing);
            }

            EditingChanged?.Invoke(false);

            string usageText = BuildUsageText(template, openName, countByName);
            foreach (var tab in m_tabs)
            {
                tab.SetStep(linearStep);
                tab.Bind(m_session, usageText);
            }
        }

        void OnTemplateChanged(TemplateAsset changed)
        {
            if (m_session == null || changed != m_session.Original)
                return;

            if (!m_session.IsEditing)
            {
                foreach (var tab in m_tabs)
                    tab.Refresh();
            }

            Changed?.Invoke();
        }

        /// <inheritdoc/>
        public void BeginEdit() => m_session?.BeginEdit();

        /// <inheritdoc/>
        public void Apply() => m_session?.Apply();

        /// <inheritdoc/>
        public void Discard() => m_session?.Discard();

        /// <summary>"used by N objects": the instances in the map resolving to template. The open instance
        /// is counted too, even if it only gets this Template when its window is confirmed. Without a count
        /// table only a general sharing note can be shown.</summary>
        string BuildUsageText(T template, string openName, IReadOnlyDictionary<string, int> countByName)
        {
            if (template == null)
                return string.Empty;

            if (countByName == null)
                return $"shared by every {m_usageNoun} using this Template";

            int count = m_resolver.CountUsers(countByName, template);
            if (!m_resolver.Matches(openName, template))
                count++;

            return count == 1 ? $"used by 1 {m_usageNoun}" : $"used by {count} {m_usageNoun}s";
        }
    }
}
