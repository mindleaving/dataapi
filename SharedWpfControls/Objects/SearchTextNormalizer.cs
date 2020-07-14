using System.Text.RegularExpressions;

namespace SharedWpfControls.Helpers
{
    public static class SearchTextNormalizer
    {
        public static string Normalize(string searchText)
        {
            var trimmed = searchText.Trim();
            var spaceNormalized = Regex.Replace(trimmed, "\\s+", " ");
            return spaceNormalized;
        }
    }
}
