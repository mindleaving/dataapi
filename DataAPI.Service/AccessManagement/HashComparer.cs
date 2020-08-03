namespace DataAPI.Service.AccessManagement
{
    public static class HashComparer
    {
        /// <summary>
        /// Constant-time compares to byte arrays
        /// </summary>
        public static bool Compare(
            byte[] a,
            byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            var areEqual = true;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    areEqual = false;
            }
            return areEqual;
        }
    }
}
