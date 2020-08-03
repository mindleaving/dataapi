using System;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;
using DataAPI.Service.Objects;

namespace DataAPI.Service.Search
{
    // ReSharper disable InconsistentNaming
    public static class DataApiSqlQueryParser
    {
        private const string SELECT = "select";
        private const string FROM = "from";
        private const string WHERE = "where";
        private const string ORDER_BY = "order by";
        private const string SORT_BY = "sort by";
        private const string GROUP_BY = "group by";
        private const string ORDERBY = "orderby";
        private const string SORTBY = "sortby";
        private const string GROUPBY = "groupby";
        private const string LIMIT = "limit";
        private const string SKIP = "skip";
        private const string JOIN = "join";
        private static readonly string[] Keywords = { SELECT, FROM, WHERE, ORDER_BY, ORDERBY, SORT_BY, SORTBY, GROUP_BY, GROUPBY, LIMIT, SKIP, JOIN };

        public static DataApiSqlQuery Parse(string query)
        {
            if(query.Contains('{') || query.Contains('}'))
                throw new FormatException(); // SECURITY NOTE: This is crucial in order to prevent SQL-injection like attacks (only with MongoDB-syntax instead of SQL)

            var preprocessedQuery = Preprocess(query);
            var lowerCaseQuery = preprocessedQuery.ToLowerInvariant();
            lowerCaseQuery = $" {lowerCaseQuery} "; // Pad with space to be able to search for space padded keywords
            var keywordPositions = Keywords
                .SelectMany(keyword => lowerCaseQuery.AllIndicesOf($" {keyword} ").Select(position => new { Keyword = keyword, Position = position }))
                .ToList();
            var orderedKeywordPositions = keywordPositions.OrderBy(x => x.Position).ToList();

            var keywordArguments = Keywords.ToDictionary(x => x, _ => (string) null);
            for (var positionIdx = 0; positionIdx < orderedKeywordPositions.Count; positionIdx++)
            {
                var keyword = orderedKeywordPositions[positionIdx].Keyword;
                var keywordPosition = orderedKeywordPositions[positionIdx].Position;
                var sectionStart = keywordPosition; // +1: Skip the leading space
                var sectionEnd = positionIdx + 1 < orderedKeywordPositions.Count
                    ? orderedKeywordPositions[positionIdx + 1].Position - 2 // -1-1: Remove trailing space and compensate for lowerCaseQuery has a space prefixed
                    : preprocessedQuery.Length-1;
                var sectionLength = sectionEnd - sectionStart + 1;
                var section = preprocessedQuery.Substring(sectionStart, sectionLength);
                var arguments = section.Substring(keyword.Length + 1); // +1: Also skip space after keyword
                var existingArguments = keywordArguments[keyword];
                if (existingArguments != null)
                {
                    switch (keyword)
                    {
                        case WHERE:
                            keywordArguments[keyword] = $"({existingArguments}) AND ({arguments})";
                            break;
                        case SELECT:
                            keywordArguments[keyword] = $"{existingArguments}, {arguments}";
                            break;
                        default:
                            throw new FormatException($"Duplicate keyword '{keyword}'");
                    }
                } 
                else
                {
                    keywordArguments[keyword] = arguments;
                }
            }
            return new DataApiSqlQuery(
                fromArguments: keywordArguments[FROM], 
                selectArguments: keywordArguments[SELECT], 
                whereArguments: keywordArguments[WHERE], 
                orderByArguments: keywordArguments[ORDER_BY] ?? keywordArguments[ORDERBY] ?? keywordArguments[SORT_BY] ?? keywordArguments[SORTBY],
                groupByArguments: keywordArguments[GROUP_BY] ?? keywordArguments[GROUPBY],
                limitArguments: keywordArguments[LIMIT],
                skipArguments: keywordArguments[SKIP],
                joinArguments: keywordArguments[JOIN]);
        }

        private static string Preprocess(string query)
        {
            var removeLineBreaks = query.Replace("\n", " ").Replace("\r", "");
            var singleSpaceQuery = Regex.Replace(removeLineBreaks, @"\s+", " ");
            return singleSpaceQuery;
        }
    }
}