using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures.Validation;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using MongoDB.Driver;
using Newtonsoft.Json.Schema;

namespace DataAPI.Service.Validators
{
    public class ValidatorManager
    {
        private readonly RdDataMongoClient mongoClient;
        private readonly IMongoCollection<ValidatorDefinition> validatorDefinitionCollection;
        private readonly ValidatorFactory validatorFactory;
        private readonly RulesetValidationDefinitionValidator validationDefinitionValidator;

        public ValidatorManager(RdDataMongoClient mongoClient, ValidatorFactory validatorFactory)
        {
            this.mongoClient = mongoClient;
            this.validatorFactory = validatorFactory;
            validationDefinitionValidator = new RulesetValidationDefinitionValidator(validatorFactory);
            validatorDefinitionCollection = mongoClient.BackendDatabase.GetCollection<ValidatorDefinition>(nameof(ValidatorDefinition));
        }

        public IValidator GetValidator(string validatorId)
        {
            var validatorDefinition = validatorDefinitionCollection.Find(x => x.Id == validatorId).FirstOrDefault();
            if (validatorDefinition == null)
                return null;
            return validatorFactory.Create(validatorDefinition);
        }

        public async Task<List<IValidator>> GetApprovedValidators(string typeName)
        {
            var validatorDefinitions = await validatorDefinitionCollection
                .Find(x => x.DataType == typeName && x.IsApproved)
                .ToListAsync();
            var validators = validatorDefinitions.Select(validatorFactory.Create).ToList();
            return validators;
        }

        public async Task<List<ValidatorDefinition>> GetUnapprovedValidatorDefinitions()
        {
            return await validatorDefinitionCollection
                .Find(x => !x.IsApproved)
                .ToListAsync();
        }

        public async Task<List<ValidatorDefinition>> GetAllValidatorDefinitions(string dataType = null)
        {
            if (dataType != null)
            {
                return await validatorDefinitionCollection
                    .Find(x => x.DataType == dataType)
                    .ToListAsync();
            }
            return await validatorDefinitionCollection
                .Find(x => true)
                .ToListAsync();
        }

        public ValidatorDefinition GetValidatorDefinition(string validatorId)
        {
            return validatorDefinitionCollection.Find(x => x.Id == validatorId).FirstOrDefault();
        }

        public void AddValidatorDefinition(ValidatorDefinition validatorDefinition, bool suppressAutoApprove = false)
        {
            try
            {
                validatorDefinitionCollection.ReplaceOne(
                    x => x.Id == validatorDefinition.Id, 
                    validatorDefinition, 
                    new ReplaceOptions { IsUpsert = true});
            }
            catch (MongoWriteException writeException)
            {
                if(writeException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    throw new DocumentAlreadyExistsException($"Validator with ID '{validatorDefinition.Id}' for type '{validatorDefinition.DataType}' already exists");
                throw;
            }
            if (!validatorDefinition.IsApproved && !suppressAutoApprove)
            {
                Task.Run(() =>
                {
                    if (CanAutoApprove(validatorDefinition))
                        ApproveValidator(validatorDefinition.Id);
                });
            }
        }

        public bool ApproveValidator(string validatorId)
        {
            var updateResult = validatorDefinitionCollection.UpdateOne(x => x.Id == validatorId, Builders<ValidatorDefinition>.Update.Set(x => x.IsApproved, true));
            return updateResult.IsAcknowledged && updateResult.MatchedCount == 1;
        }

        public bool UnapproveValidator(string validatorId)
        {
            var updateResult = validatorDefinitionCollection.UpdateOne(x => x.Id == validatorId, Builders<ValidatorDefinition>.Update.Set(x => x.IsApproved, false));
            return updateResult.IsAcknowledged && updateResult.MatchedCount == 1;
        }

        public bool DeleteValidator(string validatorId)
        {
            var deleteResult = validatorDefinitionCollection.DeleteOne(x => x.Id == validatorId);
            return deleteResult.IsAcknowledged && deleteResult.DeletedCount == 1;
        }

        private bool CanAutoApprove(ValidatorDefinition validatorDefinition)
        {
            var validator = validatorFactory.Create(validatorDefinition);
            var targetCollection = mongoClient.DataDatabase.GetCollection<GenericDataContainer>(validatorDefinition.DataType);
            using var documents = targetCollection.Find(x => true).ToCursor();
            while (documents.MoveNext())
            {
                var batch = documents.Current;
                foreach (var document in batch)
                {
                    if (!validator.Validate(DataEncoder.DecodeToJson(document.Data)).IsValid)
                        return false;
                }
            }
            return true;
        }

        public RulesetValidationResult ValidateValidatorDefinition(ValidatorDefinition validatorDefinition)
        {
            return validationDefinitionValidator.Validate(validatorDefinition);
        }
    }
}
