using System.Net;
using System.Text.Json;
using NetworkAPI.Models;

namespace NetworkAPI.Services
{
    public class NetworkScannerService : BackgroundService
    {
        private readonly List<Device> _devices = new();
        private readonly object _lock = new();

        private const string SaveFilePath = "devices.json";
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

        string localIP = "";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            localIP = GetLocalNetworkPrefix();
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

                        Console.WriteLine(" device list loaded...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" error while loading device list: {ex.Message}");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await ScanNetwork();
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            List<Device> snapshot;
            lock (_lock)
            {
                snapshot = _devices.ToList();
            }

            try
            {
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SaveFilePath, json);
                Console.WriteLine(" device list saved...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" error while saving device list: {ex.Message}");
            }

            await base.StopAsync(cancellationToken);
        }

         private string GetLocalNetworkPrefix()
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
                Console.WriteLine(" using fallback IP: " + fallbackIP);
                return fallbackIP; // fallback
            }

            var parts = firstIp.Split('.');
            if (parts.Length >= 3)
            {
                string str = $"{parts[0]}.{parts[1]}.{parts[2]}.";
                Console.WriteLine(" using IP: " + str );
                return str;
            }
            Console.WriteLine(" using fallback IP: " + fallbackIP);
            return fallbackIP;
        }


        private async Task ScanNetwork()
        {
            string baseIp = localIP;
            var activeIps = new HashSet<string>();

            var tasks = Enumerable.Range(1, 254).Select(async i =>
            {
                string ip = baseIp + i;

                string nmapRaw = await Nmap.GetNmapInfo(ip);
                var nmapData = Nmap.GetNmaps(nmapRaw);
                if (nmapData.Count == 0) return;

                string hostname = "-";
                try
                {
                    hostname = Dns.GetHostEntry(ip).HostName;
                }
                catch { }

                string os = "unknown";
                foreach (var line in nmapData)
                {
                    string lower = line.ToLower();
                    if (lower.Contains("windows")) { os = "Windows"; break; }
                    if (lower.Contains("linux")) { os = "Linux"; break; }
                    if (lower.Contains("android")) { os = "Android"; break; }
                    if (lower.Contains("iphone")) { os = "iOS"; break; }
                    if (lower.Contains("apple")) { os = "MacOS"; break; } // adjust!!
                    if (lower.Contains("freebsd")) { os = "FreeBSD"; break; }
                    if (lower.Contains("routeros")) { os = "RouterOS"; break; }
                    if (lower.Contains("openwrt")) { os = "OpenWRT"; break; }
                }

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

                lock (_lock)
                {
                    var existing = _devices.FirstOrDefault(d => d.IP == ip);
                    if (existing != null)
                    {
                        existing.Hostname = hostname;
                        existing.OS = os;
                        existing.IsOnline = true;
                        existing.Ports = ports;
                    }
                    else
                    {
                        _devices.Add(new Device
                        {
                            IP = ip,
                            Hostname = hostname,
                            OS = os,
                            IsOnline = true,
                            Ports = ports
                        });
                    }
                }

                activeIps.Add(ip);
            });

            await Task.WhenAll(tasks);

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
