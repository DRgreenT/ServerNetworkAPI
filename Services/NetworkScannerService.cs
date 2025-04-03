using System.Net;
using System.Text.Json;
using NetworkAPI.Models;
using NetworkAPI.Outputs;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace NetworkAPI.Services
{
    public class NetworkScannerService : BackgroundService
    {
        private readonly List<Device> _devices = new();
        private readonly object _lock = new();
        private const string SaveFilePath = "devices.json";
        private bool isOnExit = false;
        private string localIP = "";

        public List<Device> GetDevices()
        {
            lock (_lock)
            {
                return _devices
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
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(SaveFilePath);
                    var loaded = JsonSerializer.Deserialize<List<Device>>(json);
                    if (loaded != null)
                    {
                        lock (_lock)
                        {
                            _devices.Clear();
                            _devices.AddRange(loaded);
                        }

                        Output.Log("Device list loaded.");

                        var localIPs = Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(ip => ip.ToString());

                        Console.WriteLine("\nAPI is now online!");

                        foreach (var ip in localIPs)
                        {
                            Console.WriteLine($"->  http://{ip}:{ApiControllerSettings.WebApiPort}/{ApiControllerSettings.WebApiName}");
                        }
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
                    Console.Write($"Scans: {scanCount}, Devices found: {_devices.Count}, avg scan time: {avgDuration.TotalSeconds:F1}s");
                    Output.Log($"Scan #{scanCount} completed in {scanDuration.TotalSeconds:F1}s. Devices found: {_devices.Count}", false);
                }


                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
                snapshot = _devices.ToList();
            }
            Console.WriteLine("\n");
            Output.OverrideConsoleLine(Console.CursorTop);
            try
            {
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SaveFilePath, json);
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
                    var existing = _devices.FirstOrDefault(d => d.IP == ip);
                    if (existing != null)
                    {
                        existing.Hostname = hostname;
                        existing.OS = os;
                        existing.IsOnline = true;
                        existing.Ports = NetworkTasks.GetOpenPorts(nmapData);
                    }
                    else
                    {
                        _devices.Add(new Device
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
                foreach (var dev in _devices)
                {
                    if (!activeIps.Contains(dev.IP))
                        dev.IsOnline = false;
                }
            }
        }
    }
}
