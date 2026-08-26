using System;
using System.Collections.Generic;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Supplies a GenericComboBoxField&lt;T&gt; with its data source and two
    /// independent, optional callbacks:
    /// - ItemFactory: commit-time creation of a new T from typed text (fires on
    ///   "+", Enter-on-row, or an explicit value set - never per-keystroke).
    /// - Sanitizer: live, per-keystroke text cleanup (string -&gt; string, no T
    ///   involved - matches IGenericComboBoxField.Sanitizer).
    /// These are unrelated to each other; a provider may supply either, both,
    /// or neither. Display name for non-string T comes from ToString() - the
    /// consuming type must override it for a useful label.
    ///
    /// Replaces the old (removed) non-generic INameProvider everywhere.
    /// </summary>
    public interface IGenericNameProvider<T>
    {
        /// <summary>Direct, mutable backing list - handed straight to
        /// GenericComboBoxField&lt;T&gt;.Choices. Add/remove via the combo box's
        /// own +/- mutate this list in place.</summary>
        IList<T> Choices { get; }

        /// <summary>(text, previousValue) -&gt; T. Null is fine for string/primitive
        /// T - GenericComboBoxField auto-handles those via Convert.ChangeType.</summary>
        Func<string, T, T> ItemFactory { get; }

        /// <summary>Cleans the text field's content after each keystroke. Null
        /// means no live sanitization.</summary>
        Func<string, string> Sanitizer { get; }
    }
}
