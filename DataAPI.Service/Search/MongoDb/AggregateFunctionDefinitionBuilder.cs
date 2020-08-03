using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class AggregateFunctionDefinitionBuilder
    {
        public static JsonPipelineStageDefinition<T, T> Count<T>()
        {
            return new JsonPipelineStageDefinition<T, T>("{ $count : \"count\" }");
        }

        public static JsonPipelineStageDefinition<T, T> Sum<T>(string propertyPath)
        {
            return new JsonPipelineStageDefinition<T, T>($"{{ $group: {{ \"_id\" : \"all\", \"sum\" : {{ $sum : \"${propertyPath}\" }}  }} }}");
        }
    }
}
