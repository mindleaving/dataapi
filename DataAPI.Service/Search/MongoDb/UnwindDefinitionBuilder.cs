using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class UnwindDefinitionBuilder
    {
        public static IEnumerable<JsonPipelineStageDefinition<T, T>> Build<T>(string selectArguments)
        {
            if(selectArguments == "*")
                return Enumerable.Empty<JsonPipelineStageDefinition<T, T>>();
            var fields = selectArguments
                .Split(',')
                .Select(field => field.Trim())
                .Select(field => new string(field.TakeWhile(c => !char.IsWhiteSpace(c)).ToArray())); // handles 'Original.Field.Name AS Name' pattern in SELECT clause
            var unwindPaths = new List<string>();
            foreach (var field in fields)
            {
                if(field.StartsWith("."))
                    throw new FormatException("Fields cannot start with a dot (.)");
                var dotPositions = field.AllIndicesOf(".");
                foreach (var dotPosition in dotPositions)
                {
                    unwindPaths.Add(field.Substring(0, dotPosition));
                }
                unwindPaths.Add(field);
            }
            return unwindPaths
                .Distinct()
                .Select(path => $"{{ $unwind : \"${path}\" }}")
                .Select(json => new JsonPipelineStageDefinition<T, T>(json));
        }
    }
}