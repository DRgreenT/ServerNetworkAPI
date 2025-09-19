using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ServerNetworkAPI.dev.Services
{
    public class SystemInfoService
    {
        public string CpuUsage { get; private set; } = "0.0";
        public string MemoryUsage { get; private set; } = "-";
        public string Uptime { get; private set; } = "-";

        private static readonly int ProcessorCores = Environment.ProcessorCount;

        public static bool IsHeadlessModeFromArgs { get; set; } = false;

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

        private static bool noteWasTriggered = false;

        private static string GetUptime()
        {
            // Nutzt "uptime -s" für den exakten System-Startzeitpunkt
            var startTimeOutput = BashCmd.ExecuteCmd("uptime -s", "SystemInfoService");
            if (string.IsNullOrWhiteSpace(startTimeOutput))
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Error reading system uptime",
                    MessageType.Exception
                ));
                return "-";
            }

            if (!DateTime.TryParse(startTimeOutput.Trim(), out DateTime startTime))
            {
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Failed to parse system uptime",
                    MessageType.Exception,
                    startTimeOutput.Trim()
                ));
                return "-";
            }

            TimeSpan uptime = DateTime.Now - startTime;

            if (uptime.TotalMinutes < 5 && !noteWasTriggered)
            {
                NotificationService.SendMessage("Server was recently restarted.", true);
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    "Recent server restart detected.",
                    MessageType.Warning
                ));
                noteWasTriggered = true;
            }

            string days = uptime.Days > 0 ? $"{uptime.Days}d " : "";
            string hours = $"{uptime.Hours:D2}h ";
            string minutes = $"{uptime.Minutes:D2}m ";
            string seconds = $"{uptime.Seconds:D2}s";

            return days + hours + minutes + seconds;
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
