using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public class GroupDefinitionBuilder
    {
        public static JsonPipelineStageDefinition<T, T> Build<T>(string groupByArguments)
        {
            if (string.IsNullOrWhiteSpace(groupByArguments))
                return null;

            var propertyPath = groupByArguments.Trim();
            var groupByJson = $"{{ $group : {{ '_id' : '${propertyPath}', 'count' : {{ $sum : 1 }} }} }}";
            return new JsonPipelineStageDefinition<T, T>(groupByJson);
        }
    }
}
