namespace DataAPI.Service.Search
{
    public class DataApiSqlWhereClauseValidator
    {
        public bool IsValid(string whereArguments, out string errorText)
        {
            if (!ParenthesesCountValid(whereArguments))
            {
                errorText = "Invalid order or count of parentheses";
                return false;
            }
            errorText = null;
            return true;
        }

        private static bool ParenthesesCountValid(string whereArguments)
        {
            var parenthesisCount = 0;
            foreach (var c in whereArguments)
            {
                if (c == '(')
                    parenthesisCount++;
                if (c == ')')
                    parenthesisCount--;
                if (parenthesisCount < 0)
                    return false;
            }
            return parenthesisCount == 0;
        }
    }
}
