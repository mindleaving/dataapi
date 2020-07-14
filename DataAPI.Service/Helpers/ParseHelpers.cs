using System.Text.RegularExpressions;

namespace DataAPI.Service.Helpers
{
    public static class ParseHelpers
    {
        public static bool HasQuotes(string value)
        {
            return Regex.IsMatch(value, "^[\"'].*[\"']$");
        }

        public static string StripQuotes(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);
            if (value.StartsWith("'") && value.EndsWith("'"))
                return value.Substring(1, value.Length - 2);
            return value;
        }
    }
}
