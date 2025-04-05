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

        var pingTasks = Enumerable.Range(1, totalIps).Select(i => PingAndTrack(ipMask, i, activeIps, totalIps, scannedCount, consoleLock));

        await Task.WhenAll(pingTasks);

        FinalizeProgress(consoleLock, totalIps);

        return activeIps;
    }

    private static async Task PingAndTrack(string ipMask, int i, HashSet<string> activeIps, int totalIps, int scannedCount, object consoleLock)
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
        catch (Exception ex)
        {
            Output.Log("Error while ping: " + ex.Message);
        }
        finally
        {
            int current = Interlocked.Increment(ref scannedCount);
            UpdateScanProgress(current, totalIps, consoleLock);
        }
    }

    private static void UpdateScanProgress(int current, int totalIps, object consoleLock)
    {
        lock (consoleLock)
        {
            try
            {
                Output.UpdateProgress(Output.pingStatusRow, totalIps, "pinging IPs  ", current);
                Output.UpdateProgress(Output.nmapStatusRow, 100, "nmap progress", 0);
            }
            catch (Exception ex)
            {
                Output.Log("Error calc. update progress?! " + ex.Message);
            }
        }
    }

    private static void FinalizeProgress(object consoleLock, int totalIps)
    {
        lock (consoleLock)
        {
            try
            {
                Output.UpdateProgress(Output.pingStatusRow, totalIps, "pinging IPs  ", totalIps);
            }
            catch (Exception ex)
            {
                Output.Log("Error final update progress?! " + ex.Message);
            }
        }
    }

    public static List<OpenPorts> GetOpenPorts(List<string> nmapData)
    {
        var ports = new List<OpenPorts>();
        try
        {
            foreach (var line in nmapData)
            {
                ParsePortLine(ports, line);
            }
        }
        catch (Exception ex)
        {
            Output.Log("Error getting port data: " + ex.Message);
        }
        return ports;
    }

    private static void ParsePortLine(List<OpenPorts> ports, string line)
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
                AddRelevantNmapLine(info, line);
            }
        }
        catch (Exception ex)
        {
            Output.Log("Error refining nmap data: " + ex.Message);
        }
        return info;
    }

    private static void AddRelevantNmapLine(List<string> info, string line)
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

    public static System.Diagnostics.ProcessStartInfo CmdNmap(string ip, string parameter = "-O -Pn --host-timeout 5s")
    {
        try
        {
            return new System.Diagnostics.ProcessStartInfo
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
            return null!;
        }
    }

    public static async Task<string> GetNmapRawData(string ip)
    {
        if (!Init.isNmapScanActive)
            return "";

        var psi = CmdNmap(ip);
        if (psi == null) return "";

        try
        {
            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return "";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Timeout 
            var readTask = process.StandardOutput.ReadToEndAsync();
            var waitTask = process.WaitForExitAsync(cts.Token);

            var completed = await Task.WhenAny(waitTask, Task.Delay(-1, cts.Token));

            if (completed != waitTask)
            {
                try
                {
                    process.Kill(entireProcessTree: true); //  
                    Output.Log($"nmap scan for {ip} timed out and was killed.", false);
                    OutputManager.EditRow(18, $"# nmap scan for {ip} timed out and was killed");
                }
                catch (Exception ex)
                {
                    Output.Log($"Error killing timed out process: {ex.Message}", false);
                    OutputManager.EditRow(18, $"# Error killing timed out process: {ex.Message}");
                }
                
                return "";
            }

            return await readTask;
        }
        catch (Exception ex)
        {
            Output.Log("Error executing nmap: " + ex.Message, false);
            return "";
        }
    }
}
