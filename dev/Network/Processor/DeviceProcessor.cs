using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using System.Net;

namespace ServerNetworkAPI.dev.Network.Processor
{
    public class DeviceProcessor
    {
        public static async Task ProcessAsync(string ip, int total, Action<int> onProgressUpdate)
        {
            var nmapData = await Scanner.NmapScanner.GetNmapDataAsync(ip);
            string hostname = GetHostname(ip);
            string os = Scanner.NmapScanner.ExtractOS(nmapData);
            var ports = Scanner.NmapScanner.ExtractOpenPorts(nmapData);

            var device = new Device
            {
                IP = ip,
                Hostname = hostname,
                OS = os,
                IsOnline = true,
                LastSeen = DateTime.Now.ToString("dd-MM-yy HH:ss"),
                Ports = ports
            };

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
                LastSeen = DateTime.Now.ToString("dd-MM-yy HH:ss"),
                Ports = new List<OpenPorts>()
            };

            NetworkContext.AddOrUpdateDevice(device);
        }

        private static string GetHostname(string ip)
        {
            try
            {
                return Dns.GetHostEntry(ip).HostName;
            }
            catch
            {
                return "-";
            }
        }
    }
}
