namespace DataAPI.Service.Objects
{
    public class DataApiSqlQuery
    {
        public DataApiSqlQuery(
            string fromArguments,
            string selectArguments = null,
            string whereArguments = null,
            string orderByArguments = null,
            string groupByArguments = null,
            string limitArguments = null,
            string skipArguments = null,
            string joinArguments = null)
        {
            SelectArguments = selectArguments;
            FromArguments = fromArguments;
            WhereArguments = whereArguments;
            OrderByArguments = orderByArguments;
            GroupByArguments = groupByArguments;
            LimitArguments = limitArguments;
            SkipArguments = skipArguments;
            JoinArguments = joinArguments;
        }

        public string SelectArguments { get; }
        public string FromArguments { get; }
        public string WhereArguments { get; }
        public string OrderByArguments { get; }
        public string GroupByArguments { get; }
        public string LimitArguments { get; }
        public string SkipArguments { get; }
        public string JoinArguments { get; }
    }
}