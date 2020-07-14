using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client.Serialization
{
    public static class SeachResultStreamExtensions
    {
        public static Task<List<string>> ReadAllLinesAsync(this Stream stream)
        {
            return stream.ReadAllSearchResultsAsync(line => line);
        }

        public static Task<List<JObject>> ReadAllSearchResultsAsync(this Stream stream)
        {
            return stream.ReadAllSearchResultsAsync(JObject.Parse);
        }
        public static Task<List<T>> ReadAllSearchResultsAsync<T>(this Stream stream)
        {
            return stream.ReadAllSearchResultsAsync(ConfiguredJsonSerializer.Deserialize<T>);
        }

        public static async Task<List<T>> ReadAllSearchResultsAsync<T>(this Stream stream, Func<string,T> convertFunc)
        {
            var results = new List<T>();
            using (var streamReader = new StreamReader(stream))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    var result = convertFunc(line);
                    if(result != null)
                        results.Add(result);
                }
            }
            return results;
        }

        public static async Task ForEachSearchResult(this Stream stream, Action<JObject> action)
        {
            using (var streamReader = new StreamReader(stream))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    var result = JObject.Parse(line);
                    action(result);
                }
            }
        }
    }
}
