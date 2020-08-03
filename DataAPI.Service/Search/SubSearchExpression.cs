namespace DataAPI.Service.Search
{
    public class SubSearchExpression : ISearchExpression
    {
        public SubSearchExpression(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}