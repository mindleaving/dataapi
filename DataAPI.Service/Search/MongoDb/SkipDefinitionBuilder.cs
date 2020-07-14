using System;
using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class SkipDefinitionBuilder
    {
        public static JsonPipelineStageDefinition<T,T> Build<T>(int skips)
        {
            return new JsonPipelineStageDefinition<T, T>($"{{ $skip : {skips} }}");
        }
        public static JsonPipelineStageDefinition<T,T> Build<T>(string skipArguments)
        {
            if (string.IsNullOrEmpty(skipArguments))
                return null;
            if(!int.TryParse(skipArguments, out var limit))
                throw new FormatException($"Invalid limit '{skipArguments}'");
            return Build<T>(limit);
        }
    }
}