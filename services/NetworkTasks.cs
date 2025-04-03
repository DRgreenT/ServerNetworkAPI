using NetworkAPI.Models;
using System.Net;
using NetworkAPI.Outputs;
public class NetworkTasks{
    public static string GetLocalNetworkPrefix()
        {
            var localIPs = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                .Select(ip => ip.ToString())
                .ToList();

            var firstIp = localIPs.FirstOrDefault();
            string fallbackIP = "192.168.1.";
            if (string.IsNullOrEmpty(firstIp))
            {
                Output.Log("Using fallback IP: " + fallbackIP);
                return fallbackIP;
            }

            var parts = firstIp.Split('.');
            if (parts.Length >= 3)
            {
                string str = $"{parts[0]}.{parts[1]}.{parts[2]}.";
                Output.Log("Using IP-mask: " + str);
                return str;
            }
            Output.Log("Using fallback IP: " + fallbackIP);
            return fallbackIP;
        }
        public static async Task<HashSet<string>> GetActiveDevices(string ipMask)
        {
            var activeIps = new HashSet<string>();
            var pingTasks = Enumerable.Range(1, 254).Select(async i =>
            {
                string ip = ipMask + i;
                using var ping = new System.Net.NetworkInformation.Ping();
                try
                {
                    var reply = await ping.SendPingAsync(ip, 500);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        activeIps.Add(ip);
                    }
                }
                catch { }
            });
            await Task.WhenAll(pingTasks);
            return activeIps;
        }
                public static List<OpenPorts> GetOpenPorts(List<string> nmapData)
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
                                port = port,
                                protocolType = portProto[1],
                                service = parts[2]
                            });
                        }
                    }
                }
                return ports;
        }
        public static List<string> GetNmapData(string nmapInfo)
        {
            var info = new List<string>();

            if (string.IsNullOrWhiteSpace(nmapInfo) || nmapInfo.Contains("Note: Host seems down"))
                return info;

            var lines = nmapInfo.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("Running:") ||
                    trimmed.StartsWith("OS details:") ||
                    trimmed.StartsWith("MAC Address:") ||
                    trimmed.StartsWith("Aggressive OS guesses:") ||
                    trimmed.Contains("open"))
                {
                    info.Add(trimmed);
                }
            }
            return info;
        }

        public static string GetOS(List<string> nmapData)
        {
            string os = "unknown";
            foreach (var line in nmapData)
            {
                string lower = line.ToLower();
                if (lower.Contains("windows")) { os = "Windows"; break; }
                if (lower.Contains("linux")) { os = "Linux"; break; }
                if (lower.Contains("android")) { os = "Android"; break; }
                if (lower.Contains("iphone")) { os = "iOS"; break; }
                if (lower.Contains("apple")) { os = "MacOS"; break; }
                if (lower.Contains("freebsd")) { os = "FreeBSD"; break; }
                if (lower.Contains("routeros")) { os = "RouterOS"; break; }
                if (lower.Contains("openwrt")) { os = "OpenWRT"; break; }
            }
            return os;
        }

        public static async Task<string> GetNmapRawData(string ip)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo nmap -O -Pn {ip}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                string output = await process!.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return output;
            }
            catch
            {
                return "";
            }
        }
}