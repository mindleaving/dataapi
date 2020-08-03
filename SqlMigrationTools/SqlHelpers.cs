using System;
using System.Collections.Generic;
using System.Data;

namespace SqlMigrationTools
{
    public static class SqlHelpers
    {
        public static Guid? ReadGuid(IDataRecord reader, string columnName)
        {
            var columnIdx = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(columnIdx) ? reader.GetGuid(columnIdx) : (Guid?)null;
        }

        public static string ReadString(IDataRecord reader, string columnName)
        {
            var columnIdx = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(columnIdx) ? reader.GetString(columnIdx) : null;
        }

        public static double? ReadDouble(IDataRecord reader, string columnName)
        {
            var columnIdx = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(columnIdx))
                return null;
            var value = reader[columnIdx];
            if (value is decimal decimalValue)
                return decimal.ToDouble(decimalValue);
            return (double) value;
        }

        public static int? ReadInt(IDataRecord reader, string columnName)
        {
            var columnIdx = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(columnIdx) ? reader.GetInt32(columnIdx) : (int?)null;
        }

        public static DateTime? ReadDateTime(IDataRecord reader, string columnName)
        {
            var columnIdx = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(columnIdx) ? reader.GetDateTime(columnIdx) : (DateTime?)null;
        }

        public static string BuildInsertQuery(string tableName, Dictionary<string, string> values)
        {
            var headerNames = new List<string>();
            var valueStrings = new List<string>();
            foreach (var kvp in values)
            {
                headerNames.Add(kvp.Key);
                valueStrings.Add(kvp.Value != null ? $"'{kvp.Value}'" : "NULL");
            }

            var aggregatedHeaders = String.Join(",", headerNames);
            var aggregatedValues = String.Join(",", valueStrings);
            return $"INSERT INTO {tableName} ({aggregatedHeaders}) VALUES ({aggregatedValues})";
        }
    }
}
