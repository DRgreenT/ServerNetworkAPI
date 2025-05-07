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
        private static int aniIndex = 0;
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
                    if (token.IsCancellationRequested)
                        return;

                    if (AppConfig.IsNmapEnabled)
                    {
                        await DeviceProcessor.ProcessAsync(ip, _ =>
                        {
                            progress++;
                            OutputLayout.UpdateRow(27, $"# Nmap Scan: {progress}/{total} ({(progress * 100 / total):F1}%)");
                        });
                    }
                    else
                    {
                        DeviceProcessor.FallbackAdd(ip);
                        progress++;
                    }
                }

                if (token.IsCancellationRequested)
                    return;

                NetworkContext.MarkInactiveDevices(activeIps);

                var scanTime = DateTime.Now - scanStart;
                _totalScanTime += scanTime;
                _scanCount++;

                OutputLayout.UpdateRow(28, $"# Total Scans: {_scanCount}");
                OutputFormatter.PrintMessage($"Scan #{_scanCount} done in {scanTime.TotalSeconds:F1}s. Devices: {NetworkContext.GetDevices().Count}");

                await RunScanCountdown(token);
            }
        }

        static readonly string[] anmimation = { "", ".", "..", "..." };
        public static async Task ScanAnimation(CancellationToken token)
        {
     
            while (!token.IsCancellationRequested)
            {
                if (!isDelayRunning)
                {
                    await Task.Delay(150);

                    aniIndex = aniIndex < anmimation.Length - 1 ? aniIndex + 1 : 0;
                    OutputLayout.UpdateRow(29, $"# Scanning{anmimation[aniIndex]}");
                }
            }
        }

        public static bool isDelayRunning = false;
        private static async Task RunScanCountdown(CancellationToken token)
        {
            isDelayRunning = true;
            for (int i = AppConfig.ScanIntervalSeconds; i > 0; i--)
            {
                if (token.IsCancellationRequested) break;

                OutputLayout.UpdateRow(29, $"# Next scan in {i}s...");
                await Task.Delay(1000, token);
            }
            isDelayRunning = false;

        }
    }
}
