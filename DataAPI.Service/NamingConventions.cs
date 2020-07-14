using System.Text.RegularExpressions;

namespace DataAPI.Service
{
    public static class NamingConventions
    {
        public const string CollectionNamePattern = "[a-zA-Z0-9_-]+";

        public static bool IsValidDataType(string dataType)
        {
            return Regex.IsMatch(dataType, $"^{CollectionNamePattern}$");
        }
    }
}
