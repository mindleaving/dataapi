using System.Linq;
using Commons.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public static class SqlUpdateStatement
    {
        public static string CreateFromContainer(string dataType, GenericDataContainer container)
        {
            var keyValuePairs = GenericDataContainerTraversal.Traverse(container);
            var query = $"UPDATE {dataType} SET ";
            var firstItem = true;
            foreach (var (key, value) in keyValuePairs)
            {
                if(key.InSet("Id", "CreatedTimeUtc", "OriginalSubmitter"))
                    continue;
                if (!firstItem)
                    query += ", ";
                else
                    firstItem = false;
                if(key == "Id")
                    query += $"{key}={value}";
                else
                    query += $"{key}='{value}'";
            }
            query += $" WHERE Id='{container.Id}'";
            return query;
        }

        public static string CreateFromDataAndTableSetup(BsonDocument data, SqlTableSetup tableSetup, string id)
        {
            var jObject = JObject.Parse(DataEncoder.DecodeToJson(data));
            var properties = jObject.Properties().Where(property => property.Name != tableSetup.IdColumnName).ToList();
            var keyValuePairs = string.Join(", ", properties.Where(property => property.Name != tableSetup.IdColumnName)
                    .ToDictionary(property => property.Name, property => JTokenStringify.Stringify(property.Value))
                    .Select(kvp => $"{kvp.Key} = '{kvp.Value}'"));
            var query = $"UPDATE {tableSetup.TableName} SET {keyValuePairs} WHERE {tableSetup.IdColumnName} = '{id}'";
            return query;
        }
    }
}
