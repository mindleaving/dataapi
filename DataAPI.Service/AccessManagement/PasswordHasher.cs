using System;
using System.Security.Cryptography;

namespace DataAPI.Service.AccessManagement
{
    public static class PasswordHasher
    {
        public const int RecommendedHashLength = 256;

        public static byte[] Hash(string password, byte[] salt, int hashBitLength)
        {
            if(hashBitLength < 128)
                throw new ArgumentException("Requested hash length is too short. " +
                                            "Technically there isn't anything wrong with the requested hash length, " +
                                            "but I'm worried that you might not know why a long hash is important. " +
                                            "If you absolutely need a shorter hash length, truncate the returned byte array.");
            const int Iterations = 10000;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                return kdf.GetBytes(hashBitLength / 8);
            }
        }
    }
}