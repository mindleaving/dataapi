using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace DataAPI.Service
{
    public class AccessControlMongoClient
    {
        private IMongoClient Client { get; }
        public IMongoDatabase Database { get; }

        public AccessControlMongoClient(
            IEnumerable<string> serverAddresses, 
            string replicaSetName,
            string databaseName, 
            string username, 
            string password)
        {
            SetupConventions();
            var useReplicaSet = !string.IsNullOrEmpty(replicaSetName);
            Client = new MongoClient(new MongoClientSettings
            {
                ConnectionMode = useReplicaSet ? ConnectionMode.ReplicaSet : ConnectionMode.Direct,
                ReplicaSetName = useReplicaSet ? replicaSetName : null,
                Servers = serverAddresses.Select(MongoServerAddress.Parse).ToList(),
                Credential = MongoCredential.CreateCredential("admin", username, password)
            });
            Database = Client.GetDatabase(databaseName);
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