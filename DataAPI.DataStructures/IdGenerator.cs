using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DataAPI.DataStructures.Constants;

namespace DataAPI.DataStructures
{
    public static class IdGenerator
    {
        public const char PrefixDelimiter = '.';
        public static readonly Dictionary<IdSourceSystem, string> SourceSystemPrefixes = new Dictionary<IdSourceSystem, string>
        {
            //{ IdSourceSystem.<SystemName>, "<Prefix>" },
        };
        public static readonly Dictionary<string, IdSourceSystem> ReverseSourceSystemPrefixes =
            SourceSystemPrefixes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static string GlobalizeLocalId(IdSourceSystem sourceSystem, string localId)
        {
            return SourceSystemPrefixes[sourceSystem] + PrefixDelimiter + localId;
        }

        public static string FromGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static string FromUserAndTime(string username, DateTime time)
        {
            return $"{username}_{time:yyyy-MM-dd_HHmmss}";
        }

        public static string FromUserAndName(string username, string name)
        {
            var lowerCaseName = name.ToLowerInvariant();
            var spaceReplacedName = Regex.Replace(lowerCaseName, "\\s+", "_");
            var normalizedName = Regex.Replace(spaceReplacedName, "[^a-z0-9]", "");
            return $"{username}_{normalizedName}";
        }

        public static string Sha1HashFromFilePathAndDataProjectId(string imagePath, string dataProjectId)
        {
            var combinedId = $"{dataProjectId}_{imagePath}";
            using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(combinedId)))
            using (var outputStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.CopyTo(zipStream);
                }
                return BitConverter.ToString(outputStream.ToArray()).Replace("-", "");
            }
        }

        public static string Sha1HashFromByteArray(byte[] array)
        {
            using (var stream = new MemoryStream(array))
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-","");
            }
        }

        public static string Sha1HashFromStream(Stream stream)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-","");
            }
        }

        public static string Sha1HashFromFile(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-","");
            }
        }

        public static string Sha1HashFromString(string str)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-","");
            }
        }

        public static IdSourceSystem GetSourceSystem(string id)
        {
            var prefix = GetPrefix(id);
            if (!ReverseSourceSystemPrefixes.ContainsKey(prefix))
                throw new FormatException($"Could not detect source system from ID '{id}'");
            return ReverseSourceSystemPrefixes[prefix];
        }

        public static string GetApplicationId(string id)
        {
            var dotPosition = id.IndexOf(PrefixDelimiter);
            if(dotPosition <= 0) // Include 0, because an ID starting with a dot is not a valid global ID
                throw new FormatException($"'{id}' is not a valid global ID. No prefix found.");
            return id.Substring(dotPosition + 1);
        }

        private static string GetPrefix(string id)
        {
            var prefix = new string(id.TakeWhile(c => c != PrefixDelimiter).ToArray()).ToUpperInvariant();
            return prefix;
        }
    }
}
