using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DataAPI.Service.Objects;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class AggregatePipelineBuilder
    {
        public static PipelineDefinition<BsonDocument, BsonDocument> Build(DataApiSqlQuery parsedQuery, uint? hardLimitMaxResults = null)
        {
            if (parsedQuery.JoinArguments != null)
                throw new NotSupportedException("Joining is not supported");

            if (parsedQuery.FromArguments == null)
                throw new FormatException("Missing FROM statement");
            if (parsedQuery.FromArguments == string.Empty)
                throw new FormatException("Missing arguments for FROM statement");

            var stages = new List<JsonPipelineStageDefinition<BsonDocument, BsonDocument>>();

            var filterDefinition = FilterDefinitionBuilder.Build<BsonDocument>(parsedQuery.WhereArguments);
            if (filterDefinition != null)
                stages.Add(filterDefinition);

            var groupDefinition = GroupDefinitionBuilder.Build<BsonDocument>(parsedQuery.GroupByArguments);
            if(groupDefinition != null)
                stages.Add(groupDefinition);

            var selectArguments = parsedQuery.SelectArguments ?? "*";
            if (Regex.IsMatch(selectArguments, "^count\\([^()]*\\)$", RegexOptions.IgnoreCase))
            {
                var countDefinition = AggregateFunctionDefinitionBuilder.Count<BsonDocument>();
                stages.Add(countDefinition);
            }
            else if (Regex.IsMatch(selectArguments, "^sum\\([^()]+\\)$", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(selectArguments, "^sum\\((?<PropertyPath>[^()]+)\\)$", RegexOptions.IgnoreCase);
                var propertyPath = match.Groups["PropertyPath"].Value;
                if(string.IsNullOrWhiteSpace(propertyPath))
                    throw new FormatException("Property path is invalid in SUM(<propertyPath>)-function");
                var sumDefinition = AggregateFunctionDefinitionBuilder.Sum<BsonDocument>(propertyPath);
                stages.Add(sumDefinition);
            }
            else
            {
                if(selectArguments.Contains("(") || selectArguments.Contains(")"))
                    throw new FormatException("SELECT-argument contains invalid aggregate function");
                var sortDefinition = SortDefinitionBuilder.Build<BsonDocument>(parsedQuery.OrderByArguments);
                if (sortDefinition != null)
                    stages.Add(sortDefinition);

                var unwindDefinitions = UnwindDefinitionBuilder.Build<BsonDocument>(selectArguments);
                stages.AddRange(unwindDefinitions);

                var projectionDefinition = ProjectionDefinitionBuilder.Build<BsonDocument>(selectArguments);
                if (projectionDefinition != null)
                    stages.Add(projectionDefinition);

                var skipDefinition = SkipDefinitionBuilder.Build<BsonDocument>(parsedQuery.SkipArguments);
                if (skipDefinition != null)
                    stages.Add(skipDefinition);

                if (hardLimitMaxResults.HasValue)
                {
                    var hardLimitDefinition = LimitDefinitionBuilder.Build<BsonDocument>(hardLimitMaxResults.Value);
                    stages.Add(hardLimitDefinition);
                }

                var limitDefinition = LimitDefinitionBuilder.Build<BsonDocument>(parsedQuery.LimitArguments);
                if (limitDefinition != null)
                    stages.Add(limitDefinition);

                // Re-run sort
                if (sortDefinition != null)
                    stages.Add(sortDefinition);
            }

            return PipelineDefinition<BsonDocument, BsonDocument>.Create(stages);
        }
    }
}
