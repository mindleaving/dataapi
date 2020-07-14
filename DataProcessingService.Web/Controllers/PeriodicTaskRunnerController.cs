using DataProcessing;
using Microsoft.AspNetCore.Mvc;

namespace DataProcessingService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PeriodicTaskRunnerController : ControllerBase
    {
        private readonly PeriodicTasksRunner periodicTasksRunner;

        public PeriodicTaskRunnerController(
            PeriodicTasksRunner periodicTasksRunner)
        {
            this.periodicTasksRunner = periodicTasksRunner;
        }

        [HttpGet]
        public IActionResult IsRunning()
        {
            return Ok(periodicTasksRunner.IsRunning.ToString());
        }
    }
}
