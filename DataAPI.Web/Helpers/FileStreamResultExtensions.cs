using System;
using System.Collections.Generic;
using System.Linq;
using DataAPI.Service;
using DataAPI.Service.Search;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataAPI.Web.Helpers
{
    internal static class FileStreamResultExtensions
    {
        public static FileStreamResult ToFileStreamResult(this IAsyncEnumerable<string> asyncEnumerable)
        {
            var stream = new SearchResultStream(asyncEnumerable);
            return new FileStreamResult(stream, Conventions.JsonContentType);
        }

        public static FileStreamResult ToFileStreamResult<T>(
            this IAsyncEnumerable<T> asyncEnumerable,
            JsonSerializerSettings settings)
        {
            var stream = new SearchResultStream(asyncEnumerable.Select(x => JsonConvert.SerializeObject(x, settings)));
            return new FileStreamResult(stream, Conventions.JsonContentType);
        }

        public static FileStreamResult ToFileStreamResult<T>(
            this IAsyncEnumerable<T> asyncEnumerable, 
            Func<T,string> selector)
        {
            var stream = new SearchResultStream(asyncEnumerable.Select(selector));
            return new FileStreamResult(stream, Conventions.JsonContentType);
        }
    }
}
