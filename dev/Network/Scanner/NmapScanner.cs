using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using ServerNetworkAPI.dev.Network.Adapter;
using ServerNetworkAPI.dev.Services;

namespace ServerNetworkAPI.dev.Network.Scanner
{
    public class NmapScanner
    {
        public static async Task<NmapData> GetNmapDataAsync(string ip, string parameter = "-O -Pn --osscan-guess --host-timeout 60s")
        {
            if (!AppConfig.IsNmapEnabled)
                return new();

            try
            {
                string command = BuildNmapCommand(ip, parameter);
                var output = await BashCmd.ExecuteCmdAsync(command, "NmapScanner");

                if (string.IsNullOrWhiteSpace(output))
                {
                    Logger.Log(LogData.NewLogEvent(
                        "NmapScanner",
                        $"Empty scan {ip} (timed out)",
                        MessageType.Warning,
                        ""));
                    return new();
                }

                var result = ParseNmapOutput(output);

                return ExtractFromRawData(result);
            }
            catch (Exception ex)
            {
                Logger.Log(LogData.NewLogEvent(
                    "NmapScanner",
                    $"Error scan {ip}",
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
                Program.isInitNmap = false;

                string cmd = $"echo sudo -S nmap {parameter} {ip}";

                return cmd;
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
                    trimmed.StartsWith("OS CPE:") ||
                    trimmed.StartsWith("OS details:") ||
                    trimmed.StartsWith("MAC Address:") ||
                    trimmed.StartsWith("Aggressive OS guesses:") ||
                    trimmed.Contains("open") ||
                    trimmed.StartsWith("Nmap scan report for") ||
                    trimmed.StartsWith("Network Distance:"))
                {
                    result.Add(trimmed);
                }
            }

            return result;
        }

        private static NmapData ExtractFromRawData(List<string> nmapData)
        {
            bool hasSearcedOS = false;

            var nmapDataObj = new NmapData();
            bool macAddressFound = false;

            foreach (var line in nmapData)
            {
                if(line.Contains(LocalAdapterService.GetLocalIPv4Address()))
                {
                    nmapDataObj.MACAddress = LocalAdapterService.GetLocalMacAdress();
                    macAddressFound = true;
                }
                if (!hasSearcedOS)
                {
                    if (line.StartsWith("Running:") || line.StartsWith("Aggressive OS guesses: ") || line.StartsWith("OS CPE:") || line.StartsWith("OS details:") || line.StartsWith("Nmap scan report for"))
                    {
                        nmapDataObj.OS = ExtractOS(line);
                        if (nmapDataObj.OS != "unknown")
                            hasSearcedOS = true;
                    }
                }

                if (line.StartsWith("MAC Address:") && !macAddressFound)
                {
                    nmapDataObj.MACAddress = ExtractMacAddress(line);
                }

                if (line.StartsWith("Network Distance:"))
                {
                    nmapDataObj.NetworkDistance = ExtractNetworkDistance(line);
                }


                if (line.Contains("open"))
                {
                    nmapDataObj.OpenPorts.Add(ExtractOpenPorts(line));
                }

            }
            return nmapDataObj;

        }

        static string[] appleOS = { "iOS", "Mac", "MacOS", "IPhone", "iPhone", "iPad", "iPod", "iOS device", "Apple", "ipadOS", "AFS3-Fileserver", "macOS" };
        static string[] androidOS = { "Redme", "Android", "Google", "Galaxy", "Samsung", "Pixel", "Huawei", "OnePlus", "Xiaomi", "MIUI", "Oppo", "Realme", "Motorola", "Sony", "Nexus", "Android TV", "Android Wear", };
        static string[] windowsOS = { "Microsoft", "Windows", "Win", "MSIE", "Windows NT", "Windows 10", "Windows 11", "Windows 7", "Windows 8", "Windows XP", "Windows Vista", "Win32", "Win64", "XBOX", "Xbox", "XBox" };
        static string[] linuxOS = { "Linux", "Debian", "Ubuntu", "Fedora", "CentOS", "Red Hat", "Arch", "Raspbian", "Kali", "Mint", "openSUSE", "Gentoo", "Slackware" };
        private static bool ContainsAny(string target, string[] values)
        {
            return values.Any(value => target.Contains(value, StringComparison.OrdinalIgnoreCase));
        }

        public static string ExtractOS(string line)
        {
            string os = "unknown";

            if (line.StartsWith("Running:") || line.StartsWith("Aggressive OS guesses: ") || line.StartsWith("OS CPE:") || line.StartsWith("OS details:") || line.StartsWith("Nmap scan report for"))
            {
                if (ContainsAny(line, androidOS)) return "Android";
                if (ContainsAny(line, windowsOS)) return "MS";
                if (ContainsAny(line, linuxOS)) return "Linux";
                if (ContainsAny(line, appleOS)) return "Apple";
            }
            return os;
        }

        public static string ExtractMacAddress(string line)
        {

            if (line.StartsWith("MAC Address:"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    return parts[2];
                }
            }

            return string.Empty;
        }

        public static string ExtractNetworkDistance(string line)
        {

            if (line.StartsWith("Network Distance:"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    return parts[2];
                }
            }
            return string.Empty;
        }
        public static OpenPorts ExtractOpenPorts(string line)
        {
            var ports = new OpenPorts();

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && parts[0].Contains("/") && parts[1] == "open")
            {
                var portProto = parts[0].Split('/');
                if (portProto.Length == 2 && int.TryParse(portProto[0], out var port))
                {
                    ports.Port = port;
                    ports.ProtocolType = portProto[1];
                    ports.Service = parts[2];
                }
            }
            return ports;
        }
    }
}
