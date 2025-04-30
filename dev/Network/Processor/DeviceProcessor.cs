using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.IO;
using System.Net;
using System.Net.Sockets;

namespace ServerNetworkAPI.dev.Network.Processor
{
    public class DeviceProcessor
    {
        public static async Task ProcessAsync(string ip, Action<int> onProgressUpdate)
        {
            var nmapData = await Scanner.NmapScanner.GetNmapDataAsync(ip);
            string hostname = await GetHostnameAsync(ip);

            var device = new Device
            {
                IP = ip,
                Hostname = hostname,
                OS = nmapData.OS,
                MacAddress = nmapData.MACAddress,
                HopDistance = nmapData.NetworkDistance,
                IsOnline = true,
                LastSeen = DateTime.Now.ToString("dd-MM-yy HH:mm"),
                Ports = nmapData.OpenPorts,
            };
            Logger.Log(LogData.NewLogEvent(
                "DeviceProcessor",
                $"{device.IP} : {device.MacAddress} - name?: " + (device.Hostname == "" ? false : true)+  $" - ports: {device.Ports.Count} - dist: {device.HopDistance} -  OS?: " + (device.OS == "unknown" ? false : true),
                Models.Enums.MessageType.Standard,
                ""));

            NetworkContext.AddOrUpdateDevice(device);

            onProgressUpdate?.Invoke(1); 
        }

        public static void FallbackAdd(string ip)
        {
            var device = new Device
            {
                IP = ip,
                Hostname = "-",
                OS = "",
                IsOnline = true,
                LastSeen = DateTime.Now.ToString("dd-MM-yy HH:mm"),
                Ports = new List<OpenPorts>()
            };

            NetworkContext.AddOrUpdateDevice(device);
        }

        private static async Task<string> GetHostnameAsync(string ip)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Dns.GetHostEntry(ip).HostName;
                }
                catch(SocketException ex)
                {
                    Logger.Log(LogData.NewLogEvent(
                        "DeviceProcessor",
                        $"Fehler beim Abrufen des Hostnamens für {ip}: {ex.Message}",
                        Models.Enums.MessageType.Exception,
                        ""));
                    return "-";
                }
            });
        }
    }
}
