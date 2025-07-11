﻿using ServerNetworkAPI.dev.IO;
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

        public static bool IsHeadlessFromArgs { get; set; } = false;

        public static bool IsConsoleInactive { get; private set; } = false;

        public static void SetConsoleState()
        {
            IsConsoleInactive = IsHeadlessServer(); 
        }
        public static bool IsHeadlessServer(bool debugMode = true)
        {
            try
            {
                bool inputRedirected = Console.IsInputRedirected;
                bool githubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
                bool keyAvailableFails = false;
                bool noUserInteraction = !Environment.UserInteractive;
                bool isHeadless = false;

                try
                {
                    var _ = Console.KeyAvailable;
                }
                catch (Exception)
                {
                    keyAvailableFails = true;
                }

                isHeadless = inputRedirected || githubActions || keyAvailableFails || noUserInteraction || IsHeadlessFromArgs;
               
                string message = isHeadless ? "On" : "Off";

                string debugMessage = debugMode ? $" -> InputRedirected:{ inputRedirected}, GitHubActions: { githubActions}, KeyAvailableFails: { keyAvailableFails}, NoUserInteraction: { noUserInteraction}, FromArgs: { IsHeadlessFromArgs}" : "";
                
                Logger.Log(LogData.NewLogEvent(
                    "SystemInfoService",
                    $"Headless mode {message}{debugMessage}",
                    MessageType.Standard,
                    ""));

                return isHeadless;
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

            if(oldestProcess.Contains("  "))
            {
                oldestProcess = Regex.Replace(oldestProcess, @"\s{2,}", " ");
            }

            if (DateTime.TryParseExact(oldestProcess, "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startTime))
            {
                TimeSpan uptime = DateTime.Now - startTime;
                string days = (uptime.Days > 0) ? $"{uptime.Days}d " : "";
                string hours = (uptime.Hours > 0) ? $"{uptime.Hours.ToString().PadLeft(2, '0')}h " : "00h";
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
