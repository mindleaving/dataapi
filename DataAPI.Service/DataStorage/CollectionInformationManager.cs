using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Validation;
using DataAPI.Service.AccessManagement;
using MongoDB.Driver;

namespace DataAPI.Service.DataStorage
{
    public class CollectionInformationManager
    {
        private readonly RdDataMongoClient rdDataClient;
        private readonly AuthorizationModule authorizationModule;
        private readonly IMongoCollection<CollectionMetadata> collectionMetadataCollection;
        private readonly IMongoCollection<ValidatorDefinition> validatorDefinitionCollection;

        public CollectionInformationManager(
            RdDataMongoClient rdDataClient, 
            AuthorizationModule authorizationModule)
        {
            this.rdDataClient = rdDataClient;
            this.authorizationModule = authorizationModule;
            collectionMetadataCollection = rdDataClient.BackendDatabase
                .GetCollection<CollectionMetadata>(nameof(CollectionMetadata));
            validatorDefinitionCollection = rdDataClient.BackendDatabase
                .GetCollection<ValidatorDefinition>(nameof(ValidatorDefinition));
        }

        public async Task<bool> IsHiddenAsync(string collectionName)
        {
            var collectionMetadata = await collectionMetadataCollection
                .Find(x => x.CollectionName == collectionName)
                .FirstOrDefaultAsync();
            return collectionMetadata?.IsHidden ?? false;
        }

        public async Task<CollectionInformation> GetCollectionInformationAsync(string collectionName, User user)
        {
            var collectionMetadata = await collectionMetadataCollection
                .Find(x => x.CollectionName == collectionName)
                .FirstOrDefaultAsync();
            var displayName = collectionMetadata?.DisplayName ?? collectionName;
            var description = collectionMetadata?.Description ?? string.Empty;
            var isProtected = collectionMetadata?.IsProtected ?? false;
            var nonAdminUsersCanOverwriteData = collectionMetadata?.NonAdminUsersCanOverwriteData ?? false;
            var isHidden = collectionMetadata?.IsHidden ?? false;
            var idGeneratorType = collectionMetadata?.IdGeneratorType ?? Conventions.DefaultIdGenerator;
            var userRoles = await authorizationModule.GetCollectionRolesForUserAsync(collectionName, user);
            var validatorDefinitions = await validatorDefinitionCollection
                .Find(x => x.DataType == collectionName)
                .ToListAsync();
            return new CollectionInformation(
                collectionName,
                displayName,
                description,
                isProtected,
                nonAdminUsersCanOverwriteData,
                isHidden,
                idGeneratorType,
                user.UserName,
                userRoles,
                validatorDefinitions);
        }
    }
}
