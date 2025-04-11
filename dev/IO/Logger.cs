using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.UI;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;

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
                
                string logLine = message;

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
        public static void Log(LogData data)
        {
            var color = GetColorByType(data.MessageType);

            
            string exceptionInfo = string.IsNullOrWhiteSpace(data.ExeptionMessage)
                ? string.Empty
                : $" Exception: {RemoveNewLineSymbolFromString(data.ExeptionMessage)}";

            string message = RemoveNewLineSymbolFromString(data.Message);
            if (!string.IsNullOrEmpty(exceptionInfo) && exceptionInfo.Contains(message))
                exceptionInfo = string.Empty;

            string logLine = $"[{data.TimeStamp}] [{data.Source}]: {message}{exceptionInfo}";

            string displayLine;
            int prefixLength = data.TimeStamp!.Length + 3; 
            int maxLength = Console.WindowWidth - 3;
            if (logLine.Length > prefixLength + maxLength)
            {
                displayLine = logLine.Substring(prefixLength, maxLength) + "...";
            }
            else
            {
                displayLine = logLine.Substring(prefixLength);
            }

            OutputFormatter.PrintMessage(displayLine, color);
            Write(logLine, false);
        }


        private static ConsoleColor GetColorByType(MessageType type)
        {
            return type switch
            {
                MessageType.Error or MessageType.Exception => ConsoleColor.Red,
                MessageType.HardWarning => ConsoleColor.DarkYellow,
                MessageType.Warning => ConsoleColor.Yellow,
                MessageType.Success => ConsoleColor.Green,
                MessageType.Standard => ConsoleColor.White,
                _ => ConsoleColor.Gray
            };
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
