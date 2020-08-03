namespace DataAPI.Service
{
    public static class StringExtensions
    {
        public static string RemoveLineBreaks(this string str, string replacement = " ")
        {
            return str.Replace("\n", replacement).Replace("\n", replacement).Replace("\r", replacement);
        }
    }
}
