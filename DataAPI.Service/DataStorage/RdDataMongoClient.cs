using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace DataAPI.Service.DataStorage
{
    public class RdDataMongoClient
    {
        private IMongoClient Client { get; }
        public IMongoDatabase DataDatabase { get; }
        public IMongoDatabase BackendDatabase { get; }

        public RdDataMongoClient(
            IEnumerable<string> serverAddresses, 
            string replicaSetName,
            string databaseName, 
            string backendDataDatabaseName,
            string username, 
            string password)
        {
            SetupConventions();
            Client = new MongoClient(new MongoClientSettings
            {
                ConnectionMode = ConnectionMode.ReplicaSet,
                ReplicaSetName = replicaSetName,
                Servers = serverAddresses.Select(MongoServerAddress.Parse).ToList(),
                Credential = MongoCredential.CreateCredential("admin", username, password)
            });
            DataDatabase = Client.GetDatabase(databaseName);
            BackendDatabase = Client.GetDatabase(backendDataDatabaseName);
        }

        private static bool isConventionSetup;
        private static void SetupConventions()
        {
            if(isConventionSetup)
                return;
            var enumConventionPack = new ConventionPack {new EnumRepresentationConvention(BsonType.String)};
            ConventionRegistry.Register("EnumSerializationConvention", enumConventionPack, t => t.IsEnum);
            isConventionSetup = true;
        }
    }
}

