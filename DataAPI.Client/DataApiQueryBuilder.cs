using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataAPI.DataStructures.DataIo;

namespace DataAPI.Client
{
    public class DataApiQueryBuilder<T>
    {
        private string selectArguments;
        private string SelectArguments
        {
            get => selectArguments;
            set
            {
                if(selectArguments != null)
                    throw new InvalidOperationException("Select can only be called once for each query. Calling Count() corresponds to a Select-statement");
                selectArguments = value;
            }
        }
        private string whereArguments;
        private string WhereArguments
        {
            get => whereArguments;
            set
            {
                if(whereArguments != null)
                    throw new InvalidOperationException("Where can only be called once for each query");
                whereArguments = value;
            }
        }
        private string limitArguments;
        private string LimitArguments
        {
            get => limitArguments;
            set
            {
                if(limitArguments != null)
                    throw new InvalidOperationException("Limit can only be called once for each query");
                limitArguments = value;
            }
        }

        private string skipArguments;
        public string SkipArguments
        {
            get => skipArguments;
            set
            {
                if(skipArguments != null)
                    throw new InvalidOperationException("Skip can only be called once for each query");
                skipArguments = value;
            }
        }

        private string groupByArguments;
        private string GroupByArguments
        {
            get => groupByArguments;
            set
            {
                if(groupByArguments != null)
                    throw new InvalidOperationException("GroupBy can only be called once for each query");
                groupByArguments = value;
            }
        }

        private string OrderByArguments { get; set; }

        public DataApiQueryBuilder<T> Select(params Expression<Func<T, object>>[] projections)
        {
            var paths = new List<string>();
            foreach (var projection in projections)
            {
                var path = ExpressionParser.ExtractPath(projection.Body);
                paths.Add(path);
            }
            if (paths.Any())
                SelectArguments = string.Join(", ", paths);
            else
                SelectArguments = "*";
            return this;
        }

        public DataApiQueryBuilder<T> Count()
        {
            SelectArguments = "COUNT()";
            return this;
        }

        public DataApiQueryBuilder<T> Skip(int skipCount)
        {
            SkipArguments = $"{skipCount}";
            return this;
        }

        public DataApiQueryBuilder<T> Limit(int limit)
        {
            LimitArguments = $"{limit}";
            return this;
        }

        public DataApiQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereArguments = ExpressionParser.ParseWhereExpression(predicate.Body);
            return this;
        }

        public DataApiQueryBuilder<T> GroupBy(Expression<Func<T, object>> groupKey)
        {
            GroupByArguments = ExpressionParser.ExtractPath(groupKey.Body);
            return this;
        }

        public DataApiQueryBuilder<T> OrderBy(Expression<Func<T, object>> sortField, SortDirection sortDirection)
        {
            if(sortDirection == SortDirection.Undefined)
                throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, "Undefined is not a valid sort direction");
            if (OrderByArguments != null)
                throw new InvalidOperationException("OrderBy can only be called once for each query. Use ThenBy for adding secondary sort criteria.");
            var path = ExpressionParser.ExtractPath(sortField.Body);
            var sortDirectionString = sortDirection == SortDirection.Descending ? "DESC" : "ASC";
            OrderByArguments = $"{path} {sortDirectionString}";
            return this;
        }

        public DataApiQueryBuilder<T> ThenBy(Expression<Func<T, object>> sortField, SortDirection sortDirection)
        {
            if(sortDirection == SortDirection.Undefined)
                throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, "Undefined is not a valid sort direction");
            if (OrderByArguments == null)
                throw new InvalidOperationException("ThenBy can only be called after OrderBy");
            var path = ExpressionParser.ExtractPath(sortField.Body);
            var sortDirectionString = sortDirection == SortDirection.Descending ? "DESC" : "ASC";
            OrderByArguments += $", {path} {sortDirectionString}";
            return this;
        }

        public string Build()
        {
            var queryParts = new List<string>
            {
                $"SELECT {SelectArguments ?? "*"}",
                $"FROM {DataApiClient.GetCollectionName<T>()}"
            };
            if(WhereArguments != null)
                queryParts.Add($"WHERE {WhereArguments}");
            if(GroupByArguments != null)
                queryParts.Add($"GROUP BY {GroupByArguments}");
            if(OrderByArguments != null)
                queryParts.Add($"ORDER BY {OrderByArguments}");
            if(SkipArguments != null)
                queryParts.Add($"SKIP {SkipArguments}");
            if(LimitArguments != null)
                queryParts.Add($"LIMIT {LimitArguments}");
            return string.Join(" ", queryParts);
        }
    }
}