using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.Views;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.DataRouting;
using DataAPI.Service.Objects;
using DataAPI.Service.Search;
using DataAPI.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataAPI.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
#pragma warning disable 1591
    public class ViewController : ControllerBase
#pragma warning restore 1591
    {
        private readonly ViewManager viewManager;
        private readonly IDataRouter dataRouter;
        private readonly AuthorizationModule authorizationModule;
        private readonly ApiEventLogger apiEventLogger;
        private readonly IHttpContextAccessor httpContextAccessor;

#pragma warning disable 1591
        public ViewController(
#pragma warning restore 1591
            ViewManager viewManager, 
            IDataRouter dataRouter,
            AuthorizationModule authorizationModule,
            ApiEventLogger apiEventLogger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.viewManager = viewManager;
            this.dataRouter = dataRouter;
            this.authorizationModule = authorizationModule;
            this.apiEventLogger = apiEventLogger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Create view
        /// </summary>
        [HttpPost]
        [ActionName(nameof(Create))]
        [ProducesResponseType(typeof(ViewInformation), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> Create([FromBody] CreateViewBody body)
        {
            if(FromArgumentIsNullOrContainsPlaceholder(body.Query))
                return BadRequest("Data type (FROM-argument) must be specified in query and cannot be a placeholder");
            var dataType = DetermineViewCollection(body.Query);

            // Authroize
            var loggedInUsername = ControllerHelpers.GetUsername(httpContextAccessor);
            var resourceDescription = new CreateViewResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }
			
			try
			{
			    var viewInformation = await viewManager.CreateViewAsync(body, authorizationResult.User.UserName);
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' added view with ID '{viewInformation.ViewId}'");
                return Ok(viewInformation);
            }
			catch(DocumentAlreadyExistsException)
			{
				return Conflict($"View with name '{body.ViewId}' already exists");
			}
			catch(Exception e)
			{
			    return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
			}
        }

        /// <summary>
        /// Get view. Placeholders of view are replaced using the values supplied by the GET-query.
        /// E.g. if view containts placeholder {id} the query should contain &amp;id=2312412
        /// </summary>
        [HttpGet]
        [ActionName(nameof(Get))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> Get([FromQuery] string viewId, [FromQuery] string resultFormat)
        {
            // Validate
            if (string.IsNullOrEmpty(viewId))
                return BadRequest("View-ID not specified");

            var resultFormatEnum = ResultFormat.Json;
            if(resultFormat != null && !Enum.TryParse(resultFormat, out resultFormatEnum))
            {
                var validResultFormats = Enum.GetNames(typeof(ResultFormat)).Aggregate((a,b) => a + ", " + b);
                return BadRequest($"Invalid output format '{resultFormat}'. Allowed values: {validResultFormats}");
            }

            var view = await viewManager.GetView(viewId);
            if (view == null)
                return NotFound();

            string parameterInsertedQuery;
            try
            {
                parameterInsertedQuery = QueryParameterInserter.InsertParameters(view.Query, QueryCollectionToDictionary(Request.Query));
            }
            catch (FormatException formatException)
            {
                return BadRequest(formatException.Message);
            }

            // Authroize
            var loggedInUsername = ControllerHelpers.GetUsername(httpContextAccessor);
            var dataType = DetermineViewCollection(parameterInsertedQuery);
            var resourceDescription = new GetViewResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int) HttpStatusCode.Unauthorized, "Not authorized");
            }
            return await SearchExecutor.PerformSearch(dataRouter, parameterInsertedQuery, resultFormatEnum);
        }

        private Dictionary<string, List<string>> QueryCollectionToDictionary(IQueryCollection queryCollection)
        {
            return queryCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
        }

        /// <summary>
        /// Delete view
        /// </summary>
        [HttpDelete]
        [ActionName(nameof(Delete))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> Delete([FromQuery] string viewId)
        {
            // Validate
            if (string.IsNullOrEmpty(viewId))
                return BadRequest("View-ID not specified");

            var view = await viewManager.GetView(viewId);
            if (view == null)
                return NotFound();

            // Authroize
            var loggedInUsername = ControllerHelpers.GetUsername(httpContextAccessor);
            var dataType = DetermineViewCollection(view.Query);
            var resourceDescription = new DeleteViewResourceDescription(view.Submitter, dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int) HttpStatusCode.Unauthorized, "Not authorized");
            }

            await viewManager.DeleteViewAsync(viewId);
            return Ok();
        }

        private static bool FromArgumentIsNullOrContainsPlaceholder(string query)
        {
            var placeholderReplacement = "#PLACEHOLDER#";
            var placeholderRemovedQuery = Regex.Replace(query, "{[^}]+}", placeholderReplacement);
            var parsedQuery = DataApiSqlQueryParser.Parse(placeholderRemovedQuery);
            if (parsedQuery.FromArguments == null)
                return true;
            return parsedQuery.FromArguments.Contains(placeholderReplacement);
        }

        private static string DetermineViewCollection(string query)
        {
            var placeholderReplacement = "#PLACEHOLDER#";
            var placeholderRemovedQuery = Regex.Replace(query, "{[^}]+}", placeholderReplacement);
            var parsedQuery = DataApiSqlQueryParser.Parse(placeholderRemovedQuery);
            return parsedQuery.FromArguments;
        }
    }
}
