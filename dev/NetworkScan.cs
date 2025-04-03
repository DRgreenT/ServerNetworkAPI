using System.Net;
using System.Text.Json;

namespace ServerNetworkAPI.dev
{
    public class NetworkScan : BackgroundService
    {
        
        private readonly object _lock = new();
        
        private bool isOnExit = false;
        private string localIP = "";

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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            localIP = NetworkTasks.GetLocalNetworkPrefix();
            if (File.Exists(Init.SaveFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(Init.SaveFilePath);
                    var loaded = JsonSerializer.Deserialize<List<Device>>(json);
                    if (loaded != null)
                    {
                        lock (_lock)
                        {
                            Init.devices.Clear();
                            Init.devices.AddRange(loaded);
                        }

                        Output.Log("Device list loaded.");

                        var localIPs = Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(ip => ip.ToString());

                        Console.WriteLine("\nAPI is now online!");

                        foreach (var ip in localIPs)
                        {
                            Console.WriteLine($"->  http://{ip}:{Init.WebApiPort}/{Init.WebApiName}");
                        }
                        Console.WriteLine($"Make sure port {Init.WebApiPort} is not blocked by firewall!\n");
                    }
                }
                catch (Exception ex)
                {
                    Output.Log($"Error while loading device list: {ex.Message}");
                }
            }

            bool isInitialScan = true;
            int scanCount = 0;
            Console.Write("\nInitial scan...");
            int coursorRow = Console.CursorTop;
            TimeSpan totalScanTime = TimeSpan.Zero;

            while (!stoppingToken.IsCancellationRequested)
            {
                var scanStart = DateTime.Now;
                await ScanNetwork();
                var scanDuration = DateTime.Now - scanStart;
                totalScanTime += scanDuration;
                scanCount++;

                var avgDuration = TimeSpan.FromSeconds(totalScanTime.TotalSeconds / scanCount);

                if (isInitialScan)
                {
                    isInitialScan = false;
                    Console.Write("done!\n");
                }
                if(!isOnExit)
                {
                    if(coursorRow <= Console.CursorTop)
                    {
                        for(int i = Console.CursorTop; i > coursorRow; i--) 
                        {
                            Output.OverrideConsoleLine(coursorRow);
                        }
                    }
                    Output.OverrideConsoleLine(coursorRow);
                    Console.Write($"Scans: {scanCount}, Devices found: {Init.devices.Count}, avg scan time: {avgDuration.TotalSeconds:F1}s");
                    Output.Log($"Scan #{scanCount} completed in {scanDuration.TotalSeconds:F1}s. Devices found: {Init.devices.Count}", false);
                }
                await Task.Delay(TimeSpan.FromSeconds(Init.timeOut), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if(!isOnExit)
            {
                isOnExit = true;
            }
            List<Device> snapshot;
            lock (_lock)
            {
                snapshot = Init.devices.ToList();
            }
            Console.WriteLine("\n");
            Output.OverrideConsoleLine(Console.CursorTop);
            try
            {
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Init.SaveFilePath, json);
                Output.Log(" Device list saved.\n");
            }
            catch (Exception ex)
            {
                Output.Log($" Error while saving device list: {ex.Message}");
            }
            await base.StopAsync(cancellationToken);
        }
        
        private async Task ScanNetwork()
        {                     
            var activeIps = await NetworkTasks.GetActiveDevices(localIP);
            var detailTasks = activeIps.Select(async ip =>
            {
                string nmapRaw = await NetworkTasks.GetNmapRawData(ip);
                var nmapData = NetworkTasks.GetNmapData(nmapRaw);
                if (nmapData.Count == 0) return;

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
                }
            });

            await Task.WhenAll(detailTasks);

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
