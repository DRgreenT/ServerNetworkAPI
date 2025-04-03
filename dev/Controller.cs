using Microsoft.AspNetCore.Mvc;

namespace ServerNetworkAPI.dev
{
    [ApiController]
    [Route("network")]
    public class Controller : ControllerBase
    {
        private readonly NetworkScan _scanner;

        public Controller(NetworkScan scanner)
        {
            _scanner = scanner;
        }

        [HttpGet]
        public IActionResult Get()
        {           
            return Ok(_scanner.GetDevices());
        }
    }
}
