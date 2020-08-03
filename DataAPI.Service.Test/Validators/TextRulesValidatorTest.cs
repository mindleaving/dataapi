using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures.Validation;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Validators;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DataAPI.Service.Test.Validators
{
    [TestFixture]
    public class TextRulesValidatorTest
    {
        private ValidatorTestObject TestObject { get; } = ValidatorTestObjectGenerator.GenerateTestObject();

        [Test]
        [TestCase("")]
        [TestCase("Id EXISTS")]
        [TestCase("Id exists")]
        [TestCase("Name IS TEXT")]
        [TestCase("Name is Text")]
        [TestCase("Products IS LIST")]
        [TestCase("Id IS NOT EMPTY")]
        [TestCase("Categories IS LIST; Categories IS NOT EMPTY; Categories HAS MORE THAN 2 ITEMS;")]
        [TestCase("Title EXISTS")]
        [TestCase("Name IS NUMBER")]
        [TestCase("Name Is number")]
        [TestCase("  Name  Is    number")]
        [TestCase("Name IS LIST")]
        [TestCase("Name\tIS   LIST")]
        [TestCase("Category.Name IS STRING")]
        [TestCase("Products.Price IS LESS THAN 100")]
        [TestCase("GrowthResults.Measurements.Concentration IS NOT NULL IF Additive IS NOT EQUAL TO 'None'")]
        [TestCase("Id IS NOT EMPTY; GrowthResults EXISTS; GrowthResults IS NOT EMPTY; GrowthResults.PlateId IS NOT EMPTY; GrowthResults.WellId EXISTS; GrowthResults.CHCCNumber EXISTS; GrowthResults.Date IS DATE; GrowthResults.IncubationCondition IS NOT EMPTY; GrowthResults.InoculationMethod IS NOT EMPTY; GrowthResults.InoculumDilution EXISTS; GrowthResults.InoculumDilution IS BETWEEN 0 AND 1; GrowthResults.Media IS NOT EMPTY; GrowthResults.IsPreparedWithReplicator EXISTS; GrowthResults.Experimenter IS NOT EMPTY; GrowthResults.Experimenter MATCHES [a-zA-Z]{5,6}; GrowthResults.PlateType IS NOT EMPTY; GrowthResults.Measurements EXISTS; GrowthResults.Measurements IS NOT EMPTY; GrowthResults.Measurements.MeasurementType IS NOT EMPTY; GrowthResults.Measurements.Additive IS NOT EMPTY; GrowthResults.Measurements.Concentration IS NOT NULL IF Additive IS NOT EQUAL TO 'None'; GrowthResults.Measurements.Concentration IS BETWEEN 0 AND 1 OR IS NaN IF GrowthResults.Measurements.Concentration EXISTS; GrowthResults.Measurements.Value EXISTS; Filename IS NOT EMPTY")]
        public void RuleSyntaxIsValid(string ruleset)
        {
            var sut = CreateTextRulesValidator(ruleset, out _);
            var rulesetValidationResult = sut.IsRulesetSyntaxValid(ruleset);
            Assert.That(rulesetValidationResult.IsValid, Is.True);
        }

        [Test]
        [TestCase("I'm not a ruleset")]
        [TestCase("Field1 IS TEXT; Field2 does not follow rules")]
        public void RuleSyntaxIsInvalid(string ruleset)
        {
            var sut = CreateTextRulesValidator(ruleset, out _);
            var rulesetValidationResult = sut.IsRulesetSyntaxValid(ruleset);
            Assert.That(rulesetValidationResult.IsValid, Is.False);
        }

        [Test]
        [TestCase("")]
        [TestCase("Id EXISTS")]
        [TestCase("Title NOT EXISTS")]
        [TestCase("Name IS TEXT")]
        [TestCase("Products IS LIST")]
        [TestCase("Id IS NOT EMPTY")]
        [TestCase("Categories IS LIST; Categories IS NOT EMPTY; Categories HAS MORE THAN 2 ITEMS; Products HAS 2 ITEMS;")]
        [TestCase("Products.Price IS LESS THAN 100")]
        [TestCase("Products.Title IS NOT EMPTY")]
        [TestCase("NonExistingField.Subfield IS LESS THAN 100")] // Rule is ignored because parent doesn't exist
        [TestCase("EmptyString IS EMPTY")]
        public void TestObjectSatisfiesRules(string rules)
        {
            var sut = CreateTextRulesValidator(rules, out _);
            var validationResult = sut.Validate(JsonConvert.SerializeObject(TestObject));
            Assert.That(validationResult.IsValid, Is.True, validationResult.ErrorDescription);
        }

        [Test]
        [TestCase("Title EXISTS")]
        [TestCase("Count NOT EXISTS")]
        [TestCase("Name IS NUMBER")]
        [TestCase("Name IS LIST")]
        [TestCase("Products.Price IS MORE THAN 200")]
        [TestCase("NonExistingField IS NULL")]
        [TestCase("Products.Title IS EMPTY")]
        [TestCase("EmptyString IS NOT EMPTY")]
        public void TestObjectRejectedByRules(string rules)
        {
            var sut = CreateTextRulesValidator(rules, out _);
            var validationResult = sut.Validate(JsonConvert.SerializeObject(TestObject));
            Assert.That(validationResult.IsValid, Is.False);
        }

        [Test]
        [TestCase("Value IS LESS THAN 8.5", 5.3)]
        [TestCase("Value IS LESS THAN OR EQUAL TO 6", 5.3)]
        [TestCase("Value IS LESS THAN OR EQUAL TO 6", 6)]
        [TestCase("Value IS GREATER THAN 8.5", 9.4)]
        [TestCase("Value IS GREATER THAN OR EQUAL TO 6", 9.4)]
        [TestCase("Value IS GREATER THAN OR EQUAL TO 6", 6)]
        [TestCase("Value IS BETWEEN 2.2 AND 8.5", 5.3)]
        [TestCase("Value IS BETWEEN 2 AND 8", 2)]
        [TestCase("Value IS BETWEEN 2 AND 8", 8)]
        [TestCase("Value IS BETWEEN -8 AND -2", -3)]
        [TestCase("Value IS BETWEEN -2 AND -8", -3)]
        [TestCase("Value IS BETWEEN 1 AND 2 OR IS NaN", double.NaN)]
        [TestCase("Value IS BETWEEN 1 AND 2 OR NaN", double.NaN)]
        [TestCase("Value IS NOT BETWEEN 1 AND 2 OR NaN", 3)]
        public void IsComparisonTrue(string rule, double value)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.True, validationResult.ErrorDescription);
        }

        [Test]
        [TestCase("Value IS LESS THAN 8.5", 9.8)]
        [TestCase("Value IS LESS THAN OR EQUAL TO 6", 9.8)]
        [TestCase("Value IS LESS THAN OR EQUAL TO 6", 6.01)]
        [TestCase("Value IS GREATER THAN 8.5", 7.1)]
        [TestCase("Value IS GREATER THAN OR EQUAL TO 6", 4.9)]
        [TestCase("Value IS GREATER THAN OR EQUAL TO 6", 5.99)]
        [TestCase("Value IS BETWEEN 2.2 AND 8.5", 2)]
        [TestCase("Value IS BETWEEN 2 AND 8", 1.99)]
        [TestCase("Value IS BETWEEN 2 AND 8", 8.01)]
        [TestCase("Value IS BETWEEN 1 AND 2 OR NaN", 3)]
        [TestCase("Value IS NOT BETWEEN 1 AND 2 OR NaN", 1.5)]
        public void IsComparisonFalse(string rule, double value)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.False);
        }

        [Test]
        [TestCase("Value HAS LENGTH GREATER THAN 3", "Pandora")]
        [TestCase("Value HAS LENGTH GREATER THAN OR EQUAL TO 3", "Pandora")]
        [TestCase("Value HAS LENGTH GREATER THAN OR EQUAL TO 3", "Tim")]
        [TestCase("Value HAS LENGTH LESS THAN 3", "cm")]
        [TestCase("Value HAS LENGTH LESS THAN OR EQUAL TO 3", "cm")]
        [TestCase("Value HAS LENGTH LESS THAN OR EQUAL TO 3", "Tim")]
        [TestCase("Value HAS LENGTH BETWEEN 3 AND 5", "Tim")]
        [TestCase("Value HAS LENGTH BETWEEN 3 AND 5", "Drive")]
        public void HasLengthTrue(string rule, string value)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.True);
        }

        [Test]
        [TestCase("Value HAS LENGTH GREATER THAN 3", "Tim")]
        [TestCase("Value HAS LENGTH GREATER THAN OR EQUAL TO 3", "cm")]
        [TestCase("Value HAS LENGTH LESS THAN 3", "Tim")]
        [TestCase("Value HAS LENGTH LESS THAN OR EQUAL TO 3", "Glas")]
        [TestCase("Value HAS LENGTH BETWEEN 3 AND 5", "Pandora")]
        public void HasLengthFalse(string rule, string value)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.False);
        }

        [Test]
        [TestCase("Value HAS MORE THAN 3 ITEMS", 5)]
        [TestCase("Value HAS MORE THAN OR EQUAL TO 3 ITEMS", 5)]
        [TestCase("Value HAS MORE THAN OR EQUAL TO 3 ITEMS", 3)]
        [TestCase("Value HAS LESS THAN 3 ITEMS", 2)]
        [TestCase("Value HAS LESS THAN OR EQUAL TO 3 ITEMS", 2)]
        [TestCase("Value HAS LESS THAN OR EQUAL TO 3 ITEMS", 3)]
        [TestCase("Value HAS BETWEEN 3 AND 5 ITEMS", 3)]
        [TestCase("Value HAS BETWEEN 3 AND 5 ITEMS", 5)]
        public void HasItemCountTrue(string rule, int itemCount)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", CreateListWithNItems(itemCount)));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.True);
        }

        [Test]
        [TestCase("Value HAS MORE THAN 3 ITEMS", 3)]
        [TestCase("Value HAS MORE THAN OR EQUAL TO 3 ITEMS", 2)]
        [TestCase("Value HAS LESS THAN 3 ITEMS", 3)]
        [TestCase("Value HAS LESS THAN OR EQUAL TO 3 ITEMS", 4)]
        [TestCase("Value HAS BETWEEN 3 AND 5 ITEMS", 2)]
        [TestCase("Value HAS BETWEEN 3 AND 5 ITEMS", 6)]
        public void HasItemCountFalse(string rule, int itemCount)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", CreateListWithNItems(itemCount)));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.False);
        }

        [Test]
        [TestCase("Value1 EXISTS IF Value2 IS GREATER THAN 2", 3)]
        [TestCase("Value1 NOT EXISTS IF Value2 IS GREATER THAN 2", 2)]
        public void ConditionalExistsTrue(string rule, int value2)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value1", "Test"), new JProperty("Value2", value2));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.True);
        }

        [Test]
        [TestCase("Value1 NOT EXISTS IF Value2 IS GREATER THAN 2", 3)]
        public void ConditionalExistsFalse(string rule, int value2)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value1", "Test"), new JProperty("Value2", value2));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.False);
        }

        [Test]
        public void DataReferenceExistsEvaluatesToTrue()
        {
            var dataType = "TestObject";
            var rule = $"Value IS REFERENCE TO {dataType}";
            var value = Guid.NewGuid().ToString();
            var sut = CreateTextRulesValidator(rule, out var dataStorageMock);
            dataStorageMock.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));
            dataStorageMock.Setup(x => x.ExistsAsync(dataType, value)).Returns(Task.FromResult(true));
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.True);
            dataStorageMock.Verify(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void DataReferenceExistsEvaluatesToFalse()
        {
            var dataType = "TestObject";
            var rule = $"Value IS REFERENCE TO {dataType}";
            var sut = CreateTextRulesValidator(rule, out var dataStorageMock);
            dataStorageMock.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));
            var jObject = new JObject(new JProperty("Value", Guid.NewGuid().ToString()));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.False);
            dataStorageMock.Verify(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        [TestCase("Value MATCHES foo", "foo")]
        [TestCase("Value MATCHES [a-z]", "abc")]
        [TestCase("Value MATCHES [a-z]{3,}", "abc")]
        [TestCase("Value MATCHES [a-z]{3,}\\s[a-z]+$", "abc def")]
        [TestCase("Value MATCHES ^[a-zA-Z0-9\\._-]+@[a-zA-Z0-9\\._-]+\\.[a-zA-Z0-9]+$", "jdoe.test-mail@mail.example.org")]
        public void MatchesTrue(string rule, string value)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.True);
        }

        [Test]
        [TestCase("Value MATCHES foo", "bar")]
        [TestCase("Value MATCHES [a-z]", "829")]
        public void MatchesFalse(string rule, string value)
        {
            var sut = CreateTextRulesValidator(rule, out _);
            var jObject = new JObject(new JProperty("Value", value));
            var validationResult = sut.Validate(JsonConvert.SerializeObject(jObject));
            Assert.That(validationResult.IsValid, Is.False);
        }

        private static TextRulesValidator CreateTextRulesValidator(string rule, out Mock<IRdDataStorage> dataStorage)
        {
            var validatorDefinition = new ValidatorDefinition(nameof(ValidatorTestObject), ValidatorType.TextRules, rule)
            {
                Submitter = "me",
                SubmitterEmail = "me@example.org"
            };
            var dataRouter = new Mock<IDataRouter>();
            dataStorage = new Mock<IRdDataStorage>();
            dataRouter.Setup(x => x.GetSourceSystemAsync(It.IsAny<string>())).Returns(Task.FromResult(dataStorage.Object));
            var textRuleEvaluator = new TextRuleEvaluator(dataRouter.Object);
            return new TextRulesValidator(validatorDefinition, textRuleEvaluator);
        }

        private List<string> CreateListWithNItems(int itemCount)
        {
            return Enumerable.Range(0, itemCount).Select(idx => Guid.NewGuid().ToString()).ToList();
        }
    }
}
