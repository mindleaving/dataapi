using Microsoft.AspNetCore.Mvc;

namespace DataProcessingService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ServiceStatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
