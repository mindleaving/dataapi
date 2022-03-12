using System.Collections.Generic;
using System.Linq;
using Commons.Misc;
using DataAPI.DataStructures;
using DataAPI.Service;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.SubscriptionManagement;
using DataAPI.Service.Validators;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{
    public class StoreSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            var dbUserName = configuration["MongoDB:Username"];
            var password = Secrets.Get(configuration["MongoDB:PasswordEnvironmentVariable"]);
            var dataServerAddresses = configuration["MongoDB:Servers"].Split(',');
            var replicaSetName = configuration["MongoDB:ReplicaSetName"];
            var dataDatabaseName = configuration["MongoDB:DataDatabaseName"];
            var backendDataDatabaseName = configuration["MongoDB:BackendDataDatabaseName"];
            services.AddSingleton(new RdDataMongoClient(dataServerAddresses, replicaSetName, dataDatabaseName, backendDataDatabaseName, dbUserName, password));

            var authorizationDatabaseName = configuration["MongoDB:AuthorizationDatabaseName"];
            services.AddSingleton(new AccessControlMongoClient(dataServerAddresses, replicaSetName, authorizationDatabaseName, dbUserName, password));

            services.AddSingleton<IdGeneratorManager>();
            services.AddSingleton(
                provider =>
                {
                    var rdDataMongoClient = provider.GetService<RdDataMongoClient>();
                    var idGeneratorManager = provider.GetService<IdGeneratorManager>();
                    var rdDataStorages = new List<IRdDataStorage>
                    {
                        new MongoDbRdDataStorage(DataStorageTypes.MongoDB, rdDataMongoClient, idGeneratorManager)
                    };
                    SetupFileSystemStorages(rdDataStorages, idGeneratorManager, configuration);
                    SetupAzureBlobStorages(rdDataStorages, idGeneratorManager, configuration);
                    SetupSqlStorages(rdDataStorages, idGeneratorManager, configuration);
                    return rdDataStorages;
                });

            services.AddSingleton<CollectionInformationManager>();
            services.AddSingleton<IDataRouter>(provider => new DataRouter(provider.GetService<RdDataMongoClient>(), DataStorageTypes.MongoDB, provider.GetService<List<IRdDataStorage>>()));
            services.AddSingleton<ValidatorFactory>();
            services.AddSingleton<ViewManager>();
            services.AddSingleton<ValidatorManager>();
            services.AddSingleton<SubscriptionManager>();
            
        }

        private void SetupFileSystemStorages(
            List<IRdDataStorage> rdDataStorages, 
            IdGeneratorManager idGeneratorManager,
            IConfiguration configuration)
        {
            var fileSystemStoragePath = configuration["FileSystem:Path"];
            if (!string.IsNullOrEmpty(fileSystemStoragePath))
            {
                var fileSystemBinaryDataStorage = new FileSystemBinaryDataStorage(fileSystemStoragePath);
                var fileSystemStorage = new BlobRdDataStorage(
                    DataStorageTypes.FileSystem,
                    fileSystemBinaryDataStorage,
                    rdDataStorages.Single(x => x.Id == DataStorageTypes.MongoDB),
                    idGeneratorManager);
                rdDataStorages.Add(fileSystemStorage);
            }
        }

        private void SetupAzureBlobStorages(
            List<IRdDataStorage> rdDataStorages, 
            IdGeneratorManager idGeneratorManager,
            IConfiguration configuration)
        {
            var azureBlobStorageAccountName = configuration["AzureBlobStorage:User"];
            if (!string.IsNullOrWhiteSpace(azureBlobStorageAccountName))
            {
                var accessKey = Secrets.Get(configuration["AzureBlobStorage:AccessKeyEnvironmentVariable"]);
                var azureCloudAccount = new CloudStorageAccount(
                    new StorageCredentials(azureBlobStorageAccountName, accessKey),
                    useHttps: true);
                var azureBlobStorage = new AzureBinaryDataStorage(azureCloudAccount);
                var rdAzureBlobStorage = new BlobRdDataStorage(
                    DataStorageTypes.AzureBlobStorage,
                    azureBlobStorage,
                    rdDataStorages.Single(x => x.Id == DataStorageTypes.MongoDB),
                    idGeneratorManager);
                rdDataStorages.Add(rdAzureBlobStorage);
            }
        }

        private void SetupSqlStorages(
            List<IRdDataStorage> rdDataStorages, 
            IdGeneratorManager idGeneratorManager,
            IConfiguration configuration)
        {
            var sqlTableSetups = GetSqlTableSetupsFromConfiguration(configuration);
            if (sqlTableSetups.Any())
            {
                var existingSqlTablesStorage = new ExistingMssqlTablesRdDataStorage(
                    DataStorageTypes.ExistingSQL,
                    rdDataStorages.Single(x => x.Id == DataStorageTypes.MongoDB),
                    sqlTableSetups);
                rdDataStorages.Add(existingSqlTablesStorage);
            }

            var genericSqlDatabaseServer = configuration["SQL:GenericDatabase:Server"];
            if (!string.IsNullOrWhiteSpace(genericSqlDatabaseServer))
            {
                var sqlRdDataStorage = new MssqlRdDataStorage(
                    DataStorageTypes.GenericSQL,
                    genericSqlDatabaseServer,
                    configuration["SQL:GenericDatabase:DatabaseName"],
                    configuration["SQL:GenericDatabase:Username"],
                    Secrets.Get(configuration["SQL:GenericDatabase:PasswordEnvironmentVariable"]),
                    idGeneratorManager);
                rdDataStorages.Add(sqlRdDataStorage);
            }
        }

        private List<SqlTableSetup> GetSqlTableSetupsFromConfiguration(IConfiguration configuration)
        {
            var index = 0;
            var sqlTableSetups = new List<SqlTableSetup>();
            while (configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.DataType)}"] != null)
            {
                var sqlTableSetup = new SqlTableSetup(
                    configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.DataType)}"],
                    configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.Server)}"],
                    configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.TableName)}"],
                    configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.Username)}"],
                    configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.PasswordEnvironmentVariable)}"],
                    configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.IdColumnName)}"]);
                sqlTableSetups.Add(sqlTableSetup);
                index++;
            }

            return sqlTableSetups;
        }
    }
}
