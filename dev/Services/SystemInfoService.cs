using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using System.Globalization;

namespace ServerNetworkAPI.dev.Services
{
    public class SystemInfoService
    {
        public string CpuUsage { get; private set; } = "0.0";
        public string MemoryUsage { get; private set; } = "-";
        public string Uptime { get; private set; } = "-";

        private static readonly int ProcessorCores = Environment.ProcessorCount;


        public static bool IsHeadlessServer()
        {
            try
            {
                bool noInput = Console.IsInputRedirected;
                bool noKey = false;

                try
                {
                    //noKey = !Console.KeyAvailable;
                }
                catch (IOException ex)
                {
                    noKey = true;
                    Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    $"Error checking headless mode: {ex.Message}",
                    MessageType.Exception,
                    ""));
                }

                bool noTTY = !File.Exists("/dev/tty");
                bool notInteractive = !Environment.UserInteractive;

                return noInput || noKey || noTTY || notInteractive;
            }
            catch (Exception ex)
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    $"Error checking headless mode: {ex.Message}",
                    MessageType.Exception,
                    ""));
                return true; 
            }
        }


        public static SystemInfoService GetProcessStats()
        {
            return new SystemInfoService
            {
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                Uptime = GetUptime()
            };
        }

        private static string GetMemoryUsage()
        {
            try
            {
                var memLines = File.ReadAllLines("/proc/meminfo");
                int totalMem = ExtractMB(memLines, "MemTotal");
                int availableMem = ExtractMB(memLines, "MemAvailable");

                int usedMem = totalMem - availableMem;

                return $"{usedMem} MB / {totalMem} MB";
            }
            catch
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Error reading /proc/meminfo",
                    MessageType.Exception,
                    ""));
                return "-";
            }
        }

        private static int ExtractMB(string[] lines, string key)
        {
            var line = lines.FirstOrDefault(l => l.StartsWith(key));
            if (line != null)
            {
                var parts = line.Split(':');
                if (parts.Length > 1)
                {
                    string numberPart = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                    if (int.TryParse(numberPart, out int kb))
                    {
                        return kb / 1024; 
                    }
                }
            }
            else
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    $"Key '{key}' not found in /proc/meminfo",
                    MessageType.Exception,
                    ""));
            }
            return 0;
        }

        private static string GetUptime()
        {
            var oldestOutput = BashCmd.ExecuteCmd("ps -eo lstart,pid,comm --sort=start_time --no-headers | head -n 1", "SystemInfoService");
            if (string.IsNullOrEmpty(oldestOutput))
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Error reading process start time",
                    MessageType.Exception,
                    ""));
                return "-";
            }

            string oldestProcess = oldestOutput.Substring(0, 24).Trim();

            if (DateTime.TryParseExact(oldestProcess, "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startTime))
            {
                TimeSpan uptime = DateTime.Now - startTime;
                string days = (uptime.Days > 0) ? $"{uptime.Days}d " : "";
                string hours = (uptime.Hours > 0) ? $"{uptime.Hours.ToString().PadLeft(2,'0')}h " : "00h";
                string minutes = (uptime.Minutes > 0) ? $"{uptime.Minutes.ToString().PadLeft(2, '0')}m " : "00m";
                string seconds = (uptime.Seconds > 0) ? $"{uptime.Seconds.ToString().PadLeft(2, '0')}s" : "00s";
                return days + hours + minutes + seconds;
            }
            else
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Failed to parse process start time",
                    MessageType.Exception,
                    oldestProcess));
                return "-";
            }
        }

        private static string GetCpuUsage()
        {
            var cpuOutput = BashCmd.ExecuteCmd("ps -eo %cpu --no-headers", "SystemInfoService");
            if (string.IsNullOrEmpty(cpuOutput))
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Error reading CPU usage",
                    MessageType.Exception,
                    ""));
                return "0.0";
            }

            double processorUsage = 0.0d;
            foreach (var line in cpuOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                processorUsage += double.TryParse(line.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double cpu) ? cpu : 0.0d;
            }

            processorUsage = processorUsage / ProcessorCores;
            return processorUsage.ToString("0.0", CultureInfo.InvariantCulture);
        }

       
    }
}
