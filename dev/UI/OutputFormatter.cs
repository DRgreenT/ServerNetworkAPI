using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Network.Adapter;

namespace ServerNetworkAPI.dev.UI
{
    public class OutputFormatter
    {
        private static readonly int[] MessageRows = Enumerable.Range(0, 20).ToArray(); // Zeilen 0–19 für Nachrichten
        private static int _messageIndex = 0;

        private static string localIp = LocalAdapterService.GetLocalIPv4Address(); // deine Utility aus AdapterService
        public static void PrintStartupInfo()
        {
            PrintMessage($"Server NetworkAPI v{AppConfig.Version} started.", ConsoleColor.Green, false);
            PrintMessage($"Port: {AppConfig.WebApiPort}, Controller: /{AppConfig.WebApiControllerName}", ConsoleColor.Yellow,false);
            PrintMessage($"IP-Mask: {AppConfig.LocalIpMask}, Nmap enabled: {AppConfig.IsNmapEnabled}, Scan Interval: {AppConfig.ScanIntervalSeconds}s",ConsoleColor.Yellow,false);
            PrintMessage($"API running at: http://{localIp}:{AppConfig.WebApiPort}/{AppConfig.WebApiControllerName}", ConsoleColor.Yellow, false);
            PrintMessage($"API running at: http://127.0.0.1:{AppConfig.WebApiPort}/{AppConfig.WebApiControllerName}", ConsoleColor.Yellow, false);
        }

        public static void PrintMessage(string message, ConsoleColor? color = null, bool hasTimeInfo = true)
        {
            string timestamp = hasTimeInfo ? $"[{DateTime.Now:HH:mm:ss}]" : "";
            int row = MessageRows[_messageIndex % MessageRows.Length];
            if (row < 6 && hasTimeInfo) row = 6;
            if (row == MessageRows.Length - 1)
            {
                ClearMessageArea();
                row = 6; 
            }
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            string spacer = hasTimeInfo ? " " : "";
            OutputLayout.UpdateRow(row, $"# {timestamp}{spacer}{message}");

            if (color.HasValue)
            {
                Console.ResetColor();
            }

            _messageIndex++;
        }

        private static void ClearMessageArea()
        {
            for (int i = 6; i < MessageRows.Length; i++)
            {
                OutputLayout.UpdateRow(i, "#");
            }
        }

        public static void PrintDeviceSummary()
        {
            var devices = Core.NetworkContext.GetDevices();
            Console.SetCursorPosition(0, 28);
            Console.WriteLine($"Total devices: {devices.Count}");
            Console.WriteLine("IP".PadRight(16) + "| " + "Hostname".PadRight(24) + "| " + "OS".PadRight(10) + "| Status");
            Console.WriteLine(new string('-', 65));

            foreach (var dev in devices)
            {
                string status = dev.IsOnline ? "Online" : "Offline";

                string ip = dev.IP.PadRight(16);             
                string hostname = dev.Hostname.PadRight(25); 
                if(hostname.Length > 24) hostname = hostname.Substring(0, 21) + "...";
                string os = dev.OS.PadRight(10);
                string line = $"{ip}| {hostname}| {os}| {status}";

                Console.WriteLine(line);
            }
        }

        public static void PrintDivider()
        {
            int row = MessageRows[_messageIndex % MessageRows.Length];
            OutputLayout.UpdateRow(row, new string('-', 60));
            _messageIndex++;
        }

        public static void PrintProgressBar(int current, int total, string label = "Progress")
        {
            double percent = (double)current / total;
            int barWidth = 30;
            int filled = (int)(percent * barWidth);
            string bar = new string('=', filled).PadRight(barWidth);

            int row = MessageRows[_messageIndex % MessageRows.Length];
            OutputLayout.UpdateRow(row, $"# {label}: [{bar}] {percent * 100:000.0}%");
            _messageIndex++;
        }
    }
}
