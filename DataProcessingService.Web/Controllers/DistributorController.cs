using DataProcessing;
using Microsoft.AspNetCore.Mvc;

namespace DataProcessingService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DistributorController : ControllerBase
    {
        private readonly Distributor distributor;

        public DistributorController(
            Distributor distributor)
        {
            this.distributor = distributor;
        }

        [HttpGet]
        public IActionResult IsRunning()
        {
            return Ok(distributor.IsRunning.ToString());
        }
    }
}
