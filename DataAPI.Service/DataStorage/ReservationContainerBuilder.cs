using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public class ReservationContainerBuilder
    {
        private readonly List<SqlColumnInfo> columnInfos;

        public ReservationContainerBuilder(SqlQueryExecutor queryExecutor, string tableName)
        {
            columnInfos = Task.Run(async () => await GetTableSchema(queryExecutor, tableName)).Result;
        }

        private async Task<List<SqlColumnInfo>> GetTableSchema(SqlQueryExecutor queryExecutor, string tablePath)
        {
            var databaseName = tablePath.Split('.').First();
            var tableName = tablePath.Split('.').Last().Replace("[", "").Replace("]", "");
            var query = "SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE "
                        + $"FROM {databaseName}.INFORMATION_SCHEMA.COLUMNS "
                        + $"WHERE TABLE_NAME = '{tableName}'";
            var columns = queryExecutor.ExecuteReaderAsync(query);
            var columnInformation = new List<SqlColumnInfo>();
            await foreach (var column in columns)
            {
                var columnName = (string) column["COLUMN_NAME"];
                var isNullable = column["IS_NULLABLE"].Equals("YES");
                var dataType = (string) column["DATA_TYPE"];
                columnInformation.Add(new SqlColumnInfo(columnName, isNullable, dataType));
            }

            return columnInformation;
        }

        public GenericDataContainer Build(string submitter)
        {
            var utcNow = DateTime.UtcNow;
            var data = BuildData();
            return new GenericDataContainer(
                null,
                submitter,
                utcNow,
                submitter, 
                utcNow,
                ApiVersion.Current,
                data);
        }

        private BsonDocument BuildData()
        {
            var jObject = new JObject();
            foreach (var columnInfo in columnInfos.Where(columnInfo => !columnInfo.IsNullable))
            {
                JToken value;
                if (columnInfo.DataType == "bit")
                    value = 0;
                else if (columnInfo.DataType.EndsWith("int") || columnInfo.DataType.EndsWith("money") || columnInfo.DataType.InSet("float","numeric","decimal","real"))
                    value = 0;
                else if (columnInfo.DataType.Contains("date") || columnInfo.DataType.Contains("time"))
                    value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                else if (columnInfo.DataType.Contains("char") || columnInfo.DataType.Contains("text"))
                    value = "reserved";
                else if (columnInfo.DataType == "uniqueidentifier")
                    value = Guid.NewGuid();
                else
                    throw new NotSupportedException($"SQL data type '{columnInfo.DataType}' is not supported in '{nameof(ReservationContainerBuilder)}'");

                jObject.Add(columnInfo.ColumnName, value);
            }
            return BsonDocument.Parse(jObject.ToString());
        }

        private class SqlColumnInfo
        {
            public SqlColumnInfo(string columnName, bool isNullable, string dataType)
            {
                ColumnName = columnName;
                IsNullable = isNullable;
                DataType = dataType.ToLowerInvariant();
            }

            public string ColumnName { get; }
            public bool IsNullable { get; }
            public string DataType { get; }
        }
    }
}
