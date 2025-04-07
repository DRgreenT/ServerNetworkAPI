using System.Net;
using System.Text;

namespace ServerNetworkAPI.dev
{
    public class ArpScan
    {
        public List<string> GetIps()
        {
            string rawOutput = GetArpResults(Init.GetLocalNetworkPrefix());
            return ParseIpAddresses(rawOutput);
        }

        private static string GetArpResults(string ipMask)
        {
            localNetworkAdapter.GetActiveAdapters();
            StringBuilder sb = new();

            foreach (string adapter in localNetworkAdapter.activeAdapters)
            {
                var psi = StartArpCommand(adapter);
                if (psi == null) continue;

                try
                {
                    using var process = System.Diagnostics.Process.Start(psi);
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        sb.AppendLine($"### Adapter: {adapter} ###");
                        sb.AppendLine(output);

                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            sb.AppendLine($"[ERROR] {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"[EXCEPTION] Adapter: {adapter} → {ex.Message}");
                }
            }

            return sb.ToString();
        }

        private static System.Diagnostics.ProcessStartInfo StartArpCommand(string interfaceName)
        {
            try
            {
                string ipMask = Init.GetLocalNetworkPrefix().TrimEnd('.');
                return new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo arp-scan --interface={interfaceName} {ipMask}.0/24\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            catch
            {
                Output.Log("Arp command not executed");
                return null!;
            }
        }

        private static List<string> ParseIpAddresses(string rawOutput)
        {
            var ips = new List<string>();
            var lines = rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                if (parts.Length >= 1 && IPAddress.TryParse(parts[0], out _))
                {
                    ips.Add(parts[0]);
                }
            }

            return ips;
        }
    }
}
