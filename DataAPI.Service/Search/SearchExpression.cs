namespace DataAPI.Service.Search
{
    public class SearchExpression : ISearchExpression
    {
        public SearchExpression(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}