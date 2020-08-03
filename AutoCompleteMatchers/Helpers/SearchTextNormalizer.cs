using System.Text.RegularExpressions;

namespace AutoCompleteMatchers.Helpers
{
    public static class SearchTextNormalizer
    {
        public static string Normalize(string searchText)
        {
            var trimmed = searchText.Trim();
            var spaceNormalized = Regex.Replace(trimmed, "\\s+", " ");
            var quotesRemoved = Regex.Replace(spaceNormalized, "['\"]", "");
            return quotesRemoved;
        }
    }
}
