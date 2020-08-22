using DataAPI.DataStructures.Validation;
using DataAPI.Service.Validators;
using NUnit.Framework;

namespace DataAPI.Service.Test.Validators
{
    [TestFixture]
    public class RulesetValidatorDefinitionValidatorTest
    {
        [Test]
        public void InvalidJsonSchemaIsDetectedAsInvalid()
        {
            var ruleset = 
@"{
    notASchema: true
}";
            var validatorDefinition = new ValidatorDefinition("MyType", ValidatorType.JsonSchema, ruleset);
            var sut = new RulesetValidationDefinitionValidator(null);

            var actual = sut.Validate(validatorDefinition);

            Assert.That(actual.IsValid, Is.False);
        }
    }
}
