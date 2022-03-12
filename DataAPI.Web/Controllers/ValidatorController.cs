using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Validation;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.Objects;
using DataAPI.Service.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataAPI.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
#pragma warning disable 1591
    public class ValidatorController : ControllerBase
#pragma warning restore 1591
    {
        private readonly AuthorizationModule authorizationModule;
        private readonly ValidatorManager validatorManager;
        private readonly IEventLogger apiEventLogger;

#pragma warning disable 1591
        public ValidatorController(
#pragma warning restore 1591
            AuthorizationModule authorizationModule,
            ValidatorManager validatorManager,
            IEventLogger apiEventLogger)
        {
            this.authorizationModule = authorizationModule;
            this.validatorManager = validatorManager;
            this.apiEventLogger = apiEventLogger;
        }

        /// <summary>
        /// Validate a validator definition.
        /// Use this to test your syntax before submitting the validator.
        /// The same check will be performed when the validator definition is actually submitted.
        /// </summary>
        /// <response code="403">Validator type must currently be 'TextRules'. Otherwise this status is returned</response>
        [HttpPost]
        [ActionName(nameof(Validate))]
        [ProducesResponseType(typeof(RulesetValidationResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 403)]
        public Task<IActionResult> Validate([FromBody] SubmitValidatorBody body)
        {
            if (body.ValidatorDefinition == null)
                return Task.FromResult<IActionResult>(BadRequest("Validator definition is null"));
            if (!NamingConventions.IsValidDataType(body.ValidatorDefinition.DataType))
                return Task.FromResult<IActionResult>(BadRequest($"Data type '{body.ValidatorDefinition.DataType}' is not a valid name for a collection"));

            if(body.ValidatorDefinition.ValidatorType != ValidatorType.TextRules)
                return Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.Forbidden, "Currently only text rule validators are supported"));
            if(body.ValidatorDefinition.ValidatorType == ValidatorType.Exe)
                return Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.Forbidden, "Exe-validators are currently rejected because of security concerns."));

            var rulesetValidationResult = validatorManager.ValidateValidatorDefinition(body.ValidatorDefinition);
            return Task.FromResult<IActionResult>(new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(rulesetValidationResult),
                StatusCode = (int)HttpStatusCode.OK
            });
        }

        /// <summary>
        /// Submit validator. Administrators can set 'IsApproved' to 'true' to activate the validator immediately. For all other users this flag is ignored.
        /// </summary>
        /// <response code="400">Returns either a string with an error description or a ruleset validation result as JSON</response>
        /// <response code="403">Validator type must currently be 'TextRules'. Otherwise this status is returned</response>
        [HttpPost]
        [ActionName(nameof(Submit))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(RulesetValidationResult), 400)]
        [ProducesResponseType(typeof(string), 403)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> Submit([FromBody] SubmitValidatorBody body)
        {
            // Validate
            if (body.ValidatorDefinition == null)
                return BadRequest("Validator definition is null");
            if (!NamingConventions.IsValidDataType(body.ValidatorDefinition.DataType))
                return BadRequest($"Data type '{body.ValidatorDefinition.DataType}' is not a valid name for a collection");
            if(body.ValidatorDefinition.ValidatorType != ValidatorType.TextRules)
                return StatusCode((int)HttpStatusCode.Forbidden, "Currently only text rule validators are supported");
            if(body.ValidatorDefinition.ValidatorType == ValidatorType.Exe)
                return StatusCode((int)HttpStatusCode.Forbidden, "Exe-validators are currently rejected because of security concerns.");


            var definitionValidationResult = validatorManager.ValidateValidatorDefinition(body.ValidatorDefinition);
            if (!definitionValidationResult.IsValid)
            {
                return new ContentResult
                {
                    ContentType = Conventions.JsonContentType,
                    Content = JsonConvert.SerializeObject(definitionValidationResult),
                    StatusCode = (int) HttpStatusCode.BadRequest
                };
            }

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new AddValidatorResourceDescription(body.ValidatorDefinition.DataType), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Ensure that submitter is equal to the current user
            body.ValidatorDefinition.Submitter = authorizationResult.User.UserName;
            body.ValidatorDefinition.SubmitterEmail = authorizationResult.User.Email;
            if (body.ValidatorDefinition.IsApproved && !authorizationResult.User.Roles.Contains(Role.Admin))
                body.ValidatorDefinition.IsApproved = false;

            try
            {
                validatorManager.AddValidatorDefinition(body.ValidatorDefinition, body.SuppressAutoApprove);
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' "
                                                  + $"added validator for type '{body.ValidatorDefinition.DataType}' "
                                                  + $"with ID '{body.ValidatorDefinition.Id}'");
                return Ok();
            }
            catch(DocumentAlreadyExistsException)
            {
                return Conflict($"Validator with ID '{body.ValidatorDefinition.Id}' for type '{body.ValidatorDefinition.DataType}' already exists");
            }
            catch(Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Apply a validator to a data object. Can be used to test the validity of the data object using the given ruleset.
        /// </summary>
        [HttpPost]
        [ActionName(nameof(Apply))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(RulesetValidationResult), 400)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Apply([FromBody] ApplyValidatorBody body)
        {
            List<IValidator> matchingValidators;
            if (!string.IsNullOrEmpty(body.ValidatorId))
            {
                var validator = validatorManager.GetValidator(body.ValidatorId);
                if (validator == null)
                    return NotFound();
                matchingValidators = new List<IValidator>
                {
                    validator
                };
            }
            else
            {
                matchingValidators = await validatorManager.GetApprovedValidators(body.DataType);
            }

            foreach (var validator in matchingValidators)
            {
                try
                {
                    var validationResult = validator.Validate(body.Data.ToString());
                    if (!validationResult.IsValid)
                    {
                        return new ContentResult
                        {
                            ContentType = Conventions.JsonContentType,
                            Content = JsonConvert.SerializeObject(validationResult),
                            StatusCode = (int) HttpStatusCode.BadRequest
                        };
                    }
                }
                catch (Exception e)
                {
                    apiEventLogger.Log(LogLevel.Error, e.ToString());
                    return StatusCode((int) HttpStatusCode.InternalServerError, e.Message);
                }
            }
            return Ok();
        }

        /// <summary>
        /// Get validator definition.
        /// </summary>
        /// <param name="validatorId">ID of validator</param>
        [HttpGet]
        [ActionName(nameof(Get))]
        [ProducesResponseType(typeof(ValidatorDefinition), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Get([FromQuery] string validatorId)
        {
            if (string.IsNullOrEmpty(validatorId))
                return BadRequest("No validatorId provided");

            var validatorDefinition = validatorManager.GetValidatorDefinition(validatorId);
            if (validatorDefinition == null)
                return NotFound();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(new GetValidatorResourceDescription(validatorDefinition), loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' "
                                              + $"accessed validator for type '{validatorDefinition.DataType}' "
                                              + $"with ID '{validatorDefinition.Id}'");
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(validatorDefinition),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Get all validators.
        /// </summary>
        /// <param name="dataType">Optional. If specified only validators for that data type are returned</param>
        /// <response code="200">List of validator definitions: One validator definition per line as JSON</response>
        [HttpGet]
        [ActionName(nameof(GetAll))]
        [ProducesResponseType(typeof(IEnumerable<ValidatorDefinition>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetAll([FromQuery] string dataType = null)
        {
            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new ManageValidatorsResourceDescription(ValidatorManagementAction.ListAll, null, null);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            var validatorDefinitions = await validatorManager.GetAllValidatorDefinitions(dataType);
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(validatorDefinitions),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Approve a validator. Results in this validator being applied to all future incoming data objects.
        /// </summary>
        /// <param name="validatorId">ID of validator</param>
        [HttpGet]
        [ActionName(nameof(Approve))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Approve([FromQuery] string validatorId)
        {
            if (string.IsNullOrEmpty(validatorId))
                return BadRequest("No validatorId provided");

            var validatorDefinition = validatorManager.GetValidatorDefinition(validatorId);
            if (validatorDefinition == null)
                return NotFound();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new ManageValidatorsResourceDescription(
                ValidatorManagementAction.Approve, 
                validatorDefinition.Submitter,
                validatorDefinition.DataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            if (!validatorManager.ApproveValidator(validatorId))
                return NotFound();
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' approved validator with ID '{validatorId}'");
            return Ok();
        }

        /// <summary>
        /// Unapprove/deactivate validator.
        /// </summary>
        /// <param name="validatorId">ID of validator</param>
        [HttpGet]
        [ActionName(nameof(Unapprove))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Unapprove([FromQuery] string validatorId)
        {
            if (string.IsNullOrEmpty(validatorId))
                return BadRequest("No validatorId provided");

            var validatorDefinition = validatorManager.GetValidatorDefinition(validatorId);
            if (validatorDefinition == null)
                return NotFound();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new ManageValidatorsResourceDescription(
                ValidatorManagementAction.Approve, 
                validatorDefinition.Submitter,
                validatorDefinition.DataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            if (!validatorManager.UnapproveValidator(validatorId))
                return StatusCode((int)HttpStatusCode.NotFound);
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' unapproved validator with ID '{validatorId}'");
            return Ok();
        }

        /// <summary>
        /// Delete validator.
        /// </summary>
        /// <param name="validatorId">ID of validator</param>
        [HttpDelete]
        [ActionName(nameof(Delete))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Delete([FromQuery] string validatorId)
        {
            if (string.IsNullOrEmpty(validatorId))
                return BadRequest("No validatorId provided");

            var validatorDefinition = validatorManager.GetValidatorDefinition(validatorId);
            if (validatorDefinition == null)
                return NotFound();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new ManageValidatorsResourceDescription(
                ValidatorManagementAction.Delete, 
                validatorDefinition.Submitter, 
                validatorDefinition.DataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            if (!validatorManager.DeleteValidator(validatorId))
                return StatusCode((int)HttpStatusCode.NotFound);
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' deleted validator with ID '{validatorId}'");
            return Ok();
        }
    }
}
