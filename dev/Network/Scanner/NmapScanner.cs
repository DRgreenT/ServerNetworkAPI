using System.Diagnostics;
using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
namespace ServerNetworkAPI.dev.Network.Scanner
{
    public class NmapScanner
    {
        public static async Task<List<string>> GetNmapDataAsync(string ip, string parameter = "-O -Pn --host-timeout 5s")
        {
            if (!AppConfig.IsNmapEnabled)
                return new();

            var psi = BuildNmapCommand(ip, parameter);
            if (psi == null) return new();

            LogData log = new LogData();

            try
            {
                using var process = Process.Start(psi);
                if (process == null) return new();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var readTask = process.StandardOutput.ReadToEndAsync();
                var waitTask = process.WaitForExitAsync(cts.Token);

                var completed = await Task.WhenAny(waitTask, Task.Delay(-1, cts.Token));

                if (completed != waitTask)
                {                  
                    try 
                    { 
                        process.Kill(entireProcessTree: true);

                        log = LogData.NewData(
                            "NmapScanner",
                            $"Process killed for {ip}",
                            MessageType.Warning,
                            ""
                        );
                    } 
                    catch(Exception ex)
                    {
                        log = LogData.NewData(
                            "NmapScanner",
                            $"Error killing process for {ip}",
                            MessageType.Exception,
                            Logger.RemoveNewLineSymbolFromString(ex.Message)
                        );
                    }
                    Logger.Log(log);
                    return new();
                }

                var output = await readTask;
                return ParseNmapOutput(output);
            }
            catch (Exception ex)
            {
                string exMessage = Logger.RemoveNewLineSymbolFromString(ex.Message);

                log = LogData.NewData(
                    "NmapScanner",
                    $"Error scanning {ip}",
                    MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                );
                
                Logger.Log(log);
                return new();
            }
        }

        private static ProcessStartInfo? BuildNmapCommand(string ip, string parameter)
        {
            try
            {
                return new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo nmap {parameter} {ip}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            catch
            {
                return null;
            }
        }

        private static List<string> ParseNmapOutput(string rawOutput)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(rawOutput) || rawOutput.Contains("Note: Host seems down"))
                return result;

            var lines = rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Running:") ||
                    trimmed.StartsWith("OS details:") ||
                    trimmed.StartsWith("MAC Address:") ||
                    trimmed.StartsWith("Aggressive OS guesses:") ||
                    trimmed.Contains("open"))
                {
                    result.Add(trimmed);
                }
            }

            return result;
        }

        public static string ExtractOS(List<string> nmapData)
        {
            string os = "unknown";
            foreach (var line in nmapData)
            {
                var lower = line.ToLower();
                if (lower.Contains("windows")) return "Windows";
                if (lower.Contains("linux")) return "Linux";
                if (lower.Contains("android")) return "Android";
                if (lower.Contains("iphone")) return "iOS";
                if (lower.Contains("apple")) return "MacOS";
                if (lower.Contains("freebsd")) return "FreeBSD";
                if (lower.Contains("routeros")) return "RouterOS";
                if (lower.Contains("openwrt")) return "OpenWRT";
            }
            return os;
        }

        public static List<OpenPorts> ExtractOpenPorts(List<string> nmapData)
        {
            var ports = new List<OpenPorts>();
            foreach (var line in nmapData)
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && parts[0].Contains("/") && parts[1] == "open")
                {
                    var portProto = parts[0].Split('/');
                    if (portProto.Length == 2 && int.TryParse(portProto[0], out var port))
                    {
                        ports.Add(new OpenPorts
                        {
                            Port = port,
                            ProtocolType = portProto[1],
                            Service = parts[2]
                        });
                    }
                }
            }
            return ports;
        }
    }
}
