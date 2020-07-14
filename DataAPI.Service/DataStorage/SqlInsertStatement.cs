using System.Linq;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public static class SqlInsertStatement
    {
        public static string CreateFromContainer(string dataType, GenericDataContainer container)
        {
            var keyValuePairs = GenericDataContainerTraversal.Traverse(container);
            var query = $"INSERT INTO {dataType} ({string.Join(", ", keyValuePairs.Keys)}) "
                + $"VALUES ({string.Join(", ", keyValuePairs.Select(kvp => $"'{kvp.Value}'"))})";
            return query;
        }

        public static string CreateFromDataAndTableSetup(BsonDocument data, SqlTableSetup tableSetup)
        {
            var jObject = JObject.Parse(DataEncoder.DecodeToJson(data));
            var properties = jObject.Properties().Where(property => property.Name != tableSetup.IdColumnName).ToList();
            var query = $"INSERT INTO {tableSetup.TableName} ({string.Join(", ", properties.Select(property => property.Name))}) "
                        + $"OUTPUT INSERTED.{tableSetup.IdColumnName} "
                        + $"VALUES ({string.Join(", ", properties.Select(property => $"'{JTokenStringify.Stringify(property.Value)}'"))})";
            return query;
        }
    }
}
