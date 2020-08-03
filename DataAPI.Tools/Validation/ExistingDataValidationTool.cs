using System;
using System.IO;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DataAPI.Tools.Validation
{
    [TestFixture]
    public class ExistingDataValidationTool : DataApiAccess
    {
        [Test]
        [TestCase("<NAME OF COLLECTION>")]
        public async Task ValidateExistingData(string dataType)
        {
            var outputDirectory = @"C:\temp\Validation";
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var limit = 10;
            var existingData = await dataApiClient.SearchAsync($"SELECT * FROM {dataType} ORDER BY SubmissionTimeUtc DESC LIMIT {limit}", ResultFormat.Json);
            var validators = await dataApiClient.GetAllValidatorDefinitionsAsync(dataType);
            await Client.Serialization.SeachResultStreamExtensions.ForEachSearchResult(existingData, jObject =>
            {
                var json = jObject["Data"].ToString();
                foreach (var validator in validators)
                {
                    var id = jObject["_id"].Value<string>();
                    try
                    {
                        dataApiClient.ApplyValidatorAsync(dataType, json, validator.Id).Wait();
                        Console.WriteLine($"Object '{id}' matches validator '{validator.Id}'");
                    }
                    catch (AggregateException aggregateException)
                    {
                        var apiException = aggregateException.InnerException as ApiException;
                        Console.WriteLine($"Object '{id}' _DOESN'T_ match validator '{validator.Id}'!");
                        var validationJson = JObject.Parse(apiException.Message);
                        File.WriteAllText(
                            $@"{outputDirectory}\{dataType}_{id}.json",
                            $"{jObject.ToString(Formatting.Indented)}\n{validationJson.ToString(Formatting.Indented)}");
                    }
                }
            });
        }
    }
}
