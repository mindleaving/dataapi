using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class FilterDefinitionBuilder
    {
        public static JsonPipelineStageDefinition<T,T> Build<T>(string whereArguments)
        {
            var sqlWhereClauseParser = new DataApiSqlWhereClauseParser(new MongoDbExpressionBuilder());
            var expression = sqlWhereClauseParser.Parse(whereArguments);
            if (expression == null)
                return null;
            var filterJson = $"{{ $match : {expression} }}";
            return new JsonPipelineStageDefinition<T, T>(filterJson);
        }
    }
}

