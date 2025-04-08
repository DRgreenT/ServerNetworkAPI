using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Network.Processor;
using ServerNetworkAPI.dev.Network.Scanner;
using ServerNetworkAPI.dev.UI;

namespace ServerNetworkAPI.dev.Services
{
    public class ScanService
    {
        private int _scanCount = 0;
        private TimeSpan _totalScanTime = TimeSpan.Zero;

        public async Task RunAsync(CancellationToken token)
        {
            await DeviceRepository.LoadAsync();
            OutputFormatter.PrintMessage("Initializing scan loop...");

            while (!token.IsCancellationRequested)
            {
                var scanStart = DateTime.Now;

                HashSet<string> activeIps = ArpScanner.Scan(AppConfig.LocalIpMask);
                int total = activeIps.Count;
                int progress = 0;

                OutputFormatter.PrintMessage($"Found {total} active IPs via arp-scan.");

                foreach (var ip in activeIps)
                {
                    if (AppConfig.IsNmapEnabled)
                    {
                        await DeviceProcessor.ProcessAsync(ip, total, _ =>
                        {
                            progress++;
                            OutputLayout.UpdateRow(22, $"# Nmap Scan: {progress}/{total} ({(progress * 100 / total):F1}%)");
                        });
                    }
                    else
                    {
                        DeviceProcessor.FallbackAdd(ip);
                        progress++;
                    }
                }

                NetworkContext.MarkInactiveDevices(activeIps);

                var scanTime = DateTime.Now - scanStart;
                _totalScanTime += scanTime;
                _scanCount++;

                OutputLayout.UpdateRow(23, $"# Total Scans: {_scanCount}");
                OutputFormatter.PrintMessage($"Scan #{_scanCount} done in {scanTime.TotalSeconds:F1}s. Devices: {NetworkContext.GetDevices().Count}");

                await RunScanCountdown(token);
            }
        }

        private static async Task RunScanCountdown(CancellationToken token)
        {
            for (int i = AppConfig.ScanIntervalSeconds; i > 0; i--)
            {
                if (token.IsCancellationRequested) break;

                OutputLayout.UpdateRow(24, $"# Next scan in {i}s...");
                await Task.Delay(1000, token);
            }

            OutputLayout.UpdateRow(24, "# Scanning...");
        }
    }
}
