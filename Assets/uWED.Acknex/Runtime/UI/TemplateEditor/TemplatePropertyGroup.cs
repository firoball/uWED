using System.Collections.Generic;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// A titled group of Template property names shown together (a Manipulator tab, or a section of an
    /// Inspector). Properties are listed by name in display order and resolved by TemplatePropertyPanel.
    /// </summary>
    public class TemplatePropertyGroup
    {
        /// <summary>Title of the group, e.g. a tab caption.</summary>
        public string Title { get; }

        /// <summary>Names of the Template properties in display order.</summary>
        public IReadOnlyList<string> Properties { get; }

        /// <summary>Number of columns the fields of the group flow in.</summary>
        public int Columns { get; }

        /// <summary>True if the group offers a button clearing all its bool properties.</summary>
        public bool ClearAllButton { get; }

        /// <summary>Names of properties not editable yet; shown as disabled rows after the fields.</summary>
        public IReadOnlyList<string> Placeholders { get; }

        /// <summary>Short explanatory text shown below the fields, or null.</summary>
        public string Note { get; }

        /// <summary>Creates a group of properties, laid out in columns columns; clearAllButton adds a button
        /// that sets every bool property of the group to false. placeholders lists properties shown as
        /// disabled rows, note is shown below the fields.</summary>
        public TemplatePropertyGroup(string title, IReadOnlyList<string> properties, int columns = 1, bool clearAllButton = false,
            IReadOnlyList<string> placeholders = null, string note = null)
        {
            Title = title;
            Properties = properties;
            Columns = columns;
            ClearAllButton = clearAllButton;
            Placeholders = placeholders;
            Note = note;
        }
    }
}
