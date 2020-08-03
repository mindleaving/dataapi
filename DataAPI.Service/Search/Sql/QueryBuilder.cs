using System;
using System.Collections.Generic;
using DataAPI.Service.Objects;

namespace DataAPI.Service.Search.Sql
{
    public class QueryBuilder
    {
        private readonly DataApiSqlWhereClauseParser whereClauseParser;
        private readonly Func<string, string> fieldNameManipulationFunc;

        public QueryBuilder(Func<string, string> fieldNameManipulationFunc)
        {
            this.fieldNameManipulationFunc = fieldNameManipulationFunc;
            whereClauseParser = new DataApiSqlWhereClauseParser(new SqlSearchSyntaxBuilder(fieldNameManipulationFunc));
        }

        public string Build(DataApiSqlQuery parsedQuery, string tablePath, uint? hardLimitMaxResults = null)
        {
            if(!string.IsNullOrWhiteSpace(parsedQuery.SkipArguments))
                throw new NotSupportedException("SKIP is not supported for SQL backends");
            var queryParts = new List<string>();
            var limit = hardLimitMaxResults;
            if (!string.IsNullOrWhiteSpace(parsedQuery.LimitArguments))
            {
                var softLimit = int.Parse(parsedQuery.LimitArguments);
                if (softLimit >= 0)
                {
                    if (!hardLimitMaxResults.HasValue || softLimit < hardLimitMaxResults.Value)
                        limit = (uint) softLimit;
                }
            }
            var limitStatement = limit.HasValue ? $"TOP ({limit})" : string.Empty;
            if (!string.IsNullOrWhiteSpace(parsedQuery.SelectArguments))
            {
                var fieldNameReplacedSelectArgument = fieldNameManipulationFunc(parsedQuery.SelectArguments.Replace("COUNT()", "COUNT(*)"));
                queryParts.Add($"SELECT {limitStatement} {fieldNameReplacedSelectArgument}");
            }
            else
                queryParts.Add($"SELECT {limitStatement} *");
            queryParts.Add($"FROM {tablePath}");
            if(!string.IsNullOrWhiteSpace(parsedQuery.JoinArguments))
                queryParts.Add($"JOIN {fieldNameManipulationFunc(parsedQuery.JoinArguments)}");
            if(!string.IsNullOrWhiteSpace(parsedQuery.WhereArguments))
            {
                var sqlWhereArguments = whereClauseParser.Parse(parsedQuery.WhereArguments);
                queryParts.Add($"WHERE {sqlWhereArguments}");
            }
            if(!string.IsNullOrWhiteSpace(parsedQuery.GroupByArguments))
                queryParts.Add($"GROUP BY {fieldNameManipulationFunc(parsedQuery.GroupByArguments)}");
            if(!string.IsNullOrWhiteSpace(parsedQuery.OrderByArguments))
                queryParts.Add($"ORDER BY {fieldNameManipulationFunc(parsedQuery.OrderByArguments)}");
            return string.Join(' ', queryParts);
        }
    }
}
