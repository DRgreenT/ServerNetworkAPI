using Microsoft.AspNetCore.Mvc;
using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using ServerNetworkAPI.dev.Services;

namespace ServerNetworkAPI.dev.WebAPI
{
    [ApiController]
    [Route("api")]
    public class ControllerAPI : ControllerBase
    {
        public static string NetworkControllerName { get;} = @"api/network";
        
        [HttpGet("network")]
        public IActionResult GetDevices()
        {
            var devices = NetworkContext.GetDevices();
            return Ok(devices);
        }

        public static string LogControllerName { get;} = @"api/log";

        [HttpGet("log")]
        public IActionResult GetLogs()
        {
            var logData =  LogData.GetLogData();
            return Ok(logData);
        }

        public static string NotificationControllerName { get; } = @"api/notes";

        [HttpGet("notes")]
        public IActionResult GetNotes()
        {

            var noteData = NotificationData.GetNotificationData();
            return Ok(noteData);
        }

        public static string DataControllerName { get; } = @"api/data";

        [HttpGet("data")]
        public IActionResult GetApiData()
        {
            try
            {
                var noteData = TasksBackgroundService.apiData;
                return Ok(noteData);
            }
            catch(Exception ex)
            {
                LogData.NewLogEvent("DataApiController","[Exeption -> ApiData]", MessageType.Exception,ex.Message);
                return BadRequest(ex.Message);
            }

        }
    }
}
