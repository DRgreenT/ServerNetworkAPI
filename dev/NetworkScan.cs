using System.Net;
using System.Text.Json;

namespace ServerNetworkAPI.dev
{
    public class NetworkScan : BackgroundService
    {
        private readonly object _lock = new();
        private bool isOnExit = false;
        private string localIP = "";
        private CancellationTokenSource _displayTokenSource = new();

        private int _completedScans = 0;

        public List<Device> GetDevices() =>
            GetOrderedDevices();

        public static List<string> GetLocalIPv4Addresses() =>
            Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToList();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => Output.DisplayLoop(_displayTokenSource.Token));

            localIP = Init.GetLocalNetworkPrefix();
            await FileSystem.LoadDeviceFromJson(_lock);
            int scanCount = 0;

            OutputManager.EditRow(10, "#");
            OutputManager.EditRow(11, "# Initial scan...");
            TimeSpan totalScanTime = TimeSpan.Zero;

            while (!stoppingToken.IsCancellationRequested)
            {
                var scanStart = DateTime.Now;
                var activeIps = await NetworkTasks.GetActiveDevices(localIP);
                await ScanNetwork(activeIps);

                var scanDuration = DateTime.Now - scanStart;
                totalScanTime += scanDuration;
                scanCount++;

                var avgDuration = TimeSpan.FromSeconds(totalScanTime.TotalSeconds / scanCount);

                if (Init.isInitialScan)
                {
                    OutputManager.EditRow(11, "# Initial scan...done!");
                    Init.isInitialScan = false;
                }

                if (!isOnExit)
                {
                    OutputManager.EditRow(Output.totalScanStatusRow, $"# Scans: {scanCount}, Devices found: {Init.devices.Count}, avg scan time: {avgDuration.TotalSeconds:F1}s");
                    Output.Log($"Scan #{scanCount} completed in {scanDuration.TotalSeconds:F1}s. Devices found: {Init.devices.Count}", false);
                }

                await RunScanCountdown(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!isOnExit)
                isOnExit = true;

            List<Device> snapshot;
            lock (_lock)
            {
                snapshot = Init.devices.ToList();
            }

            _displayTokenSource.Cancel();
            await FileSystem.SaveDevicesToJson(snapshot);
            await base.StopAsync(cancellationToken);
            Console.SetCursorPosition(0, 26);
            Console.WriteLine("\n");
        }

        private List<Device> GetOrderedDevices()
        {
            lock (_lock)
            {
                return Init.devices
                    .Select(d => new Device
                    {
                        IP = d.IP,
                        Hostname = d.Hostname,
                        OS = d.OS,
                        IsOnline = d.IsOnline,
                        Ports = d.Ports,
                        index = int.TryParse(d.IP[(d.IP.LastIndexOf('.') + 1)..], out var idx) ? idx : 0
                    })
                    .OrderBy(x => x.index)
                    .ToList();
            }
        }

        private async Task RunScanCountdown(CancellationToken token)
        {
            for (int i = Init.timeOut; i > 0; i--)
            {
                if (token.IsCancellationRequested) break;

                OutputManager.EditRow(Output.nextScanStatusRow, $"# Next scan in {i} seconds...");
                await Task.Delay(1000, token);

                if (i == 1)
                    OutputManager.EditRow(Output.nextScanStatusRow, "# Scanning..." + new string(' ', Console.WindowWidth));
            }
        }

        private async Task ScanNetwork(HashSet<string> activeIps)
        {
            int total = activeIps.Count;
            _completedScans = 0;

            if (Init.isNmapScanActive)
            {
                var detailTasks = activeIps.Select(ip => ProcessDeviceNmapScan(ip, total));
                await Task.WhenAll(detailTasks);
                Output.UpdateProgress(Output.nmapStatusRow, total, "nmap progress", total);
            }
            else
            {
                Output.UpdateProgress(Output.nmapStatusRow, 100, "nmap progress", 0);
            }

            CleanupInactiveDevices(activeIps);
        }

        private async Task ProcessDeviceNmapScan(string ip, int total)
        {
            string nmapRaw = await NetworkTasks.GetNmapRawData(ip);
            var nmapData = NetworkTasks.RefineNmapData(nmapRaw);
            if (nmapData.Count == 0)
            {
                Output.UpdateProgress(Output.nmapStatusRow, total, "nmap progress", total);
                return;
            }

            string hostname = GetHostname(ip);
            string os = NetworkTasks.GetOS(nmapData);
            var ports = NetworkTasks.GetOpenPorts(nmapData);

            lock (_lock)
            {
                var existing = Init.devices.FirstOrDefault(d => d.IP == ip);
                if (existing != null)
                {
                    existing.Hostname = hostname;
                    existing.OS = os;
                    existing.IsOnline = true;
                    existing.Ports = ports;
                }
                else
                {
                    Init.devices.Add(new Device
                    {
                        IP = ip,
                        Hostname = hostname,
                        OS = os,
                        IsOnline = true,
                        Ports = ports
                    });
                }
                _completedScans++;
            }

            Output.UpdateProgress(Output.nmapStatusRow, total, "nmap progress", _completedScans);
        }

        private string GetHostname(string ip)
        {
            try
            {
                return Dns.GetHostEntry(ip).HostName;
            }
            catch
            {
                Output.Log("No hostname found: " + ip);
                return "-";
            }
        }

        private void CleanupInactiveDevices(HashSet<string> activeIps)
        {
            lock (_lock)
            {
                foreach (var dev in Init.devices)
                {
                    if (!activeIps.Contains(dev.IP))
                    {
                        dev.IsOnline = false;
                        dev.OS = "";
                        dev.Ports = new List<OpenPorts>();
                    }
                    else if (!Init.isNmapScanActive)
                    {
                        dev.OS = "";
                        dev.Ports = new List<OpenPorts>();
                    }
                }
            }
        }
    }
}
