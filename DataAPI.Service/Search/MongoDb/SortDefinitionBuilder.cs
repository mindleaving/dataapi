using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace DataAPI.Service.Search.MongoDb
{
    public static class SortDefinitionBuilder
    {
        public static JsonPipelineStageDefinition<T,T> Build<T>(string orderByArguments)
        {
            if (string.IsNullOrWhiteSpace(orderByArguments))
                return null;
            var splittedOrderKeys = orderByArguments.Split(',').Select(x => x.Trim());
            var sortParameters = new List<SortParameter>();
            foreach (var orderKey in splittedOrderKeys)
            {
                var spaceSplit = orderKey.Split();
                if (spaceSplit.Length > 2)
                    throw new FormatException();

                var parameterName = spaceSplit[0];
                var sortDirection = SortDirection.Ascending;
                if (spaceSplit.Length == 2)
                {
                    var directionString = spaceSplit[1].ToLowerInvariant();
                    sortDirection = directionString.StartsWith("asc") ? SortDirection.Ascending
                        : directionString.StartsWith("desc") ? SortDirection.Descending
                        : throw new FormatException($"Invalid search direction '{spaceSplit[1]}'");

                }
                sortParameters.Add(new SortParameter(parameterName, sortDirection));
            }
            var aggregatedSortParameters = sortParameters
                .Select(x => $"\"{x.ParameterName}\" : {SortDirectionToInteger(x.Direction)}")
                .Aggregate((a, b) => a + ", " + b);
            var sortJson = $"{{ $sort : {{ {aggregatedSortParameters} }} }}";
            return new JsonPipelineStageDefinition<T,T>(sortJson);
        }

        private static int SortDirectionToInteger(SortDirection sortDirection)
        {
            return sortDirection == SortDirection.Ascending ? 1 : -1;
        }

        private class SortParameter
        {
            public SortParameter(string parameterName, SortDirection direction)
            {
                ParameterName = parameterName;
                Direction = direction;
            }

            public string ParameterName { get; }
            public SortDirection Direction { get; }
        }
    }
}
