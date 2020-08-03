using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Commons.Misc;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Validation;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Tools.Validation
{
    [TestFixture]
    public class ValidatorGenerator : DataApiAccess
    {
        private readonly string outputFilePath = @"C:\temp\validators.json";
        private readonly TypeValidationRuleBuilder typeValidationRuleBuilder = new TypeValidationRuleBuilder();

        [Test]
        public async Task GenerateValidatorsForMyDataStructures()
        {
            if(File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            var iidClasses = Assembly.GetAssembly(typeof(IId)).GetTypes()
                .Where(t => t.IsInterface)
                .Where(t => typeof(IId).IsAssignableFrom(t))
                .Except(new []{ typeof(IId) })
                .ToList();
            var customTypes = iidClasses
                .Where(t => t.Namespace != null && t.Namespace.StartsWith("My.Namespace")) // TODO: Fill in your namespace
                .ToList();
            var validationDefinitions = new List<ValidatorDefinition>();
            foreach (var customType in customTypes)
            {
                var validatorDefinition = ExtractValidatorDefinition(customType);
                if(validatorDefinition == null)
                    continue;
                validationDefinitions.Add(validatorDefinition);
                //var dataType = DataApiClient.GetCollectionName(customType);
                //await SubmitValidator(dataType, validatorDefinition);
            }

            //var distinctValidations = validationDefinitions
            //    .GroupBy(x => x.DataType)
            //    .Select(group => group.OrderByDescending(validationDefinition => validationDefinition.Ruleset.Length).First())
            //    .ToList();
            await File.WriteAllLinesAsync(
                outputFilePath, 
                validationDefinitions.Select(validatorDefinition => JsonConvert.SerializeObject(validatorDefinition, Formatting.Indented))
            );
        }

        [Test]
        //TODO: [TestCase(typeof(XXX))]
        public async Task GenerateValidatorForSpecificType(Type customType)
        {
            var validatorDefinition = ExtractValidatorDefinition(customType);
            if(validatorDefinition == null)
                return;
            var dataType = DataApiClient.GetCollectionName(customType);
            await SubmitValidator(dataType, validatorDefinition);
            await File.AppendAllLinesAsync(outputFilePath, new[] {JsonConvert.SerializeObject(validatorDefinition, Formatting.Indented)});
        }

        private async Task SubmitValidator(string dataType, ValidatorDefinition validatorDefinition)
        {
            var existingTypeValidators = await dataApiClient.GetAllValidatorDefinitionsAsync(dataType);
            var initials = dataApiClient.LoggedInUsername;
            if (existingTypeValidators.Any(x => x.Submitter != initials))
                Assert.Fail($"Data type {dataType} has validators not from '{initials}'");
            foreach (var existingValidator in existingTypeValidators)
            {
                await dataApiClient.DeleteValidatorAsync(existingValidator.Id);
            }

            await dataApiClient.SubmitValidatorAsync(validatorDefinition, suppressAutoApprove: true);
        }

        [Test]
        //TODO: [TestCase()]
        public async Task GenerateValidatorForSqlTable(string dataType, string dataSource, string tablePath, string username, string passwordEnvironmentVariable)
        {
            //if(File.Exists(outputFilePath))
            //    File.Delete(outputFilePath);
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                UserID = username,
                Password = Secrets.Get(passwordEnvironmentVariable)
            }.ConnectionString;
            var rules = new List<string>();
            await using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                await sqlConnection.OpenAsync();
                await using var sqlCommand = new SqlCommand($"SELECT TOP 1 * FROM {tablePath}", sqlConnection);
                await using var reader = await sqlCommand.ExecuteReaderAsync();
                var columns = reader.GetColumnSchema();
                foreach (var column in columns.Where(column => !column.AllowDBNull.Value))
                {
                    if (column.IsIdentity.Value)
                    {
                        rules.Add($"{column.ColumnName} NOT EXISTS");
                    }
                    else
                    {
                        rules.Add($"{column.ColumnName} EXISTS");
                        rules.Add($"{column.ColumnName} IS NOT NULL");
                        if (column.DataTypeName == "int")
                        {
                            rules.Add($"{column.ColumnName} IS INT");
                        }
                        else if (column.DataTypeName == "datetime")
                        {
                            rules.Add($"{column.ColumnName} IS DATE");
                        }
                    }
                }
            }
            var ruleset = string.Join("; ", rules);
            var validatorDefinition = new ValidatorDefinition(dataType, ValidatorType.TextRules, ruleset);
            await SubmitValidator(dataType, validatorDefinition);
            await File.AppendAllLinesAsync(outputFilePath, new []{ JsonConvert.SerializeObject(validatorDefinition, Formatting.Indented) });
        }

        private ValidatorDefinition ExtractValidatorDefinition(Type type)
        {
            var rules = typeValidationRuleBuilder.Build(type, "");
            if (!rules.Any())
                return null;
            var ruleset = rules.Aggregate((a, b) => a + "; " + b);
            return new ValidatorDefinition(DataApiClient.GetCollectionName(type), ValidatorType.TextRules, ruleset);
        }


    }
}
