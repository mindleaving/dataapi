namespace DataAPI.Service.Search
{
    public static class DataApiSqlOperators
    {
        public static readonly string[] LogicalOperators = {" AND ", " OR ", "&&", "||"};
        public static readonly string[] UnaryOperators = {" EXISTS", " NOT EXISTS"};
        public static readonly string[] RelationshipOperators = {"=", "==", "!=", "<>", " IS ", " IS NOT ", " LIKE ", ">", ">=", "<", "<=", " IN ", " NOT IN "};
    }
}
