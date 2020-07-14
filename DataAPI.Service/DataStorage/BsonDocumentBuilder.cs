using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using DataAPI.Service.Helpers;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public static class BsonDocumentBuilder
    {
        public static BsonDocument BuildFromDelimitedColumns(SqlDataReader reader, SqlTableSetup tableSetup = null)
        {
            var columnNames = reader.GetColumnSchema().Select(x => x.ColumnName);
            var jObject = new JObject();
            foreach (var columnName in columnNames)
            {
                var propertyName = GetPropertyName(columnName, tableSetup);
                jObject.EnsureParent(propertyName);
                var value = reader[columnName];
                jObject[propertyName] = value == null || value is DBNull ? null : value.ToString();
            }
            return DataEncoder.Encode(jObject.ToString());
        }

        private static string GetPropertyName(string columnName, SqlTableSetup tableSetup)
        {
            return columnName.Replace(GenericDataContainerTraversal.PathDelimiter, '.');
        }
    }
}
