using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ServerNetworkAPI.dev
{
    public class localNetworkAdapter
    {
        public static List<string> activeAdapters = new List<string>();
        public static List<string> GetActiveAdapters()
        {
            activeAdapters.Clear();

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.GetIPProperties().UnicastAddresses
                        .Any(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                );

            foreach (var nic in adapters)
            {
                var ip = nic.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString() ?? "Keine IPv4";
                activeAdapters.Add(ip);
            }

            if (!adapters.Any())
            {
                Output.Log("Keine aktiven Netzwerkadapter mit IPv4 gefunden.");
                return activeAdapters!;
            }
            return activeAdapters;

        }
    }
}
