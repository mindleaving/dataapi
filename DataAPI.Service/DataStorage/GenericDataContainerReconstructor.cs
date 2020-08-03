using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using DataAPI.Service.Helpers;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public static class GenericDataContainerReconstructor
    {
        public static GenericDataContainer Reconstruct(SqlDataReader reader, SqlTableSetup tableSetup = null)
        {
            return BuildContainer(reader, tableSetup);
        }

        private static GenericDataContainer BuildContainer(SqlDataReader reader, SqlTableSetup tableSetup = null)
        {
            var id = (string) reader["Id"];
            var originalSubmitter = (string) reader["OriginalSubmitter"];
            var createdTimeUtc = (DateTime) reader["CreatedTimeUtc"];
            var submitter = (string) reader["Submitter"];
            var submissionTimeUtc = (DateTime) reader["SubmissionTimeUtc"];
            var data = BuildData(reader);
            return new GenericDataContainer(
                id,
                originalSubmitter,
                createdTimeUtc,
                submitter,
                submissionTimeUtc,
                ApiVersion.Current,
                data);
        }

        private static BsonDocument BuildData(SqlDataReader reader)
        {
            var columnNames = reader.GetColumnSchema()
                .Select(x => x.ColumnName)
                .Where(columnName => columnName.StartsWith("Data" + GenericDataContainerTraversal.PathDelimiter));
            var jObject = new JObject();
            foreach (var columnName in columnNames)
            {
                var splittedColumnName = columnName.Split(GenericDataContainerTraversal.PathDelimiter);
                var propertyPath = string.Join('.', splittedColumnName.Skip(1));
                var propertyName = splittedColumnName.Last();
                var parent = jObject.EnsureParent(propertyPath);
                var value = reader[columnName] as string;
                parent.Add(propertyName, value);
            }
            return DataEncoder.Encode(jObject.ToString());
        }
    }
}
