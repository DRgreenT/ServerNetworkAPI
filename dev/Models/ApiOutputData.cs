using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.Network.Scanner;

namespace ServerNetworkAPI.dev.Models
{
    public class ApiOutputData
    {
        public string Version { get; private set; } = string.Empty;
        public int ActiveDeviceCount { get; private set; } = 0;
        public int TotalDeviceCount { get; private set; } = 0;
        public string LastUpdateTime { get; private set; } = string.Empty;
        public string Uptime { get; private set; } = string.Empty;
        public bool IsInternetAvailable { get; private set; } = false;
        public bool IsNmapEnabled { get; private set; } = false;
        public SystemInfoService SystemInfo { get; private set; } = new SystemInfoService();
        public List<Device> Devices { get; private set; } = [];
        public Stack<LogData> Log { get; private set; } = [];
        public Stack<NotificationData> Notifications { get; private set; } = [];
        public int[] ActiveDevicesCounts { get; private set; } = [];

        public static ApiOutputData GetData()
        {
            return new ApiOutputData
            {
                Version = AppConfig.Version,
                ActiveDeviceCount = NetworkContext.GetActiveDevices().Count(),
                TotalDeviceCount = NetworkContext.GetDevices().Count(),
                LastUpdateTime = ArpScanner.LastScanTime.ToString("HH:mm:ss"),
                Uptime = FormatUptime(TasksBackgroundService.ServiceStartTime),
                IsInternetAvailable = TasksBackgroundService.IsInternetAvailable,
                Devices = NetworkContext.GetDevices().ToList(),
                IsNmapEnabled = AppConfig.IsNmapEnabled,
                Log = LogData.GetLogData(),

                Notifications = NotificationData.GetNotificationData(),
                SystemInfo = SystemInfoService.GetProcessStats(),

                ActiveDevicesCounts = GetLastScanCounts(),

            };
        }

        private static string FormatUptime(DateTime serviceStart)
        {
            TimeSpan uptime = DateTime.Now - serviceStart;
            return $"{(int)uptime.TotalDays:D2}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }

        private static int[] GetLastScanCounts()
        {
            List<int> ScanCounts = new List<int>();
            ScanCounts.AddRange(TasksBackgroundService.activeDevicesCounts);

            int arraySize = ScanCounts.Count < 1000 ? ScanCounts.Count : 1000;
            int[] array = new int[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                array[i] = ScanCounts[ScanCounts.Count - arraySize + i];
            }

            return array;
        }
    }
}
