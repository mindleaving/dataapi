namespace DataAPI.Service.AccessManagement
{
    public static class UsernameNormalizer
    {
        public static string Normalize(string username)
        {
            var normalizedUsername = username.ToLowerInvariant();
            if (normalizedUsername.Contains("\\"))
                normalizedUsername = normalizedUsername.Substring(normalizedUsername.LastIndexOf('\\') + 1);
            return normalizedUsername;
        }
    }
}