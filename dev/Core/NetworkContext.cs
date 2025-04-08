using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.IO;
namespace ServerNetworkAPI.dev.Core
{
    public class NetworkContext
    {
        private static readonly object _lock = new();
        private static readonly List<Device> _devices = new();

        public static IReadOnlyList<Device> GetDevices()
        {
            lock (_lock)
            {
                return _devices
                    .Select(d => d.CloneWithIndex())
                    .OrderBy(d => d.Index)
                    .ToList();
            }
        }

        public static void SetDevices(List<Device> newDevices)
        {
            lock (_lock)
            {
                _devices.Clear();
                _devices.AddRange(newDevices);
            }
        }

        public static void AddOrUpdateDevice(Device updatedDevice)
        {
            lock (_lock)
            {
                var existing = _devices.FirstOrDefault(d => d.IP == updatedDevice.IP);
                if (existing != null)
                {
                    existing.Hostname = updatedDevice.Hostname;
                    existing.OS = updatedDevice.OS;
                    existing.IsOnline = updatedDevice.IsOnline;
                    existing.Ports = updatedDevice.Ports;

                    if(IsolateLastIPNumber(updatedDevice.IP) > AppConfig.MaxIPv4AddressWithoutWarning)
                    {
                        Logger.Log($"[NetworkContext] Non administrated IP detected: {updatedDevice.IP}", true, ConsoleColor.Red);
                    }
                }
                else
                {
                    if (IsolateLastIPNumber(updatedDevice.IP) > AppConfig.MaxIPv4AddressWithoutWarning)
                    {
                        Logger.Log($"[NetworkContext] New non administrated IP detected: {updatedDevice.IP}", true, ConsoleColor.Red);
                    }
                    else
                    {
                        Logger.Log($"[NetworkContext] New device detected: {updatedDevice.IP}", true, ConsoleColor.Yellow);
                    }
                   
                    _devices.Add(updatedDevice);
                }
            }
        }

        private static int IsolateLastIPNumber(string ip)
        {
            var parts = ip.Split('.');
            if (parts.Length == 4 && int.TryParse(parts[3], out int lastPart))
            {
                return lastPart;
            }
            return -1; // Invalid IP format
        }

        public static void MarkInactiveDevices(HashSet<string> activeIps)
        {
            lock (_lock)
            {
                foreach (var device in _devices)
                {
                    if (!activeIps.Contains(device.IP))
                    {
                        device.IsOnline = false;
                        device.OS = "";
                        device.Ports = new List<OpenPorts>();
                    }
                }
            }
        }

        public static List<Device> Snapshot()
        {
            lock (_lock)
            {
                return _devices.Select(d => d.Clone()).ToList();
            }
        }
    }
}
