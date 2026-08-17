using System.Text;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// Sanitizes freshly-typed names (not existing list entries, which stay
    /// untouched - see INameProvider). Rules: trim, lowercase, keep only
    /// [a-z0-9_].
    /// </summary>
    public static class NameSanitizer
    {
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            input = input.Trim().ToLowerInvariant();

            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_')
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
