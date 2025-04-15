using ServerNetworkAPI.dev.Network.Adapter;
using System.Diagnostics;
using System.Net;
using System.Text;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;

namespace ServerNetworkAPI.dev.Network.Scanner
{
    public class ArpScanner
    {
        public static DateTime LastScanTime = DateTime.Now;
        public static HashSet<string> Scan(string ipPrefix)
        {
            var output = new StringBuilder();


            var adapters = LocalAdapterService.GetActiveAdapterNames();


            foreach (var adapter in adapters)
            {
                LogData log = new();
                var psi = BuildArpScanCommand(adapter, ipPrefix);
                if (psi == null) continue;

                try
                {
                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        string stdOut = process.StandardOutput.ReadToEnd();
                        string stdErr = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        output.AppendLine(stdOut);

                        if (!string.IsNullOrWhiteSpace(stdErr))
                        {
                            output.AppendLine($"[ERROR] {stdErr}");

                            log = LogData.NewData(
                                "ArpScanner",
                                $"Adapter: {adapter} → {stdErr}",
                                MessageType.Error,
                                ""
                            );

                            Logger.Log(log);
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.AppendLine($"[EXCEPTION] Adapter: {adapter} → {ex.Message}");

                    log = LogData.NewData(
                        "ArpScanner",
                        $"Adapter: {adapter}",
                        MessageType.Exception,
                        Logger.RemoveNewLineSymbolFromString(ex.Message)
                    );

                    Logger.Log(log);
                }

            }
            LastScanTime = DateTime.Now;
            return ParseIpAddresses(output.ToString());
        }



        private static ProcessStartInfo? BuildArpScanCommand(string interfaceName, string ipPrefix)
        {
            try
            {
                ipPrefix = ipPrefix.TrimEnd('.');
                return new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo arp-scan --interface={interfaceName} {ipPrefix}.0/24\"",
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

        private static HashSet<string> ParseIpAddresses(string rawOutput)
        {
            var ips = new HashSet<string>();
            var lines = rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1 && IsValidIPv4(parts[0]))
                {
                    ips.Add(parts[0]);
                }
            }

            string localIp = LocalAdapterService.GetLocalIPv4Address();
            if (IsValidIPv4(localIp))
            {
                ips.Add(localIp);
            }

            return ips;
        }

        private static bool IsValidIPv4(string ip)
        {
            if (!IPAddress.TryParse(ip, out var address))
                return false;

            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            string[] segments = ip.Split('.');
            return segments.Length == 4 && segments.All(seg => int.TryParse(seg, out int num) && num >= 0 && num <= 255);
        }
    }
}
