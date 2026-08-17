using System.Collections.Generic;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Minimal in-memory INameProvider - lets the Manipulator windows compile and
    /// be tried out before a real, project-wide name registry exists. Swap out
    /// for whatever actually backs your Segment/Region/Way/Object names once
    /// that's ready; this one does not persist anything between Editor sessions.
    /// </summary>
    public class SimpleNameProvider : INameProvider
    {
        readonly List<string> m_names;

        public SimpleNameProvider(List<string> names)
        {
            m_names = names;
        }
        public IReadOnlyList<string> GetNames() => m_names;

        public bool TryCreateName(string sanitizedName)
        {
            if (string.IsNullOrEmpty(sanitizedName) || m_names.Contains(sanitizedName))
                return false;

            m_names.Add(sanitizedName);
            return true;
        }
    }
}
