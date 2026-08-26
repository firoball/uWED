using System;
using System.Collections.Generic;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Default IGenericNameProvider&lt;string&gt; - in-memory backing list, no
    /// ItemFactory needed (string is auto-handled by GenericComboBoxField),
    /// Sanitizer wired to the existing NameSanitizer rules (trim/lowercase/
    /// [a-z0-9_], applied live as the user types). Used automatically by every
    /// manipulator's Name field when no provider is supplied; swap out for
    /// whatever actually backs a given name pool once ready.
    /// </summary>
    public class SimpleGenericNameProvider : IGenericNameProvider<string>
    {
        readonly List<string> m_names;

        public SimpleGenericNameProvider(List<string> names)
        {
            m_names = names;
        }

        public IList<string> Choices => m_names;
        public Func<string, string, string> ItemFactory => null;
        public Func<string, string> Sanitizer => NameSanitizer.Sanitize;
    }
}
