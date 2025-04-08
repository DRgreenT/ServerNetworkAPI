using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.UI;

namespace ServerNetworkAPI.dev.IO
{
    public class Logger
    {
        private static readonly object _lock = new();
        private static bool _logInitialized = false;

        public static void Write(string message, bool alsoConsole = true)
        {
            lock (_lock)
            {
                string timestamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ";
                string logLine = timestamp + message;

                if (!_logInitialized)
                {
                    if (File.Exists(AppConfig.LogFilePath))
                        File.Delete(AppConfig.LogFilePath);

                    _logInitialized = true;
                }

                File.AppendAllText(AppConfig.LogFilePath, logLine + Environment.NewLine);

                if (alsoConsole)
                {
                    Console.WriteLine(logLine);
                }
            }
        }
        public static void Log(string message, bool succsess, ConsoleColor? color = null)
        {
            if(color == null)
            {
                color = succsess ? ConsoleColor.Green : ConsoleColor.Red;
            }                
            OutputFormatter.PrintMessage(message, color);
            Write(message,false);
        }

        public static string RemoveNewLineSymbolFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int index = input.IndexOf('\n');
            return index >= 0 ? input.Substring(0, index) : input;
        }

    }
}
