using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace ServerNetworkAPI.dev.Network.Adapter
{
    public class LocalAdapterService
    {
        public static HashSet<string> GetActiveAdapterNames()
        {
            var activeAdapters = new HashSet<string>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    !ni.Description.ToLower().Contains("virtual"))
                {
                    activeAdapters.Add(ni.Name);
                }
            }

            return activeAdapters;
        }

        public static string GetLocalMacAdress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var macAddr = ni.GetPhysicalAddress().ToString();
                    if (macAddr.Length == 0)
                        continue;
                    if (macAddr.Contains(':'))return macAddr;

                    string _macAddr = "";
                    for (int i = 0; i < macAddr.Length-1;i+=2)
                    {
                        _macAddr += macAddr.Substring(i, 2) + ":";
                    }
                    return _macAddr;
                }
            }
            return "?";
        }

        public static string GetLocalIPv4Address()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProps = ni.GetIPProperties();

                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(ua.Address))
                    {
                        return ua.Address.ToString();
                    }
                }
            }

            return "127.0.0.1";
        }
    }
}
