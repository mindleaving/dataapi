using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.PostBodies;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.Objects;
using DataAPI.Service.SubscriptionManagement;
using DataAPI.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
#pragma warning disable 1591
    public class SubscriptionController : ControllerBase
#pragma warning restore 1591
    {
        private readonly AuthenticationModule authenticationModule;
        private readonly AuthorizationModule authorizationModule;
        private readonly SubscriptionManager subscriptionManager;
        private readonly ApiEventLogger apiEventLogger;

#pragma warning disable 1591
        public SubscriptionController(
#pragma warning restore 1591
            AuthenticationModule authenticationModule,
            AuthorizationModule authorizationModule, 
            SubscriptionManager subscriptionManager, 
            ApiEventLogger apiEventLogger)
        {
            this.authenticationModule = authenticationModule;
            this.authorizationModule = authorizationModule;
            this.subscriptionManager = subscriptionManager;
            this.apiEventLogger = apiEventLogger;
        }

        /// <summary>
        /// Subscribe to data changes.
        /// The subscription is for a specific data type and objects can be filtered by providing a filter-query which consists of the WHERE-argument of a SQL-query (without WHERE).
        /// E.g. Filter = Data.Name = 'John' AND Data.Age > 50
        /// The subscription must contain at least one data change event type. The available event types are: Created, Replaced, Deleted.
        /// </summary>
        /// <response code="200">The subscription ID. This is needed for modifying and deleting the subscription.</response>
        [HttpPost]
        [ActionName(nameof(Subscribe))]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> Subscribe([FromBody] SubscriptionBody subscription)
        {
            if (subscription == null)
                return BadRequest("Subscription body was missing or malformatted");
            if (subscription.ModificationTypes.Count == 0)
                return BadRequest($"{nameof(SubscriptionBody.ModificationTypes)} was empty");
            if (subscription.ModificationTypes.Any(x => x == DataModificationType.Unknown))
                return BadRequest($"{nameof(SubscriptionBody.ModificationTypes)} contained value '{DataModificationType.Unknown}'");

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new SubscriptionResourceDescription(subscription.DataType), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }
            var username = authorizationResult.User.UserName;

            if (await subscriptionManager.HasExistingSubscriptionAsync(subscription, username))
                return Conflict("A subscription with exact same parameters already exists");

            // Provide
            var subscriptionId = await subscriptionManager.SubscribeAsync(subscription, username);
            apiEventLogger.Log(LogLevel.Info, $"User '{username}' subscribed to '{subscription.DataType}' with filter '{subscription.Filter}'");
            return new ContentResult
            {
                ContentType = "text/plain",
                Content = subscriptionId,
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Delete subscription.
        /// </summary>
        /// <param name="id">The subscription-ID that was returned by <see cref="Subscribe"/></param>
        [HttpGet]
        [ActionName(nameof(Unsubscribe))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Unsubscribe([FromQuery] string id)
        {
            // Validate
            if (string.IsNullOrEmpty(id))
                return BadRequest("Subscription ID missing");

            var matchingSubscription = await subscriptionManager.GetSubscriptionByIdAsync(id);
            if (matchingSubscription == null)
                return NotFound();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new UnsubscribeResourceDescription(matchingSubscription.Username), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            await subscriptionManager.UnsubscribeAsync(authorizationResult.User.UserName, id);
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' unsubscribed from subscription '{id}'");
            return Ok();
        }

        /// <summary>
        /// Delete all subscription for logged in user for the specified data type
        /// </summary>
        /// <param name="dataType">Data type for which all subscriptions of the logged in user should be deleted</param>
        [HttpGet]
        [ActionName(nameof(UnsubscribeAll))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> UnsubscribeAll([FromQuery] string dataType)
        {
            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new UnsubscribeAllResourceDescription(), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            await subscriptionManager.UnsubscribeAllAsync(authorizationResult.User.UserName, dataType);
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' unsubscribed from '{dataType ?? "Everything"}'");
            return Ok();
        }

        /// <summary>
        /// Get all subscriptions for logged in user. If <paramref name="dataType"/> is not provided, all subscriptions are returned
        /// </summary>
        /// <param name="dataType">Optional: </param>
        /// <response code="200">List of subscriptions: One subscription per line as JSON</response>
        [HttpGet]
        [ActionName(nameof(GetSubscriptions))]
        [ProducesResponseType(typeof(IEnumerable<Subscription>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetSubscriptions([FromQuery] string dataType = null)
        {
            // Validate
            dataType = dataType?.Trim();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ListSubscriptionResourceDescription(),
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            var subscriptions = subscriptionManager.GetSubscriptionsAsync(dataType, authorizationResult.User.UserName);
            return subscriptions.ToFileStreamResult();
        }

        /// <summary>
        /// Get subscriptions notifications, i.e. data-change events, for user.
        /// Remember to discard the notifications using <see cref="DeleteNotification"/>,
        /// it is otherwise returned in future calls to <see cref="GetSubscribedObjects"/>.
        /// </summary>
        /// <param name="dataType">Optional. If specified only notifications for this data type are returned</param>
        /// <response code="200">List of subscription notifications: One notification per line as JSON</response>
        [HttpGet]
        [ActionName(nameof(GetSubscribedObjects))]
        [ProducesResponseType(typeof(IEnumerable<SubscriptionNotification>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetSubscribedObjects([FromQuery]string dataType = null)
        {
            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new SubscriptionResourceDescription(dataType), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            var subscribedObjects = subscriptionManager.GetSubscribedObjectsAsync(authorizationResult.User.UserName, dataType);
            return subscribedObjects.ToFileStreamResult();
        }

        /// <summary>
        /// Delete a subscription notification.
        /// </summary>
        [HttpDelete]
        [ActionName(nameof(DeleteNotification))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> DeleteNotification([FromQuery] string notificationId)
        {
            // Validate
            if (string.IsNullOrEmpty(notificationId))
                return BadRequest("Notification ID not provided");


            var notification = await subscriptionManager.GetNotificationByIdAsync(notificationId);
            if (notification == null)
                return NotFound();

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new DeleteNotificationResourceDescription(notification.Username), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            if (!await subscriptionManager.DeleteNotificationAsync(notificationId))
            {
                return StatusCode(
                    (int) HttpStatusCode.InternalServerError,
                    $"Could not delete notification with ID '{notificationId}' for user '{authorizationResult.User.UserName}'");
            }
            return Ok();
        }

        /// <summary>
        /// Report a data object to another user. Use this to make another user aware of the data.
        ///
        /// _Use-case:_
        /// Data can also be reported to 'dataprocessingservice' which will then run the matching processors.
        /// Usually this isn't required because 'dataprocessingservice' will subscribe to the data anyway,
        /// but if the data has already been processed and needs to be reprocessed, <see cref="ReportTo"/> can be used.
        /// </summary>
        /// <param name="recipient">The username of the recipient</param>
        /// <param name="dataType">The data type which is reported</param>
        /// <param name="id">The ID of the data object which is reported</param>
        [HttpGet]
        [ActionName(nameof(ReportTo))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> ReportTo(
            [FromQuery] string recipient,
            [FromQuery] string dataType,
            [FromQuery] string id)
        {
            // Validate
            if (string.IsNullOrEmpty(recipient))
                return BadRequest("Recipient missing");
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type missing");
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID missing");
            var recipientExists = await authenticationModule.ExistsAsync(recipient);
            if (!recipientExists)
                return BadRequest("Unknown recipient");

            // Authenticate
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ReportDataResourceDescription(), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Provide
            await subscriptionManager.NotifyUserAboutNewDataAsync(recipient, dataType, id);
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' reported '{dataType}' with ID '{id}' to '{recipient}'");
            return Ok();
        }
    }
}
