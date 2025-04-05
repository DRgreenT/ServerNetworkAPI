using System.Net;
using ServerNetworkAPI.dev;
public class NetworkTasks
{

    public static async Task<HashSet<string>> GetActiveDevices(string ipMask)
    {
        var activeIps = new HashSet<string>();
        int totalIps = 254;
        int scannedCount = 0;
        object consoleLock = new();

        var pingTasks = Enumerable.Range(1, totalIps).Select(async i =>
        {
            string ip = ipMask + i;
            using var ping = new System.Net.NetworkInformation.Ping();
            try
            {
                var reply = await ping.SendPingAsync(ip, 500);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    lock (activeIps)
                        activeIps.Add(ip);
                }
            }
            catch(Exception ex) 
            {
                Output.Log("Error while ping: " + ex.Message);
            }
            finally
            {
                int current = Interlocked.Increment(ref scannedCount);
                double percent = (double)current / totalIps * 100;

                lock (consoleLock)
                {
                    try
                    {
                        Output.UpdateProgress(Output.pingStatusRow, totalIps, "pinging IPs  ", current);
                        Output.UpdateProgress(Output.nmapStatusRow, 100, "nmap progress", 0);
                    }
                    catch(Exception ex)
                    {
                        Output.Log("Error clac. update progress?!" + ex.Message);
                    }
                }
            }
        });

        await Task.WhenAll(pingTasks);

        lock (consoleLock)
        {
            try
            {
                Output.UpdateProgress(Output.pingStatusRow, totalIps, "pinging IPs  ", totalIps);
            }
            catch (Exception ex)
            { 
                Output.Log("Error clac. update progress?!" + ex.Message); 
            }
        }

        return activeIps;
    }


    public static List<OpenPorts> GetOpenPorts(List<string> nmapData)
    {
        var ports = new List<OpenPorts>();
        try
        {
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
        }
        catch (Exception ex)
        {   
            Output.Log("Error getting port data: " + ex.Message); 
        }
        return ports;
    }
    public static List<string> RefineNmapData(string nmapInfo)
    {
        var info = new List<string>();

        if (string.IsNullOrWhiteSpace(nmapInfo) || nmapInfo.Contains("Note: Host seems down"))
            return info;
        try
        {
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
        }
        catch(Exception ex)
        {
            Output.Log("Error refining nmap data: " + ex.Message);
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
        if (Init.isNmapScanActive)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo nmap -O -Pn {ip}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                string output = await process!.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return output;
            }
            catch(Exception ex)
            {
                Output.Log("Error refining nmap data: " + ex.Message);
                return "";
            }
        }
        return "";
    }
}