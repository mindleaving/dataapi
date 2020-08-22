using System;
using System.IO;
using DataAPI.DataStructures.Validation;
using DataAPI.Service.DataRouting;

namespace DataAPI.Service.Validators
{
    public class ValidatorFactory
    {
        private const string ValidatorScriptsDirectory = @"C:\Temp\DataApiValidatorScripts";
        private readonly TextRuleEvaluator textRuleEvaluator;

        public ValidatorFactory(IDataRouter dataRouter)
        {
            textRuleEvaluator = new TextRuleEvaluator(dataRouter);
        }

        public IValidator Create(ValidatorDefinition validatorDefinition)
        {
            switch (validatorDefinition.ValidatorType)
            {
                case ValidatorType.PythonScript:
                    return CreatePythonScriptValidator(validatorDefinition);
                case ValidatorType.Exe:
                    return CreateExecutableValidator(validatorDefinition);
                case ValidatorType.JsonSchema:
                    return CreateJsonRulesetValidator(validatorDefinition);
                case ValidatorType.TextRules:
                    return CreateTextRulesetValidator(validatorDefinition);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IValidator CreatePythonScriptValidator(ValidatorDefinition validatorDefinition)
        {
            var extension = ".py";
            var scriptPath = StoreScript(validatorDefinition, extension);
            return new PythonScriptValidator(validatorDefinition, scriptPath);
        }

        private IValidator CreateExecutableValidator(ValidatorDefinition validatorDefinition)
        {
            var extension = ".exe";
            var scriptPath = StoreScript(validatorDefinition, extension);
            return new ExecutableValidator(validatorDefinition, scriptPath);
        }

        private string StoreScript(ValidatorDefinition validatorDefinition, string extension)
        {
            if (!Directory.Exists(ValidatorScriptsDirectory))
                Directory.CreateDirectory(ValidatorScriptsDirectory);
            var scriptBytes = Convert.FromBase64String(validatorDefinition.ExecutableBase64);
            var scriptPath = Path.Combine(ValidatorScriptsDirectory, validatorDefinition.Id + extension);
            File.WriteAllBytes(scriptPath, scriptBytes);
            return scriptPath;
        }

        private IValidator CreateJsonRulesetValidator(ValidatorDefinition validatorDefinition)
        {
            return new JsonSchemaValidator(validatorDefinition);
        }

        private IValidator CreateTextRulesetValidator(ValidatorDefinition validatorDefinition)
        {
            return new TextRulesValidator(validatorDefinition, textRuleEvaluator);
        }
    }
}
