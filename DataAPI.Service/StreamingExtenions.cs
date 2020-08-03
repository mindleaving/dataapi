using System.Collections.Generic;
using MongoDB.Driver;

namespace DataAPI.Service
{
    public static class StreamingExtenions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IFindFluent<T,T> fluent)
        {
            using var cursor = await fluent.ToCursorAsync();
            while (await cursor.MoveNextAsync())
            {
                foreach (var result in cursor.Current)
                {
                    yield return result;
                }
            }
        }
    }
}
