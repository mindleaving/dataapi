using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Web.Controllers
{
    [Route("api/[controller]/[action]")]
#pragma warning disable 1591
    public class ServiceStatusController : ControllerBase
#pragma warning restore 1591
    {
        /// <summary>
        /// Returns "pong". Can be used to check availability of DataAPI.
        /// </summary>
        [HttpGet]
        [ActionName(nameof(Ping))]
        [Produces("text/plain")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        /// <summary>
        /// Returns identity of the authenticated user
        /// </summary>
        [HttpGet]
        [Authorize]
        [ActionName(nameof(WhoAmI))]
        [Produces("text/plain")]
        public IActionResult WhoAmI()
        {
            return Ok(User.Identity.Name);
        }
    }
}
