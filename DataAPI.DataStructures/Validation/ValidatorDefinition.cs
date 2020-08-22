using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.DataStructures.Validation
{
    public class ValidatorDefinition
    {
        [JsonConstructor]
        private ValidatorDefinition(string id,
            string dataType,
            string submitter,
            string submitterEmail,
            ValidatorType validatorType,
            string executableBase64,
            string ruleset,
            bool isApproved)
        {
            Id = id;
            DataType = dataType;
            Submitter = submitter;
            SubmitterEmail = submitterEmail;
            ValidatorType = validatorType;
            ExecutableBase64 = executableBase64;
            Ruleset = ruleset;
            IsApproved = isApproved;
        }

        public ValidatorDefinition(
            string dataType, 
            ValidatorType validatorType, 
            string rulesetOrExecutable)
        {
            Id = $"{dataType}_{Guid.NewGuid()}";
            DataType = dataType;
            ValidatorType = validatorType;
            if (validatorType == ValidatorType.JsonSchema || validatorType == ValidatorType.TextRules)
                Ruleset = rulesetOrExecutable;
            else
                ExecutableBase64 = rulesetOrExecutable;
        }

        [Required]
        public string Id { get; private set; }

        [Required]
        public string DataType { get; private set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Submitter { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string SubmitterEmail { get; set; }

        [Required]
        public ValidatorType ValidatorType { get; private set; }

        public string ExecutableBase64 { get; private set; }

        public string Ruleset { get; private set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public bool IsApproved { get; set; }
    }
}
