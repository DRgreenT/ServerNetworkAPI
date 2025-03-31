using Microsoft.AspNetCore.Mvc;
using NetworkAPI.Services;

namespace NetworkAPI.Controllers
{
    [ApiController]
    [Route("network")]
    public class NetworkController : ControllerBase
    {
        private readonly NetworkScannerService _scanner;

        public NetworkController(NetworkScannerService scanner)
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
