using System.Text;

namespace uWED.Runtime.UI.Manipulator
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
            bool first = true;
            foreach (char c in input)
            {
                if ((c >= 'a' && c <= 'z') || (!first && c >= '0' && c <= '9') || c == '_')
                {
                    sb.Append(c);
                    first = false;
                }
            }

            return sb.ToString();
        }
    }
}
