using System.Text;

namespace ServerNetworkAPI.dev
{
    public class Output
    {
        public static readonly int pingStatusRow = 21;
        public static readonly int nmapStatusRow = 22;
        public static readonly int totalScanStatusRow = 23;
        public static readonly int nextScanStatusRow = 24;
        static int row = 5;
        public static void Log(string message, bool isDisplayMessage = true)
        {
            if (row >= 18) row = 12;
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            if (isDisplayMessage)
            {
                OutputManager.EditRow(row, "# " + logMessage);
                row++;
            }

            if (File.Exists(Init.logFilePath) && Init.isLogInitialized == false)
            {
                File.Delete(Init.logFilePath);
                Init.isLogInitialized = true;
            }
            File.AppendAllText(Init.logFilePath, logMessage + Environment.NewLine);
        }

        public static void ShowLocalHostAddresses(List<string> localIPs)
        {
            OutputManager.EditRow(row, "# API is now online!");
            row++;
            foreach (string ip in localIPs)
            {
                OutputManager.EditRow(row, $"# ->  http://{ip}:{Init.WebApiPort}/{Init.WebApiName}");
                row++;
            }
            MessageFirewall();
        }
        public static void MessageFirewall()
        {
            OutputManager.EditRow(row, $"# Make sure port {Init.WebApiPort} is not blocked by firewall!");
            row++;
        }
        public static void UpdateProgress(int row, int total, string progressTyp, int current)
        {
            double percent = ((double)current / total * 100);
            int barLength = 30;
            int filledLength = (int)(percent / 100 * barLength);
            string bar = new string('=', filledLength).PadRight(barLength);
            string formattedPercent = percent.ToString("000.0").PadLeft(6);
            OutputManager.EditRow(row, $"# {progressTyp}: [{bar}] {formattedPercent}%");
        }

        public static void UpdateDisplay()
        {
            List<OutputRow> data = OutputManager.GetList().OrderBy(d => d.consoleRow).ToList();
            Console.SetCursorPosition(0, 0);
            StringBuilder sbOutput = new();
            foreach (var row in data)
            {
                sbOutput.Append(row.rowValue).Append("\n");
            }
            Console.WriteLine(sbOutput.ToString());
        }
        public static async Task DisplayLoop(CancellationToken token)
        {
            Console.CursorVisible = false;
            while (!token.IsCancellationRequested)
            {
                Output.UpdateDisplay();
                await Task.Delay(200, token);
            }
            Console.CursorVisible = true;
        }
    }
}
