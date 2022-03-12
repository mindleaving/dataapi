using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.UserManagement;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.Objects;
using DataAPI.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataAPI.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
#pragma warning disable 1591
    public class AccountController : ControllerBase
#pragma warning restore 1591
    {
        private readonly AuthenticationModule authenticationModule;
        private readonly AuthorizationModule authorizationModule;
        private readonly IEventLogger apiEventLogger;

#pragma warning disable 1591
        public AccountController(
#pragma warning restore 1591
            AuthenticationModule authenticationModule,
            AuthorizationModule authorizationModule,
            IEventLogger apiEventLogger)
        {
            this.authenticationModule = authenticationModule;
            this.authorizationModule = authorizationModule;
            this.apiEventLogger = apiEventLogger;
        }

        /// <summary>
        /// Get all user profiles
        /// </summary>
        /// <response code="200">Returns a list of user profiles</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ActionName(nameof(GetUserProfiles))]
        [ProducesResponseType(typeof(IEnumerable<UserProfile>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetUserProfiles()
        {
            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(new ViewUserProfilesResoruceDescription(), loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            var userProfiles = authenticationModule.GetAllUserProfilesAsync();
            return userProfiles.ToFileStreamResult();
        }

        /// <summary>
        /// Get list of roles for user
        /// </summary>
        /// <param name="username"></param>
        /// <response code="200">Returns list of roles for user</response>
        /// <reponse code="401">Unauthorized</reponse>
        /// <response code="404">User doesn't exist</response>
        [HttpGet]
        [ActionName(nameof(GetGlobalRoles))]
        [ProducesResponseType(typeof(List<Role>), 200)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> GetGlobalRoles([FromQuery] string username)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(username);

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(new GetGlobalRolesResourceDescription(normalizedUsername), loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            if (!await authenticationModule.ExistsAsync(normalizedUsername))
                return NotFound($"User '{normalizedUsername}' doesn't exist");

            var roles = await authenticationModule.GetGlobalRolesForUserAsync(normalizedUsername);
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(roles),
                StatusCode = (int) HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/register
        ///     {
        ///         "Username" : "jdoe",
        ///         "FirstName" : "Jamie",
        ///         "LastName" : "Doe",
        ///         "Email" : "jamie.doe@example.org",
        ///         "Password" : "ANonTerriblePassword"
        ///     }
        /// </remarks>
        /// <response code="200">User successfully created</response>
        /// <response code="400">Invalid request, e.g. password is null or empty</response>
        /// <response code="409">A user with that name already exists</response>
        [HttpPost]
        [AllowAnonymous]
        [ActionName(nameof(Register))]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> Register([FromBody] RegistrationInformation registrationInformation)
        {
            if (string.IsNullOrWhiteSpace(registrationInformation.Username))
                return BadRequest("Username must not be empty");
            if (string.IsNullOrEmpty(registrationInformation.Password))
                return BadRequest("Password must not be empty");
            if (string.IsNullOrWhiteSpace(registrationInformation.Email))
                return BadRequest("Email must not be empty");
            var existingUser = await authenticationModule.FindUserAsync(registrationInformation.Username);
            if(existingUser != null)
                return Conflict($"User '{registrationInformation.Username}' already exists");

            var newUser = UserFactory.Create(registrationInformation);
            if (!await authenticationModule.CreateUserAsync(newUser))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            apiEventLogger.Log(LogLevel.Info, $"New user '{newUser.UserName}' added");
            return Ok();
        }

        /// <summary>
        /// Create access token using Active Directory authentication.
        /// This is typically only meaningful if an access token is needed.
        /// If Active Directory authentication is available
        /// the API methods can be accessed directly without login.
        /// </summary>
        [HttpGet]
        [ActionName(nameof(LoginWithAD))]
        [ProducesResponseType(typeof(AuthenticationResult), 200)]
        [ProducesResponseType(typeof(AuthenticationResult), 401)]
        public async Task<IActionResult> LoginWithAD()
        {
            var loggedInUsername = User.Identity.Name;
            var user = await authenticationModule.FindUserAsync(loggedInUsername);
            var authenticationResult = user != null
                ? authenticationModule.BuildSecurityTokenForUser(user)
                : AuthenticationResult.Failed(AuthenticationErrorType.UserNotFound);

            if(authenticationResult.IsAuthenticated)
                apiEventLogger.Log(LogLevel.Info, $"User '{loggedInUsername}' successfully logged in");
            else
                apiEventLogger.Log(LogLevel.Warning, $"User '{loggedInUsername}' was rejected");

            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(authenticationResult),
                StatusCode = authenticationResult.IsAuthenticated ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Unauthorized
            };
        }

        /// <summary>
        /// Login with username and password. Use this if Active Directory authentication is not available.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ActionName(nameof(Login))]
        [ProducesResponseType(typeof(AuthenticationResult), 200)]
        [ProducesResponseType(typeof(AuthenticationResult), 401)]
        public async Task<IActionResult> Login([FromBody] LoginInformation loginInformation)
        {
            var authenticationResult = await authenticationModule.AuthenticateAsync(loginInformation);
            if(authenticationResult.IsAuthenticated)
                apiEventLogger.Log(LogLevel.Info, $"User '{loginInformation.Username}' successfully logged in");
            else
                apiEventLogger.Log(LogLevel.Warning, $"User '{loginInformation.Username}' was rejected");
            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(authenticationResult),
                StatusCode = authenticationResult.IsAuthenticated ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Unauthorized
            };
        }

        /// <summary>
        /// Returns OK. Since API uses tokens which expire by themselves, nothing happens when calling this method.
        /// Clients should remove the bearer-token from the header and delete their copy of the authentication token.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName(nameof(Logout))]
        [ProducesResponseType(typeof(void), 200)]
        public IActionResult Logout()
        {
            // Nothing to do. Since we use tokens, they expire automatically after 1 hour, if not refreshed.
            return Ok();
        }

        /// <summary>
        /// Change password for user. Admins and user managers can change other users' passwords, while anyone else can only change their own password.
        /// </summary>
        [HttpPost]
        [ActionName(nameof(ChangePassword))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordBody body)
        {
            if (string.IsNullOrEmpty(body.Username))
                return BadRequest("Username not specified");
            if (string.IsNullOrEmpty(body.Password))
                return BadRequest("Password not specified");
            var normalizedUsername = UsernameNormalizer.Normalize(body.Username);
            if (!await authenticationModule.ExistsAsync(normalizedUsername))
                return NotFound($"User '{normalizedUsername}' not found");

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ManageUserResourceDescription(normalizedUsername, UserManagementActionType.ChangePassword),
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Execute
            if (!await authenticationModule.ChangePasswordAsync(normalizedUsername, body.Password))
                return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update password");
            return Ok();
        }

        /// <summary>
        /// Delete user.
        /// Note that status 200 is returned if user doesn't exist.
        /// </summary>
        [HttpDelete]
        [ActionName(nameof(DeleteUser))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> DeleteUser([FromQuery] string username)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(username);

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ManageUserResourceDescription(normalizedUsername, UserManagementActionType.Delete), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }
            var wasDeleted = await authenticationModule.DeleteUserAsync(normalizedUsername);
            if (wasDeleted)
            {
                apiEventLogger.Log(LogLevel.Warning, $"User '{normalizedUsername}' has been deleted");
                return Ok();
            }
            var userExists = await authenticationModule.FindUserAsync(normalizedUsername) != null;
            if(userExists)
                return StatusCode((int)HttpStatusCode.InternalServerError, "User exists but could not be deleted");
            return Ok();
        }

        /// <summary>
        /// Add role to user.
        /// </summary>
        /// <param name="username">User to which role is assigned</param>
        /// <param name="role">Role to add</param>
        /// <param name="dataType">
        /// Optional.
        /// Specify a specific collection/data type for which the role is granted.
        /// If not specified the role is granted globally for non-protected collection
        /// </param>
        [HttpGet]
        [ActionName(nameof(AddRole))]
        public async Task<IActionResult> AddRole([FromQuery]string username, [FromQuery]string role, [FromQuery]string dataType = null)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username not specified");
            if (string.IsNullOrEmpty(role))
                return BadRequest("Role not specified");
            if (!Enum.TryParse<Role>(role, out var roleEnumValue))
                return BadRequest($"Unknown role '{role}'. Valid values are {Enum.GetNames(typeof(Role)).Aggregate((a,b) => $"{a}, {b}")}");
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            if (!await authenticationModule.ExistsAsync(normalizedUsername))
                return NotFound($"User '{normalizedUsername}' not found");

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ManageUserResourceDescription(normalizedUsername, UserManagementActionType.AssignRole), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Execute
            var isGlobalPermission = string.IsNullOrEmpty(dataType);
            if (isGlobalPermission)
            {
                if (!await authorizationModule.AddGlobalRoleToUser(normalizedUsername, roleEnumValue))
                    return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update user roles");
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' added global role '{roleEnumValue}' "
                                                  + $"to  user '{normalizedUsername}'");
            }
            else
            {
                if (!await authorizationModule.AddCollectionRoleToUser(normalizedUsername, roleEnumValue, dataType))
                    return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update user roles");
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' added role '{roleEnumValue}' "
                                                  + $"for collection '{dataType}' to  user '{normalizedUsername}'");
            }
            return Ok();
        }

        /// <summary>
        /// Set multiple roles at once for user.
        /// </summary>
        /// <param name="username">User to which role is assigned</param>
        /// <param name="roles">Roles to add. Roles must be separated by the vertical line-character: |</param>
        /// <param name="dataType">
        /// Optional.
        /// Specify a specific collection/data type for which the role is granted.
        /// If not specified the role is granted globally for non-protected collection
        /// </param>
        [HttpGet]
        [ActionName(nameof(SetRoles))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> SetRoles([FromQuery]string username, [FromQuery]string roles, [FromQuery]string dataType)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username not specified");
            if (string.IsNullOrEmpty(roles))
                return BadRequest("Roles not specified");
            if (!RolesParser.TryParse(roles, out var rolesEnumValues))
                return BadRequest($"Unknown roles '{roles}'. Valid values are {Enum.GetNames(typeof(Role)).Aggregate((a,b) => $"{a}, {b}")} separated by |");
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            if (!await authenticationModule.ExistsAsync(normalizedUsername))
                return NotFound($"User '{normalizedUsername}' not found");

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ManageUserResourceDescription(normalizedUsername, UserManagementActionType.AssignRole), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Execute
            var isGlobalPermission = string.IsNullOrEmpty(dataType);
            if (isGlobalPermission)
            {
                if (!await authorizationModule.SetGlobalRolesForUser(normalizedUsername, rolesEnumValues))
                    return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update user roles");
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' set global roles to '{roles}' "
                                                  + $"for  user '{normalizedUsername}'");
            }
            else
            {
                if (!await authorizationModule.SetCollectionRolesForUser(normalizedUsername, rolesEnumValues, dataType))
                    return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update user roles");
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' set role '{roles}' "
                                                  + $"for collection '{dataType}' to  user '{normalizedUsername}'");
            }
            return Ok();
        }

        /// <summary>
        /// Remove role from user.
        /// </summary>
        /// <param name="username">User from which the role is removed</param>
        /// <param name="role">Role to remove</param>
        /// <param name="dataType">
        /// Optional.
        /// Specify a specific collection/data type from which the role is removed
        /// If not specified the role is removed globally for non-protected collection
        /// </param>
        [HttpGet]
        [ActionName(nameof(RemoveRole))]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> RemoveRole([FromQuery]string username, [FromQuery]string role, [FromQuery]string dataType)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username not specified");
            if (string.IsNullOrEmpty(role))
                return BadRequest("Role not specified");
            if (!Enum.TryParse<Role>(role, out var roleEnumValue))
                return BadRequest($"Unknown role '{role}'. Valid values are {Enum.GetNames(typeof(Role)).Aggregate((a,b) => $"{a}, {b}")}");
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            if (!await authenticationModule.ExistsAsync(normalizedUsername))
                return NotFound($"User '{normalizedUsername}' not found");

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new ManageUserResourceDescription(normalizedUsername, UserManagementActionType.AssignRole), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            // Execute
            var isGlobalPermission = string.IsNullOrEmpty(dataType);
            if (isGlobalPermission)
            {
                if (!await authorizationModule.RemoveGlobalRoleFromUser(normalizedUsername, roleEnumValue))
                    return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update user roles");
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' removed global role '{roleEnumValue}' "
                                                  + $"to  user '{normalizedUsername}'");
            }
            else
            {
                if (!await authorizationModule.RemoveCollectionRoleFromUser(normalizedUsername, roleEnumValue, dataType))
                    return StatusCode((int) HttpStatusCode.InternalServerError, "Could not update user roles");
                apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' removed role '{roleEnumValue}' "
                                                  + $"for collection '{dataType}' to  user '{normalizedUsername}'");
            }
            return Ok();
        }

        /// <summary>
        /// Get the user's permissions on the collection
        /// </summary>
        [HttpGet]
        [ActionName(nameof(GetCollectionPermissions))]
        [ProducesResponseType(typeof(List<CollectionUserPermissions>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<IActionResult> GetCollectionPermissions([FromQuery] string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Invalid collection name");

            // Authroize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var authorizationResult = await authorizationModule.AuthorizeAsync(
                new GetCollectionPermissionsResourceDescription(), 
                loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            var collectionUserPermissions = await authorizationModule.GetAllCollectionPermissionsAsync(collectionName);

            return new ContentResult
            {
                ContentType = Conventions.JsonContentType,
                Content = JsonConvert.SerializeObject(collectionUserPermissions),
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
