using Microsoft.AspNetCore.Mvc;
using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;

namespace ServerNetworkAPI.dev.WebAPI
{
    [ApiController]
    [Route("api")]
    public class ControllerAPI : ControllerBase
    {
        public static string NetworkControllerName { get; private set; } = @"api/network";
        
        [HttpGet("network")]
        public IActionResult GetDevices()
        {
            var devices = NetworkContext.GetDevices();
            return Ok(devices);
        }

        public static string LogControllerName { get; private set; } = @"api/log";

        [HttpGet("log")]
        public IActionResult GetLogs()
        {
            var logData =  LogData.GetLogData();
            return Ok(logData);
        }
    }
}
