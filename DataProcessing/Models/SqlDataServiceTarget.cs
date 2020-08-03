using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DomainModels;

namespace DataProcessing.Models
{
    public class SqlDataServiceTarget : IDataServiceTarget
    {
        public SqlDataServiceTarget(
            string id,
            string dataSource,
            string databaseName,
            string tableName,
            string username)
        {
            Id = id;
            DataSource = dataSource;
            DatabaseName = databaseName;
            TableName = tableName;
            Username = username;
        }

        public string Id { get; }
        public DataServiceTargetType Type { get; } = DataServiceTargetType.Sql;
        public string DataSource { get; }
        public string DatabaseName { get; }
        public string TableName { get; }
        public string Username { get; }

        public override string ToString()
        {
            return $"{Type} {DataSource} {DatabaseName} {TableName} ({Username})";
        }
    }
}