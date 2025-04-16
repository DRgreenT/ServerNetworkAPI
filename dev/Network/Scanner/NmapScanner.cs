
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

            try
            {
                string command = BuildNmapCommand(ip, parameter);
                var output = await BashCmd.ExecuteCmdAsync(command, "NmapScanner");

                if (string.IsNullOrWhiteSpace(output))
                {
                    Logger.Log(LogData.NewData(
                        "NmapScanner",
                        $"Leere Ausgabe beim Scan von {ip}",
                        MessageType.Warning,
                        ""));
                    return new();
                }

                return ParseNmapOutput(output);
            }
            catch (Exception ex)
            {
                Logger.Log(LogData.NewData(
                    "NmapScanner",
                    $"Fehler beim Scannen von {ip}",
                    MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                ));
                return new();
            }
        }

        private static string BuildNmapCommand(string ip, string parameter)
        {
            if (Program.isInitNmap)
            {
                string passwordEscaped = Program.InitPassword.Replace("'", "'\\''");
                Program.isInitNmap = false;
                return $"echo '{passwordEscaped}' | sudo -S nmap {parameter} {ip}";
            }
            else
            {
                return $"sudo nmap {parameter} {ip}";
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
