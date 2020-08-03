using System;

namespace DataAPI.Client.Repositories
{
    public static class WhereClauseCombiner
    {
        public static string CombinedWhereClause(string parsedPermanentFilter, string sqlWhereClause)
        {
            var combinedWhereClause = parsedPermanentFilter;
            if (sqlWhereClause != null)
            {
                if (!string.IsNullOrEmpty(combinedWhereClause))
                    combinedWhereClause += $" AND ({sqlWhereClause})";
                else
                    combinedWhereClause = sqlWhereClause;
            }

            return combinedWhereClause;
        }
    }
}
