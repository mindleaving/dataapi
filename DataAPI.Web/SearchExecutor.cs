using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.DataStructures;
using DataAPI.Service;
using DataAPI.Service.DataRouting;
using DataAPI.Service.Search;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DataAPI.Web
{
    internal static class SearchExecutor
    {
        public static async Task<IActionResult> PerformSearch(IDataRouter dataRouter, string query, ResultFormat resultFormat)
        {
            try
            {
                var parsedQuery = DataApiSqlQueryParser.Parse(query);
                var collection = parsedQuery.FromArguments.Trim();
                var rdDataStorage = await dataRouter.GetSourceSystemAsync(collection);
                var result = rdDataStorage.SearchAsync(parsedQuery);
                switch (resultFormat)
                {
                    case ResultFormat.Json:
                        return BuildJsonResultAsync(result);
                    case ResultFormat.Csv:
                        return BuildCsvResultAsync(result);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resultFormat), resultFormat, null);
                }
            }
            catch (FormatException formatException)
            {
                return new ContentResult
                {
                    ContentType = "text/plain",
                    Content = formatException.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            catch (AggregateException aggregateException)
            {
                var innermostException = aggregateException.InnermostException();
                if (innermostException is FormatException)
                {
                    return new ContentResult
                    {
                        ContentType = "text/plain",
                        Content = aggregateException.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }
                return new ContentResult
                {
                    Content = aggregateException.InnerException?.Message ?? aggregateException.Message,
                    ContentType = "text/plain",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private static IActionResult BuildJsonResultAsync(IAsyncEnumerable<BsonDocument> documents)
        {
            var searchResultStream = new SearchResultStream(documents.Select(DataEncoder.DecodeToJson));
            return new FileStreamResult(searchResultStream, Conventions.JsonContentType);
        }

        private static IActionResult BuildCsvResultAsync(IAsyncEnumerable<BsonDocument> documents)
        {
            var searchResultStream = new SearchResultStream(ToCsv(documents));
            return new FileStreamResult(searchResultStream, "text/csv");
        }

        private static async IAsyncEnumerable<string> ToCsv(IAsyncEnumerable<BsonDocument> documents)
        {
            const char Delimiter = ';';
            var columnNames = new List<string>();

            var firstDocument = true;
            await foreach (var document in documents)
            {
                var topLevelFields = document.Elements;
                var entry = topLevelFields.ToDictionary(element => element.Name, element => $"\"{element.Value}\"");
                var entryColumnNames = entry.Keys.ToList();
                // Update header map with new columns
                entryColumnNames
                    .Where(columnName => !columnNames.Contains(columnName))
                    .ForEach(columnName => columnNames.Add(columnName));
                if (firstDocument)
                {
                    yield return string.Join(Delimiter, columnNames);
                    firstDocument = false;
                }

                var rowValues = columnNames.Select(columnName => entry.ContainsKey(columnName) ? entry[columnName] : null);
                yield return string.Join(Delimiter, rowValues);
            }
        }
    }
}
