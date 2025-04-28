using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.Models.Enums;

namespace ServerNetworkAPI.dev.Core
{
    public class NetworkContext
    {
        private static readonly Lock _lock = new();
        private static readonly List<Device> _devices = [];

        public static IReadOnlyList<Device> GetActiveDevices()
        {
            return GetDevices().Where(d => d.IsOnline).ToList();
        }
        public static IReadOnlyList<Device> GetDevices()
        {
            lock (_lock)
            {
                return [.. _devices
                    .Select(d => d.CloneWithIndex())
                    .OrderBy(d => d.Index)];
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
                LogData log = new();
                var existing = _devices.FirstOrDefault(d => d.IP == updatedDevice.IP);
                if (existing != null)
                {
                    existing.Hostname = updatedDevice.Hostname;
                    existing.OS = updatedDevice.OS;
                    existing.IsOnline = updatedDevice.IsOnline;
                    existing.LastSeen = updatedDevice.LastSeen;
                    existing.Ports = updatedDevice.Ports;

                    if(IsolateLastIPNumber(updatedDevice.IP) > AppConfig.MaxIPv4AddressWithoutWarning)
                    {
                        log = LogData.NewLogEvent(
                            "NetworkContext",
                            $"Non administrated IP detected: {updatedDevice.IP}",
                            MessageType.HardWarning
                        );
                        Logger.Log(log);
                    }
                }
                else
                {
                    if (IsolateLastIPNumber(updatedDevice.IP) > AppConfig.MaxIPv4AddressWithoutWarning)
                    {
                        log = LogData.NewLogEvent(
                            "NetworkContext",
                            $"Non administrated IP detected: {updatedDevice.IP}",
                            MessageType.HardWarning
                        );

                        _ = NotificationService.SendDeviceNotificationAsync(updatedDevice);
                    }
                    else
                    {
                        log = LogData.NewLogEvent(
                            "NetworkContext",
                            $"New device detected: {updatedDevice.IP}",
                            MessageType.Warning
                        );
                    }
                    Logger.Log(log);
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
            return -1; 
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
                        device.Ports = [];
                    }
                }
            }
        }

        public static List<Device> Snapshot()
        {
            lock (_lock)
            {
                return [.. _devices.Select(d => d.Clone())];
            }
        }
    }
}
