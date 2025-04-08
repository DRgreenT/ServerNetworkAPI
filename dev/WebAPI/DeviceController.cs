using Microsoft.AspNetCore.Mvc;
using ServerNetworkAPI.dev.Core;

namespace ServerNetworkAPI.dev.WebAPI
{
    [ApiController]
    [Route("network")]
    public class DeviceController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDevices()
        {
            var devices = NetworkContext.GetDevices();
            return Ok(devices);
        }
    }
}
