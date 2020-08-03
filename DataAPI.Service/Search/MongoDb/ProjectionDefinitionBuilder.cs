using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class ProjectionDefinitionBuilder
    {
        private const bool ExcludeId = false;

        public static JsonPipelineStageDefinition<T,T> Build<T>(string selectArguments)
        {
            if (string.IsNullOrEmpty(selectArguments))
                return null;
            if (selectArguments == "*")
                return null;

            var splittedFieldProjections = selectArguments.Split(new []{','},StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            var projections = new List<string>();
            if(ExcludeId)
                projections.Add("\"_id\" : false");
            foreach (var fieldProjection in splittedFieldProjections)
            {
                var spaceSplit = fieldProjection.Split();
                var dbFieldName = spaceSplit[0];
                if (spaceSplit.Length == 3)
                {
                    if(spaceSplit[1].ToLowerInvariant() != "as")
                        throw new FormatException($"Invalid projection '{fieldProjection}'");
                    var newNameOfField = spaceSplit[2];
                    projections.Add($"\"{newNameOfField}\" : \"${dbFieldName}\"");
                }
                else if (spaceSplit.Length == 1)
                {
                    if (dbFieldName.Contains("."))
                    {
                        var flattenedDbFieldName = dbFieldName.Replace(".", "_");
                        projections.Add($"\"{flattenedDbFieldName}\" : \"${dbFieldName}\"");
                    }
                    else
                    {
                        projections.Add($"\"{dbFieldName}\" : true");
                    }
                }
                else
                {
                    throw new FormatException($"Invalid projection '{fieldProjection}'");
                }
            }
            var aggregatedFieldNames = projections.Aggregate((a,b) => a + ", " + b);
            var projectJson = $"{{ $project : {{ {aggregatedFieldNames} }} }}";
            return new JsonPipelineStageDefinition<T,T>(projectJson);
        }
    }
}