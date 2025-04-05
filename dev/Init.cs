using System.Net;

namespace ServerNetworkAPI.dev
{
    public class Init
    {
        public static readonly string version = "0.1b";

        public static readonly string baseDir = AppContext.BaseDirectory;
        public static readonly string logDir = Path.Combine(baseDir, "Log");
        public static readonly string logFilePath = Path.Combine(logDir, "scanlog.txt");
        public static readonly string SaveFilePath = baseDir + "devices.json";
       
        public static List<Device> devices = new();
        public static OutputManager OutputData = new OutputManager();

        // Default start values
        public static string WebApiPort = "5050";
        public static string WebApiName = "network";
        public static int timeOut = 15;
        public static bool isNmapScanActive = false;
        public static string fallbackIpMask = "192.168.178.";

        public static bool isLogInitialized = false;
        public static bool isInitialScan = true;
        public static string GetLocalNetworkPrefix()
        {
            var localIPs = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                .Select(ip => ip.ToString())
                .ToList();

            var firstIp = localIPs.FirstOrDefault();
            string fallbackIP = Init.fallbackIpMask;
            if (string.IsNullOrEmpty(firstIp))
            {
                Output.Log("Using fallback IP: " + fallbackIP);
                return fallbackIP;
            }

            var parts = firstIp.Split('.');
            if (parts.Length >= 3)
            {
                string str = $"{parts[0]}.{parts[1]}.{parts[2]}.";
                Output.Log("Using IP-mask: " + str);
                return str;
            }
            Output.Log("Using fallback IP: " + fallbackIP);
            return fallbackIP;
        }
    }
}
