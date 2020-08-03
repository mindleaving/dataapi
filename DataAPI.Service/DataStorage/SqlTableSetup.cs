using System.Data.SqlClient;
using Commons.Misc;
using Newtonsoft.Json;

namespace DataAPI.Service.DataStorage
{
    public class SqlTableSetup
    {
        [JsonConstructor]
        public SqlTableSetup(
            string dataType,
            string server,
            string tableName,
            string username,
            string passwordEnvironmentVariable,
            string idColumnName)
        {
            DataType = dataType;
            Server = server;
            TableName = tableName;
            Username = username;
            PasswordEnvironmentVariable = passwordEnvironmentVariable;
            IdColumnName = idColumnName;

            ConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = server,
                UserID = username,
                Password = Secrets.Get(passwordEnvironmentVariable)
            }.ConnectionString;
            QueryExecutor = new SqlQueryExecutor(ConnectionString);
            ReservationContainerBuilder = new ReservationContainerBuilder(QueryExecutor, TableName);
        }

        public string DataType { get; }
        public string Server { get; }
        public string TableName { get; }
        public string Username { get; }
        public string PasswordEnvironmentVariable { get; }
        public string IdColumnName { get; }

        [JsonIgnore]
        public string ConnectionString { get; }
        [JsonIgnore]
        public SqlQueryExecutor QueryExecutor { get; }
        [JsonIgnore]
        public ReservationContainerBuilder ReservationContainerBuilder { get; }

        public GenericDataContainer BuildReservationContainer(string submitter)
        {
            return ReservationContainerBuilder.Build(submitter);
        }
    }
}