using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures.PostBodies;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
#pragma warning disable 1591
    public class IdController : ControllerBase
#pragma warning restore 1591
    {
        private readonly IDataRouter dataRouter;
        private readonly AuthorizationModule authorizationModule;
        private readonly NewCollectionTasks newCollectionTasks;

#pragma warning disable 1591
        public IdController(
#pragma warning restore 1591
            IDataRouter dataRouter, 
            AuthorizationModule authorizationModule,
            NewCollectionTasks newCollectionTasks)
        {
            this.dataRouter = dataRouter;
            this.authorizationModule = authorizationModule;
            this.newCollectionTasks = newCollectionTasks;
        }

        /// <summary>
        /// Get and reserve IDs for the specified data type.
        /// Use this if you need to know the definitive ID of an object before submitting it.
        /// Be aware when submitting the actual data that the <see cref="SubmitDataBody.Overwrite"/>-flag in the <see cref="SubmitDataBody"/> must be 'true'.
        /// </summary>
        /// <param name="dataType">Data type for which the ID is requested</param>
        /// <param name="count">Number of IDs to return</param>
        /// <response code="200">The reserved IDs, one ID per line (newline: '\n')</response>
        [HttpGet]
        [ActionName(nameof(GetNew))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetNew(string dataType, int count = 1)
        {
            // Validate
            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SubmitDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            var idReservationResult = await rdDataStorage.GetIdsAsync(dataType, loggedInUsername, count);
            if (idReservationResult.Any(reservationResult => !reservationResult.IsReserved))
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to reserve ID");
            if(idReservationResult.Any(x => x.IsNewCollection))
                newCollectionTasks.PerformTasks(dataType, authorizationResult.User);
            return new ContentResult
            {
                ContentType = "text/plain",
                Content = string.Join("\n",idReservationResult.Select(x => x.Id)),
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Reserve the specified ID. The reservation period is currently unspecified and will often be indefinite.
        /// </summary>
        /// <param name="dataType">Data type for which the ID should be reserved</param>
        /// <param name="id">The ID to be reserved</param>
        /// <response code="403">The ID already exists and hence cannot be reserved (full truth: There might be a server error preventing the ID from being reserved)</response>
        [HttpGet]
        [ActionName(nameof(Reserve))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 403)]
        public async Task<IActionResult> Reserve(string dataType, string id)
        {
            // Validate
            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            if (!rdDataStorage.IsValidId(id))
                return BadRequest($"The ID '{id}' for object of type '{dataType}' is not valid");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new SubmitDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            var idReservationResult = await rdDataStorage.ReserveIdAsync(dataType, id, loggedInUsername);
            if (!idReservationResult.IsReserved)
                return StatusCode((int)HttpStatusCode.Forbidden, "Failed to reserve ID");
            if(idReservationResult.IsNewCollection)
                newCollectionTasks.PerformTasks(dataType, authorizationResult.User);
            return Ok();
        }
    }
}
