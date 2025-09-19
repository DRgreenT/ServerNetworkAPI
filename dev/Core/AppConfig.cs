using System.Net.Sockets;
using System.Net;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;

using ServerNetworkAPI.dev.Services;


namespace ServerNetworkAPI.dev.Core
{
    public class AppConfig
    {
        public static string Version { get; } = "0.2.7c";
        public static string BaseDirectory { get; } = AppContext.BaseDirectory;

        public static string ConfigBasePath { get; } = Path.Combine(BaseDirectory, "Configs");
        public static string LogDirectory { get; } = Path.Combine(BaseDirectory, "Log");
        public static string LogFilePath { get; } = Path.Combine(LogDirectory, "scanlog.txt");
        public static string SaveFilePath { get; } = Path.Combine(BaseDirectory, "devices.json");
        public static string FallbackIpMask { get; private set; } = "192.168.178.";
        public static int ScanIntervalSeconds { get; private set; } = 15;
        public static bool IsNmapEnabled { get; private set; } = false;
        public static int WebApiPort { get; private set; } = 5050;
        public static int MaxIPv4AddressWithoutWarning { get; private set; } = 189; // e.g. 192.168.178.190 to 192.168.178.255 will generate a warning and dicord message

        public static string LocalIpMask { get; private set; } = "";

        public static bool ConsoleUserInterface { get; private set; } = true;

        private static void LoadExternalSettings()
        {
            ConfigManager.NotificationConfig = NoteConfigBuilder.LoadOrCreate();
        }

        public static void InitializeFromArgs(ParsedArgs args)
        {
            var cfg = AppConfigBuilder.LoadOrCreate();

            FallbackIpMask = cfg.FallbackIpMask;
            ScanIntervalSeconds = cfg.ScanIntervalSeconds;
            IsNmapEnabled = cfg.IsNmapEnabled;
            WebApiPort = cfg.WebApiPort;
            MaxIPv4AddressWithoutWarning = cfg.MaxIPv4AddressWithoutWarning;

            if (!string.IsNullOrWhiteSpace(args.FallbackIpMask)) FallbackIpMask = args.FallbackIpMask;
            if (args.TimeoutSeconds > 0) ScanIntervalSeconds = args.TimeoutSeconds;
            if (args.Port > 0) WebApiPort = args.Port;
            if (args.NmapScanActive) IsNmapEnabled = true;
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
                Logger.Log(LogData.NewLogEvent(
                    "AppConfig",
                    $"No valid local IP found. Using fallback IP mask: {FallbackIpMask}",
                    MessageType.Warning,
                    ""
                ));


                return null;
            }

            var parts = firstIp.Split('.');
            if (parts.Length >= 3)
            {
                var mask = $"{parts[0]}.{parts[1]}.{parts[2]}.";

                Logger.Log(LogData.NewLogEvent(
                    "AppConfig",
                    $"Using detected IP mask: {mask}",
                    MessageType.Success,
                    ""
                ));


                return mask;
            }

            Logger.Log(LogData.NewLogEvent(
                "AppConfig",
                $"Invalid IP format: {firstIp}. Using fallback IP mask: {FallbackIpMask}",
                MessageType.Warning,
                ""
            ));

            return null;
        }

        public static string GetStartupParameterSummary()
        {
            return $"# Parameters: nmap={IsNmapEnabled}, delay={ScanIntervalSeconds}s, port={WebApiPort}, fallback IP={FallbackIpMask}";
        }
    }
}
