using System.Net.Sockets;
using System.Net;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;

namespace ServerNetworkAPI.dev.Core
{
    public class AppConfig
    {
        public static string Version { get; } = "0.2b";
        public static string BaseDirectory { get; } = AppContext.BaseDirectory;

        public static string ConfigBasePath = Path.Combine(BaseDirectory, "Configs");
        public static string LogDirectory => Path.Combine(BaseDirectory, "Log");
        public static string LogFilePath => Path.Combine(LogDirectory, "scanlog.txt");
        public static string SaveFilePath => Path.Combine(BaseDirectory, "devices.json");

        public static string FallbackIpMask { get; private set; } = "192.168.178.";
        public static int ScanIntervalSeconds { get; private set; } = 15;
        public static bool IsNmapEnabled { get; private set; } = false;
        public static int WebApiPort { get; private set; } = 5050;
        public static string WebApiControllerName { get; private set; } = "network";

        public static int MaxIPv4AddressWithoutWarning = 190; // Set to value your DHCP is set for new devices

        public static string LocalIpMask { get; private set; } = "";

        private static void LoadExternalSettings()
        {
            ConfigManager.NotificationConfig = NoteConfigBuilder.LoadOrCreate();

        }
        public static void InitializeFromArgs(CLI.ParsedArgs args)
        {
            FallbackIpMask = args.FallbackIpMask;
            ScanIntervalSeconds = args.TimeoutSeconds;
            IsNmapEnabled = args.NmapScanActive;
            WebApiPort = args.Port;
            WebApiControllerName = args.ControllerName;

            LocalIpMask = CalculateLocalNetworkPrefix() ?? FallbackIpMask;
            LoadExternalSettings();
        }

        private static string? CalculateLocalNetworkPrefix()
        {
            var localIPs = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip))
                .Select(ip => ip.ToString())
                .ToList();

            var firstIp = localIPs.FirstOrDefault();
            if (string.IsNullOrEmpty(firstIp))
            {
                Logger.Log($"[AppConfig] Using fallback IP mask: {FallbackIpMask}", false);
                return null;
            }

            var parts = firstIp.Split('.');
            if (parts.Length >= 3)
            {
                var mask = $"{parts[0]}.{parts[1]}.{parts[2]}.";
                Logger.Log($"[AppConfig] Using detected IP mask: {mask}",true);
                return mask;
            }

            Logger.Log($"[AppConfig] Invalid IP format. Using fallback: {FallbackIpMask}",false);
            return null;
        }

        public static string GetStartupParameterSummary()
        {
            return $"# Parameters: nmap={IsNmapEnabled}, delay={ScanIntervalSeconds}s, port={WebApiPort}, controller={WebApiControllerName}, fallback IP={FallbackIpMask}";
        }
    }
}
