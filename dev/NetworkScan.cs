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

        public List<Device> GetDevices()
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
                        index = int.TryParse(d.IP.Substring(d.IP.LastIndexOf(".") + 1), out var idx) ? idx : 0
                    })
                    .OrderBy(x => x.index)
                    .ToList();
            }
        }

        public static List<string> GetLocalIPv4Addresses()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToList();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => Output.DisplayLoop(_displayTokenSource.Token));


            localIP = Init.GetLocalNetworkPrefix();
            await FileSystem.LoadDeviceFromJson(_lock);
            int scanCount = 0;

            OutputManager.EditRow(10, "#");
            OutputManager.EditRow(11,"# Initial scan...");
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
                
                // Countdown bis zum nächsten Scan
                for (int i = Init.timeOut; i > 0; i--)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    OutputManager.EditRow(Output.nextScanStatusRow,$"# Next scan in {i} seconds...");
                    await Task.Delay(1000, stoppingToken);
                    if(i == 1) OutputManager.EditRow(Output.nextScanStatusRow, $"# Scanning..." + new string(' ',Console.WindowWidth));
                }
                
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!isOnExit)
            {
                isOnExit = true;
            }

            List<Device> snapshot;
            lock (_lock)
            {
                snapshot = Init.devices.ToList();
            }
            _displayTokenSource.Cancel(); // Anzeige-Loop beenden
            await FileSystem.SaveDevicesToJson(snapshot);
            await base.StopAsync(cancellationToken);
        }

        private int _completedScans = 0;

        private async Task ScanNetwork(HashSet<string> activeIps)
        {
            int total = activeIps.Count;
            _completedScans = 0;

            var detailTasks = activeIps.Select(async ip =>
            {
                string nmapRaw = await NetworkTasks.GetNmapRawData(ip);
                var nmapData = NetworkTasks.RefineNmapData(nmapRaw);
                if (nmapData.Count == 0)
                {
                    Output.UpdateProgress(Output.nmapStatusRow, total, "nmap progress", total);
                    return;
                }

                string hostname = "-";
                try
                {
                    hostname = Dns.GetHostEntry(ip).HostName;
                }
                catch { }

                string os = NetworkTasks.GetOS(nmapData);

                lock (_lock)
                {
                    var existing = Init.devices.FirstOrDefault(d => d.IP == ip);
                    if (existing != null)
                    {
                        existing.Hostname = hostname;
                        existing.OS = os;
                        existing.IsOnline = true;
                        existing.Ports = NetworkTasks.GetOpenPorts(nmapData);
                    }
                    else
                    {
                        Init.devices.Add(new Device
                        {
                            IP = ip,
                            Hostname = hostname,
                            OS = os,
                            IsOnline = true,
                            Ports = NetworkTasks.GetOpenPorts(nmapData)
                        });
                    }
                    _completedScans++;
                }

                Output.UpdateProgress(Output.nmapStatusRow,total, "nmap progress", _completedScans);
            });

            await Task.WhenAll(detailTasks);
            Output.UpdateProgress(Output.nmapStatusRow, total, "nmap progress", total);
            lock (_lock)
            {
                foreach (var dev in Init.devices)
                {
                    if (!activeIps.Contains(dev.IP))
                        dev.IsOnline = false;
                }
            }
        }


    }
}
