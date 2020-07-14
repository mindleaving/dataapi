using System;
using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class LimitDefinitionBuilder
    {
        public static JsonPipelineStageDefinition<T,T> Build<T>(uint limit)
        {
            return new JsonPipelineStageDefinition<T, T>($"{{ $limit : {limit} }}");
        }
        public static JsonPipelineStageDefinition<T,T> Build<T>(string limitArguments)
        {
            if (string.IsNullOrEmpty(limitArguments))
                return null;
            if(!uint.TryParse(limitArguments, out var limit))
                throw new FormatException($"Invalid limit '{limitArguments}'");
            return Build<T>(limit);
        }
    }
}