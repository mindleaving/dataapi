using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.UserManagement;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using DataAPI.Service.Search;
using DataAPI.Service.SubscriptionManagement;
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
    public class DataIoController : ControllerBase
#pragma warning restore 1591
    {
        private readonly IDataRouter dataRouter;
        private readonly AuthorizationModule authorizationModule;
        private readonly CollectionInformationManager collectionInformationManager;
        private readonly ValidatorManager validatorManager;
        private readonly SubscriptionManager subscriptionManager;
        private readonly ApiEventLogger apiEventLogger;
        private readonly IIdPolicy idPolicy;
        private readonly NewCollectionTasks newCollectionTasks;

#pragma warning disable 1591
        public DataIoController(
#pragma warning restore 1591
            AuthorizationModule authorizationModule,
            CollectionInformationManager collectionInformationManager,
            ValidatorManager validatorManager,
            SubscriptionManager subscriptionManager, 
            ApiEventLogger apiEventLogger,
            IDataRouter dataRouter,
            IIdPolicy idPolicy,
            NewCollectionTasks newCollectionTasks)
        {
            this.authorizationModule = authorizationModule;
            this.validatorManager = validatorManager;
            this.collectionInformationManager = collectionInformationManager;
            this.subscriptionManager = subscriptionManager;
            this.apiEventLogger = apiEventLogger;
            this.dataRouter = dataRouter;
            this.idPolicy = idPolicy;
            this.newCollectionTasks = newCollectionTasks;
        }

        /// <summary>
        /// For submission of data containing binary content, e.g. DataBlob and Image.
        /// Creates a submission, i.e. a data object without its binary payload and returns the submission ID
        /// which needs to be provided when uploading binary data using <see cref="TransferSubmissionData"/>
        /// </summary>
        /// <response code="200">Submission ID</response>
        /// <response code="409">
        /// This status code is returned if an ID was specified (either in the request body or on the object itself),
        /// and an object with that ID already exists
        /// and the <see cref="SubmitDataBody.Overwrite"/>-flag is set to 'false'
        /// </response>
        [HttpPost]
        [ActionName(nameof(CreateSubmission))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> CreateSubmission([FromBody] SubmitDataBody submitBody)
        {
            return await Submit(submitBody);
        }

        /// <summary>
        /// Transfer binary data for the specified submission (data type-submission ID-pair).
        /// A submission might be created using <see cref="CreateSubmission"/>.
        /// </summary>
        /// <param name="dataType">Data type of submission</param>
        /// <param name="submissionId">ID of submission</param>
        [HttpPost]
        [ActionName(nameof(TransferSubmissionData))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> TransferSubmissionData([FromQuery] string dataType, [FromQuery] string submissionId)
        {
            if (submissionId == null)
                return BadRequest("Invalid submissionId");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SubmitDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest($"No data storage backend for data type '{dataType}'");
            }
            if (!(rdDataStorage is IBinaryRdDataStorage binaryRdDataStorage))
                return BadRequest($"Transfer of data for data type '{dataType}' is not supported");

            if (!await binaryRdDataStorage.ExistsAsync(dataType, submissionId))
                return NotFound();

            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' has injected binary payload into '{dataType}' with ID '{submissionId}'");
            await binaryRdDataStorage.InjectDataAsync(dataType, submissionId, Request.Body);
            return Ok();
        }

        /// <summary>
        /// Submit data to database. Data is validated using available validators.
        /// </summary>
        /// <response code="200">ID of stored object</response>
        /// <response code="409">
        /// This status code is returned if an ID was specified (either in the request body or on the object itself),
        /// and an object with that ID already exists
        /// and the <see cref="SubmitDataBody.Overwrite"/>-flag is set to 'false'
        /// </response>
        [HttpPost]
        [ActionName(nameof(Submit))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> Submit([FromBody] SubmitDataBody body)
        {
            if (!NamingConventions.IsValidDataType(body.DataType))
                return BadRequest($"Data type '{body.DataType}' is not a valid name for a collection");
            if (body.Data == null)
                return BadRequest("No data submitted");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SubmitDataResourceDescription(body.DataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(body.DataType);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest($"No data storage backend for data type '{body.DataType}'");
            }

            // Validate
            var suggestedId = await idPolicy.DetermineIdAsync(body, loggedInUsername, rdDataStorage);
            if (!rdDataStorage.IsValidId(suggestedId.Id))
                return BadRequest($"The ID '{suggestedId.Id}' for object of type '{body.DataType}' is not valid");
            if (body.Overwrite)
            {
                if (string.IsNullOrEmpty(suggestedId.Id))
                    return BadRequest("Overwriting of existing data is requested, but no ID is provided.");
            }
            var validators = await validatorManager.GetApprovedValidators(body.DataType);
            foreach (var validator in validators)
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

            var existingData = await GetExistingMetadataAsync(rdDataStorage, body.DataType, suggestedId.Id);
            if (body.Overwrite)
            {
                if (!await IsAllowedToOverwriteDataAsync(body.DataType, existingData, authorizationResult.User))
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, "Data from other users cannot be overwritten, unless you're an admin");
                }
            }
            else if(existingData != null && !suggestedId.HasBeenReserved)
                return Conflict($"Document of type '{body.DataType}' with ID '{suggestedId.Id}' already exists");

            // Provide
            var apiVersion = ApiVersion.Current;
            var data = DataEncoder.Encode(JsonConvert.SerializeObject(body.Data));
            var utcNow = DateTime.UtcNow;
            var dataContainer = new GenericDataContainer(
                suggestedId.Id,
                existingData?.OriginalSubmitter ?? authorizationResult.User.UserName,
                existingData?.CreatedTimeUtc ?? utcNow,
                authorizationResult.User.UserName,
                utcNow,
                apiVersion,
                data);
            try
            {
                var storeResult = await rdDataStorage.StoreAsync(body.DataType, dataContainer, body.Overwrite || suggestedId.HasBeenReserved);
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' has submitted data of type '{body.DataType}' with ID '{suggestedId.Id}'");
                await subscriptionManager.NotifyDataChangedAsync(body.DataType, suggestedId.Id, storeResult.ModificationType);
                if(storeResult.IsNewCollection)
                    newCollectionTasks.PerformTasks(body.DataType, authorizationResult.User);
                return new ContentResult
                {
                    ContentType = "text/plain",
                    Content = storeResult.Id,
                    StatusCode = (int) HttpStatusCode.OK
                };
            }
            catch (DocumentAlreadyExistsException)
            {
                return Conflict($"Document of type '{body.DataType}' with ID '{suggestedId.Id}' already exists");
            }
            catch(Exception e)
            {
                apiEventLogger.Log(LogLevel.Error, e.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

        private Task<GenericDataContainer> GetExistingMetadataAsync(IRdDataStorage rdDataStorage, string dataType, string id)
        {
            return rdDataStorage is IBinaryRdDataStorage binaryRdDataStorage
                ? binaryRdDataStorage.GetMetadataFromId(dataType, id)
                : rdDataStorage.GetFromIdAsync(dataType, id);
        }

        private async Task<bool> IsAllowedToOverwriteDataAsync(string dataType, GenericDataContainer existingData, User user)
        {
            if (existingData == null)
                return true;
            if (existingData.Submitter == user.UserName)
                return true;
            var overwritingAllowed = await authorizationModule.IsOverwritingAllowedForCollectionAsync(dataType);
            if (overwritingAllowed)
                return true;
            if (user.Roles.Contains(Role.Admin))
                return true;
            return false;
        }

        /// <summary>
        /// Check existance of data object. Returns status 200 (OK) if object exists, 404 (NotFound) otherwise.
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID</param>
        [HttpGet]
        [ActionName(nameof(Exists))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Exists([FromQuery]string dataType, [FromQuery]string id)
        {
            // Validate
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type not specified");
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID not specified");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            // Provide
            var exists = await rdDataStorage.ExistsAsync(dataType, id);
            if (exists)
                return Ok();
            else
                return NotFound();
        }

        /// <summary>
        /// Returns JSON-representation of requested data object.
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID</param>
        [HttpGet]
        [ActionName(nameof(Get))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Get([FromQuery]string dataType, [FromQuery]string id)
        {
            // Validate
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type not specified");
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID not specified");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            // Provide
            if (!await rdDataStorage.ExistsAsync(dataType, id))
                return NotFound();
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' has accessed data of type '{dataType}' with ID '{id}'");
            var matchingContainer = await rdDataStorage.GetFromIdAsync(dataType, id);
            var json = DataEncoder.DecodeToJson(matchingContainer.Data);
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = json,
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Like <see cref="Get"/> but if the requested data type contains binary data, the binary payload is not returned.
        /// Returns JSON-representation of data object
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID</param>
        [HttpGet]
        [ActionName(nameof(GetSubmissionMetadata))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetSubmissionMetadata([FromQuery] string dataType, [FromQuery] string id)
        {
            // Validate
            if (dataType == null)
                return BadRequest("Invalid dataType");
            if (id == null)
                return BadRequest("Invalid id");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest($"No data storage backend for data type '{dataType}'");
            }

            if (!(rdDataStorage is IBinaryRdDataStorage binaryRdDataStorage))
                return await Get(dataType, id);

            if (!await binaryRdDataStorage.ExistsAsync(dataType, id))
                return NotFound();

            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' has accessed metadata of type '{dataType}' with ID '{id}'");
            var container = await binaryRdDataStorage.GetMetadataFromId(dataType, id);
            var json = DataEncoder.DecodeToJson(container.Data);
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = json,
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Like <see cref="Get"/> but if the requested data type contains binary data, only the binary data is returned.
        /// Otherwise returns JSON-representation of data object.
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID</param>
        [HttpGet]
        [ActionName(nameof(GetSubmissionData))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetSubmissionData([FromQuery] string dataType, [FromQuery] string id)
        {
            // Validate
            if (dataType == null)
                return BadRequest("Invalid dataType");
            if (id == null)
                return BadRequest("Invalid id");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest($"No data storage backend for data type '{dataType}'");
            }

            if (!(rdDataStorage is IBinaryRdDataStorage binaryRdDataStorage))
                return await Get(dataType, id);

            if (!await binaryRdDataStorage.ExistsAsync(dataType, id))
                return NotFound();

            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' has accessed binary payload of type '{dataType}' with ID '{id}'");
            var stream = await binaryRdDataStorage.GetBinaryDataFromIdAsync(dataType, id);
            return new FileStreamResult(stream, Conventions.JsonContentType);
        }

        /// <summary>
        /// Get objects of the specified type matching the filter (or all if no filter is provided).
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="whereArguments">Optional. Filter in the shape of a WHERE-argument (without WHERE). Example: Data.Name = 'John' AND Data.Age > 30</param>
        /// <param name="orderByArguments">Optional. Sorting arguments in the shape of an ORDER BY-argument (without ORDER BY). Example: Data.CreatedTime DESC, Data.Number ASC</param>
        /// <param name="limit">Optional. If not specified, all matching objects are returned</param>
        /// <response code="200">List of matching data objects. Stream contains JSON-representation of a single object per line (newline: '\n')</response>
        [HttpGet]
        [ActionName(nameof(GetMany))]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetMany([FromQuery]string dataType, [FromQuery] string whereArguments, [FromQuery] string orderByArguments, [FromQuery]uint? limit = null)
        {
            // Validate
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type not specified");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' requested objects of type '{dataType}' matching '{whereArguments?.RemoveLineBreaks()}' ordered by '{orderByArguments?.RemoveLineBreaks()}'");
            try
            {
                var getManyResult = rdDataStorage.GetManyAsync(dataType, whereArguments, orderByArguments, limit);
                var stream = new SearchResultStream(getManyResult.Select(x => DataEncoder.DecodeToJson(x.Data)));
                return new FileStreamResult(stream, Conventions.JsonContentType);
            }
            catch (FormatException formatException)
            {
                return BadRequest(formatException.Message);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.InnermostException().Message);
            }
        }

        /// <summary>
        /// Delete a data object.
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID</param>
        [HttpDelete]
        [ActionName(nameof(Delete))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Delete([FromQuery]string dataType, [FromQuery]string id)
        {
            // Validate
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type not specified");
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID not specified");

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            // Check existance
            var metadata = await GetExistingMetadataAsync(rdDataStorage, dataType, id);
            if (metadata == null) // Object doesn't exist
                return Ok(); // SECURITY NOTE: Returning OK without authentication allows for brute-force scanning for valid IDs!

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var overwritingAllowed = await authorizationModule.IsOverwritingAllowedForCollectionAsync(dataType);
            var resourceDescription = new DeleteDataResourceDescription(dataType, metadata.Submitter, overwritingAllowed);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Delete
            if (await rdDataStorage.DeleteDataContainerAsync(dataType, id))
            {
                await subscriptionManager.NotifyDataChangedAsync(dataType, id, DataModificationType.Deleted);
                apiEventLogger.Log(LogLevel.Warning, $"User '{authorizationResult.User.UserName}' has deleted data of type '{dataType}' with ID '{id}'");
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Delete multiple data objects matching a filter
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="whereArguments">Optional. Filter in the shape of a WHERE-argument (without WHERE). Example: Data.Name = 'John' AND Data.Age > 30</param>
        /// <response code="200">Returns list of deletion results</response>
        [HttpDelete]
        [ActionName(nameof(DeleteMany))]
        [ProducesResponseType(typeof(IEnumerable<DeleteResult>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> DeleteMany([FromQuery]string dataType, [FromQuery]string whereArguments)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dataType))
                return BadRequest("Data type not specified");

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var overwritingAllowed = await authorizationModule.IsOverwritingAllowedForCollectionAsync(dataType);

            // Check existance
            var query = $"SELECT _id, Submitter FROM {dataType} WHERE {whereArguments}";
            var searchResult = rdDataStorage.SearchAsync(DataApiSqlQueryParser.Parse(query));
            var deleteResults = new List<DeleteResult>();
            await foreach (var doc in searchResult)
            {
                var submitter = doc["Submitter"].AsString;
                var id = doc["_id"].AsString;

                // Authorize
                var resourceDescription = new DeleteDataResourceDescription(dataType, submitter, overwritingAllowed);
                var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
                if (!authorizationResult.IsAuthorized)
                {
                    deleteResults.Add(DeleteResult.Failed(dataType, id, "Not authorized"));
                    continue;
                }

                // Delete
                if (await rdDataStorage.DeleteDataContainerAsync(dataType, id))
                {
                    await subscriptionManager.NotifyDataChangedAsync(dataType, id, DataModificationType.Deleted);
                    deleteResults.Add(DeleteResult.Success(dataType, id));
                }
                else
                {
                    deleteResults.Add(DeleteResult.Failed(dataType, id, "Not found"));
                }
            }

            apiEventLogger.Log(LogLevel.Warning, $"User '{loggedInUsername}' has deleted data of type '{dataType}' with matching '{whereArguments}'");

            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(deleteResults),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Search database using a SQL-query.
        /// </summary>
        /// <response code="200">List of results. Stream contains JSON-representation of search results, one result per line</response>
        [HttpPost]
        [ActionName(nameof(Search))]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> Search([FromBody] SearchBody body)
        {
            if (body == null)
                return BadRequest("No search body submitted");
            var query = body.Query;
            DataApiSqlQuery parsedQuery;
            try
            {
                parsedQuery = DataApiSqlQueryParser.Parse(query);
            }
            catch (FormatException formatException)
            {
                return BadRequest(formatException.Message);
            }

            // Validate
            var dataType = parsedQuery.FromArguments;

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SearchResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' submitted query '{query.RemoveLineBreaks()}'");
            return await SearchExecutor.PerformSearch(dataRouter, query, body.Format);
        }

        /// <summary>
        /// Get collection informations
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        [HttpGet]
        [ActionName(nameof(GetCollectionInformation))]
        [ProducesResponseType(typeof(CollectionInformation), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetCollectionInformation([FromQuery] string collectionName)
        {
            // Validate
            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name not specified");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetCollectionInformationResourceDescription(collectionName);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            var collectionInformation = await collectionInformationManager.GetCollectionInformationAsync(collectionName, authorizationResult.User);
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(collectionInformation),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// List names of collections
        /// </summary>
        /// <param name="includeHidden">Include hidden collections</param>
        [HttpGet]
        [ActionName(nameof(ListCollectionNames))]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> ListCollectionNames([FromQuery]bool includeHidden = false)
        {
            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new ListCollectionsResourceDescription();
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            var collectionNames = dataRouter.ListCollectionNamesAsync();
            var permissionFilteredCollectionNames = new List<string>();
            await foreach (var collectionName in collectionNames)
            {
                var collectionInformation = await collectionInformationManager.GetCollectionInformationAsync(collectionName, authorizationResult.User);
                if (!includeHidden && collectionInformation.IsHidden)
                    continue;
                permissionFilteredCollectionNames.Add(collectionName);
            }

            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(permissionFilteredCollectionNames),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// List collections including their properties
        /// </summary>
        /// <param name="includeHidden">Include hidden collections</param>
        [HttpGet]
        [ActionName(nameof(ListCollections))]
        [ProducesResponseType(typeof(IEnumerable<CollectionInformation>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> ListCollections([FromQuery]bool includeHidden = false)
        {
            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new ListCollectionsResourceDescription();
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            var collectionNames = dataRouter.ListCollectionNamesAsync();
            var collectionInformations = new List<CollectionInformation>();
            await foreach (var collectionName in collectionNames)
            {
                var collectionInformation = await collectionInformationManager.GetCollectionInformationAsync(collectionName, authorizationResult.User);
                if (!includeHidden && collectionInformation.IsHidden)
                    continue;
                collectionInformations.Add(collectionInformation);
            }

            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(collectionInformations),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Redirect data to another data storage.
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="dataSourceSystem">Data storage ID to which the data is redirected. Current valid list of storage IDs: MongoDB, FileSystem, AzureBlobStorage, ExistingSQL, GenericSQL.</param>
        /// <response code="503">Returned if the backing data storage is not currently available</response>
        [HttpGet]
        [ActionName(nameof(SetRedirection))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 503)]
        public async Task<IActionResult> SetRedirection(
            [FromQuery] string dataType, 
            [FromQuery] string dataSourceSystem)
        {
            // Validate
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type not specified");
            if (!NamingConventions.IsValidDataType(dataType))
                return BadRequest($"Data type '{dataType}' is not a valid name for a collection");
            if (!dataRouter.IsAvailable(dataSourceSystem))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, $"Data source system '{dataSourceSystem}' is currently unavailable");
            }
            if (!dataRouter.IsDataTypeSupported(dataType, dataSourceSystem))
            {
                return BadRequest($"Data source system '{dataSourceSystem}' doesn't support data type '{dataType}'");
            }

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SetDataRedirectionResourceDescription();
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' redirected '{dataType}' to '{dataSourceSystem}'");
            await dataRouter.SetRedirectionAsync(new DataRedirection(dataType, dataSourceSystem));

            return Ok();
        }

        /// <summary>
        /// Set collection options.
        /// </summary>
        /// <param name="collectionOptions">Collection options</param>
        [HttpPost]
        [ActionName(nameof(SetCollectionOptions))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> SetCollectionOptions([FromBody] CollectionOptions collectionOptions)
        {
            if (collectionOptions == null)
                return BadRequest("Collection options not set");
            if (!NamingConventions.IsValidDataType(collectionOptions.CollectionName))
                return BadRequest($"Data type '{collectionOptions.CollectionName}' is not a valid name for a collection");

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(collectionOptions.CollectionName);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            if(collectionOptions.IdGeneratorType.HasValue && !rdDataStorage.IsIdGeneratorTypeSupported(collectionOptions.IdGeneratorType.Value))
                return BadRequest($"The backing storage for collection '{collectionOptions.CollectionName}' doesn't support ID generator type '{collectionOptions.IdGeneratorType}'");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SetCollectionOptionsResourceDescription();
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            try
            {
                await authorizationModule.AddOrUpdateCollectionMetadata(collectionOptions);
                var logEntries = LoggingHelpers.WriteCollectionMetadataLog(collectionOptions, authorizationResult.User.UserName);
                logEntries.ForEach(apiEventLogger.Log);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError, e.InnermostException().Message);
            }
        }
    }
}