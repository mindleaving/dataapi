using System.Net;
using DataAPI.Client;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Validation;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class ValidatorControllerTest : ApiTestBase
    {
        [Test]
        public void AnonymousUserCannotSubmitValidator()
        {
            var validatorDefinition = new ValidatorDefinition("MyClass", ValidatorType.TextRules, "Id EXISTS");
            var noLoginDataApiClient = new DataApiClient(ApiSetup.ApiConfiguration) {LoginMethod = LoginMethod.JsonWebToken};
            AssertStatusCode(
                () => noLoginDataApiClient.SubmitValidatorAsync(validatorDefinition).Wait(),
                HttpStatusCode.Unauthorized);
        }

        [Test]
        public void LoggedInUserCanSubmitAndDeleteValidator()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            try
            {
                var validatorDefinition = new ValidatorDefinition("MyClass", ValidatorType.TextRules, "Id EXISTS");
                AssertStatusCode(
                    () => dataApiClient.SubmitValidatorAsync(validatorDefinition).Wait(),
                    HttpStatusCode.OK, "Could not submit validator");
                AssertStatusCode(
                    () => dataApiClient.DeleteValidatorAsync(validatorDefinition.Id).Wait(),
                    HttpStatusCode.OK, "Could not delete validator");
            }
            finally
            {
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void NonAdminCannotApproveValidators()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            try
            {
                var validatorDefinition = new ValidatorDefinition("MyClass", ValidatorType.TextRules, "Id EXISTS");
                AssertStatusCode(
                    () => dataApiClient.SubmitValidatorAsync(validatorDefinition).Wait(),
                    HttpStatusCode.OK, "Could not submit validator");
                AssertStatusCode(
                    () => dataApiClient.ApproveValidatorAsync(validatorDefinition.Id).Wait(),
                    HttpStatusCode.Unauthorized, "Non-admin validator approval");
                AssertStatusCode(
                    () => adminDataApiClient.ApproveValidatorAsync(validatorDefinition.Id).Wait(),
                    HttpStatusCode.OK, "Admin validator approval");
                AssertStatusCode(
                    () => dataApiClient.UnapproveValidatorAsync(validatorDefinition.Id).Wait(),
                    HttpStatusCode.Unauthorized, "Non-admin validator unapproval");
                AssertStatusCode(
                    () => adminDataApiClient.UnapproveValidatorAsync(validatorDefinition.Id).Wait(),
                    HttpStatusCode.OK, "Admin validator unapproval");
                AssertStatusCode(
                    () => dataApiClient.DeleteValidatorAsync(validatorDefinition.Id).Wait(),
                    HttpStatusCode.OK, "Could not delete validator");
            }
            finally
            {
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void ValidatorRejectsNonMatchingObject()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            var validatorDefinition = new ValidatorDefinition(nameof(UnitTestDataObject2), ValidatorType.TextRules, "Number IS LESS THAN 150");
            var testObject = new UnitTestDataObject2
            {
                Name = "Hello world!",
                Number = 250
            };
            var objectId = "ValidatorRejectsNonMatchingObject";
            try
            {
                AssertStatusCode(
                    () => dataApiClient.InsertAsync(testObject, objectId).Wait(),
                    HttpStatusCode.OK, "Submit data before validator");
                AssertStatusCode(
                    () => dataApiClient.SubmitValidatorAsync(validatorDefinition).Wait(),
                    HttpStatusCode.OK, "Could not submit validator");
                try
                {
                    AssertStatusCode(
                        () => adminDataApiClient.ApproveValidatorAsync(validatorDefinition.Id).Wait(),
                        HttpStatusCode.OK, "Approve validator");
                    AssertStatusCode(
                        () => dataApiClient.ApplyValidatorAsync(testObject).Wait(),
                        HttpStatusCode.BadRequest, "Apply validator");
                    testObject.Number = 251;
                    AssertStatusCode(
                        () => dataApiClient.ReplaceAsync(testObject, objectId).Wait(),
                        HttpStatusCode.BadRequest, "Submit data after validator");
                    var retreivedObject = dataApiClient.GetAsync<UnitTestDataObject2>(objectId).Result;
                    Assert.That(retreivedObject.Number, Is.EqualTo(250));
                }
                finally
                {
                    dataApiClient.DeleteValidatorAsync(validatorDefinition.Id).Wait();
                }
            }
            finally
            {
                dataApiClient.DeleteAsync<UnitTestDataObject2>(objectId).Wait();
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void ValidatorAcceptsMatchingObject()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            var validatorDefinition = new ValidatorDefinition(nameof(UnitTestDataObject2), ValidatorType.TextRules, "Number IS LESS THAN 150");
            var testObject = new UnitTestDataObject2
            {
                Name = "Hello world!",
                Number = 42
            };
            var objectId = "ValidatorAcceptsMatchingObject";
            try
            {
                AssertStatusCode(
                    () => dataApiClient.SubmitValidatorAsync(validatorDefinition).Wait(),
                    HttpStatusCode.OK, "Could not submit validator");
                AssertStatusCode(
                    () => dataApiClient.ReplaceAsync(testObject, objectId).Wait(),
                    HttpStatusCode.OK, "Could not submit data");
            }
            finally
            {
                dataApiClient.DeleteAsync<UnitTestDataObject2>(objectId).Wait();
                dataApiClient.DeleteValidatorAsync(validatorDefinition.Id).Wait();
                UserGenerator.DeleteUser(dataApiClient);
            }
        }
    }
}
