using System;
using System.Diagnostics;
using System.IO;
using DataAPI.DataStructures.Validation;

namespace DataAPI.Service.Validators
{
    public class ExecutableValidator : IValidator
    {
        private readonly string scriptPath;

        public ExecutableValidator(ValidatorDefinition definition, string scriptPath)
        {
            this.scriptPath = scriptPath;
            Definition = definition;
        }

        public ValidatorDefinition Definition { get; }

        public ValidationResult Validate(string json)
        {
            var processStartInfo = new ProcessStartInfo(scriptPath, json)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(processStartInfo))
            {
                if(process == null)
                    throw new NullReferenceException($"Execution of validator script '{Path.GetFileName(scriptPath)}' for type '{Definition.DataType}' failed");
                var isValid = process.ExitCode == 0;
                if (!isValid)
                {
                    var errorText = process.StandardOutput.ReadToEnd();
                    return ValidationResult.Invalid(errorText, Definition.Id, Definition.SubmitterEmail);
                }
                return ValidationResult.Valid();
            }
        }
    }
}
