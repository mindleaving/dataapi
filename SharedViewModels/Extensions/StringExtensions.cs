namespace SharedViewModels.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string str, int length)
        {
            if (str == null)
                return null;
            return str.Length <= length ? str : str.Substring(0, length - 3) + "...";
        }
    }
}
