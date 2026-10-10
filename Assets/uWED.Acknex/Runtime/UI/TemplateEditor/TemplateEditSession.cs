using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// Edit state of one Template: the shared original, and while editing a private working copy that the
    /// property fields change instead. Apply writes the copy's values of the tracked properties (the ones
    /// the bound panels display) back to the original and persists it; Discard drops the copy. The
    /// original is never modified before Apply. A direct session skips the working copy: it is always
    /// editing, the fields change the original and every edit persists it right away.
    /// </summary>
    public class TemplateEditSession
    {
        readonly Action<TemplateAsset> m_markDirty;
        readonly HashSet<PropertyInfo> m_tracked = new();
        TemplateAsset m_workingCopy;

        /// <summary>The shared Template this session edits.</summary>
        public TemplateAsset Original { get; }

        /// <summary>True if fields edit the original directly, without working copy, Apply or Discard.</summary>
        public bool IsDirect { get; }

        /// <summary>The object property fields read and write: the working copy while editing, otherwise the original.</summary>
        public TemplateAsset Current => m_workingCopy != null ? m_workingCopy : Original;

        /// <summary>True between BeginEdit and Apply/Discard, and always for a direct session.</summary>
        public bool IsEditing => IsDirect || m_workingCopy != null;

        /// <summary>True if the working copy differs from the original in any tracked property.</summary>
        public bool IsDirty { get; private set; }

        /// <summary>Raised when editing begins or ends (BeginEdit, Apply, Discard).</summary>
        public event Action StateChanged;

        /// <summary>Raised just before a field changes Current - the hook for recording an undo step.</summary>
        public event Action BeforeEdit;

        /// <summary>Raised when a field changed Current.</summary>
        public event Action Edited;

        /// <summary>Raised after Apply wrote the working copy's values to the original.</summary>
        public event Action Applied;

        /// <summary>Creates a session on original; markDirty persists the original after Apply, or after
        /// every edit if direct is set.</summary>
        public TemplateEditSession(TemplateAsset original, Action<TemplateAsset> markDirty, bool direct = false)
        {
            Original = original;
            m_markDirty = markDirty;
            IsDirect = direct;
        }

        /// <summary>Registers a property whose value Apply copies back to the original.</summary>
        public void Track(PropertyInfo property) => m_tracked.Add(property);

        /// <summary>Creates the working copy and starts editing. No effect while already editing.</summary>
        public void BeginEdit()
        {
            if (IsDirect || IsEditing)
                return;

            m_workingCopy = UnityEngine.Object.Instantiate(Original);
            m_workingCopy.hideFlags = HideFlags.HideAndDontSave;
            IsDirty = false;
            StateChanged?.Invoke();
        }

        /// <summary>Called by a field just before it changes Current.</summary>
        public void NotifyEditing() => BeforeEdit?.Invoke();

        /// <summary>Called by a field after it changed Current.</summary>
        public void NotifyEdited()
        {
            if (IsDirect)
                m_markDirty?.Invoke(Original);
            else
                IsDirty = HasChanges();

            Edited?.Invoke();
        }

        bool HasChanges()
        {
            foreach (var property in m_tracked)
            {
                if (!ValuesEqual(property.GetValue(Original), property.GetValue(m_workingCopy)))
                    return true;
            }
            return false;
        }

        static bool ValuesEqual(object a, object b)
        {
            if (a is float floatA && b is float floatB)
                return Mathf.Approximately(floatA, floatB);

            if (a is string || b is string)
                return ((a as string) ?? string.Empty) == ((b as string) ?? string.Empty);

            return Equals(a, b);
        }

        /// <summary>Writes the tracked properties of the working copy to the original, persists it and ends editing.</summary>
        public void Apply()
        {
            if (IsDirect || !IsEditing)
                return;

            foreach (var property in m_tracked)
                property.SetValue(Original, property.GetValue(m_workingCopy));

            m_markDirty?.Invoke(Original);
            EndEdit();
            Applied?.Invoke();
        }

        /// <summary>Drops the working copy and ends editing. No effect when not editing.</summary>
        public void Discard()
        {
            if (!IsDirect && IsEditing)
                EndEdit();
        }

        void EndEdit()
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(m_workingCopy);
            else
                UnityEngine.Object.DestroyImmediate(m_workingCopy);

            m_workingCopy = null;
            IsDirty = false;
            StateChanged?.Invoke();
        }
    }
}
