using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Network.Adapter;
using ServerNetworkAPI.dev.WebAPI;

namespace ServerNetworkAPI.dev.UI
{
    public class OutputFormatter
    {
        private static string localIp = LocalAdapterService.GetLocalIPv4Address();
        public static void PrintStartupInfo()
        {
            Console.SetCursorPosition(0, 0);
            PrintMessage($"Server NetworkAPI v{AppConfig.Version} started.", ConsoleColor.Green, false,0);
            PrintMessage($"", null, false,1);
            PrintMessage($"IP-Mask: {AppConfig.LocalIpMask}, Nmap enabled: {AppConfig.IsNmapEnabled}, Scan Interval: {AppConfig.ScanIntervalSeconds}s", ConsoleColor.Yellow, false,2);
            PrintMessage($"API running at: http://{localIp}:{AppConfig.WebApiPort}/{ControllerAPI.NetworkControllerName}", ConsoleColor.Yellow, false,3);
            PrintMessage($"API running at: http://127.0.0.1:{AppConfig.WebApiPort}/{ControllerAPI.NetworkControllerName}", ConsoleColor.Yellow, false,4);
        }

        private static string TimeStamp(bool hasTimeInfo)
        {
            return hasTimeInfo ? $"[{DateTime.Now:HH:mm:ss}] " : "";
        }

        public static void PrintMessage(string message, ConsoleColor? color = null, bool hasTimeInfo = true, int? preferredRow = null)
        {
            if (!AppConfig.ConsoleUserInterface)
                return;

            string timestamp = TimeStamp(hasTimeInfo);
            int row;

            if (preferredRow.HasValue)
            {
                row = preferredRow.Value;
            }
            else
            {
                row = 23;
            }

            message = message.Length >= Console.WindowWidth - timestamp.Length - 7 
                ? message.Substring(0, Console.WindowWidth - timestamp.Length - 7) + "..." 
                : message;
            OutputLayout.UpdateRow(row, $"# {timestamp}{message}", color);

        }
        public static void PrintDeviceSummary()
        {
            if (!AppConfig.ConsoleUserInterface)
            {
                return;
            }
            var devices = NetworkContext.GetDevices();
            Console.SetCursorPosition(0, 30);
            Console.WriteLine();
            Console.WriteLine(new string('#', 20) + "< SUMMARY >" + new string('#', 20));
            Console.WriteLine($"Total devices: {devices.Count}");
            Console.WriteLine("IP".PadRight(16) + "| " + "Hostname".PadRight(24) + "| " + "OS".PadRight(10) + "| Status");
            Console.WriteLine(new string('-', 65));

            foreach (var dev in devices)
            {
                string status = dev.IsOnline ? "Online" : "Offline";

                string ip = dev.IP.PadRight(16);
                string hostname = dev.Hostname.PadRight(25);
                if (hostname.Length > 24) hostname = hostname.Substring(0, 21) + "...";
                string os = dev.OS.PadRight(10);
                string line = $"{ip}| {hostname}| {os}| {status}";

                Console.WriteLine(line);
            }
        }

    }
}
