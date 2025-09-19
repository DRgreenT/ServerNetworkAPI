using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.UI;

namespace ServerNetworkAPI.dev.IO
{
    public class Logger
    {
        private static readonly Lock _lock = new();
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

                if (!SystemInfoService.IsHeadlessModeFromArgs && alsoConsole)
                {
                    Console.WriteLine(logLine);
                }
            }
        }

        public static void Log(LogData data)
        {
            string logLine = BuildFullLogLine(data);
            if (!SystemInfoService.IsHeadlessModeFromArgs)
            {
                var color = GetColorByType(data.MessageType);
                string displayLine = BuildDisplayLine(logLine, data.TimeStamp!.Length);
                OutputFormatter.PrintMessage(displayLine, color);
            }
            
            Write(logLine, false);
            LogData.AddLog(data);
        }

        private static string BuildFullLogLine(LogData data)
        {
            string message = RemoveNewLineSymbolFromString(data.Message);
            string exceptionInfo = FormatExceptionInfo(data.ExeptionMessage, message);

            return $"[{data.TimeStamp}] [{data.Source}]: {message}{exceptionInfo}";
        }

        private static string FormatExceptionInfo(string? exceptionMessage, string baseMessage)
        {
            if (string.IsNullOrWhiteSpace(exceptionMessage)) return string.Empty;

            string cleaned = RemoveNewLineSymbolFromString(exceptionMessage);
            return cleaned.Contains(baseMessage) ? string.Empty : $" Exception: {cleaned}";
        }

        private static string BuildDisplayLine(string fullLogLine, int timeStampLength)
        {
            int windowWidth = GetConsoleWidth();
            int prefixLength = timeStampLength + 3;
            int maxLength = (windowWidth - 3) > 60 ? windowWidth - 3 : 80;

            if (fullLogLine.Length > prefixLength + maxLength)
            {
                return string.Concat(fullLogLine.AsSpan(prefixLength, maxLength), "...");
            }

            return fullLogLine.Length < prefixLength
                ? fullLogLine
                : fullLogLine[prefixLength..];
        }

        private static int GetConsoleWidth()
        {
            if (SystemInfoService.IsHeadlessModeFromArgs)
                return 0;
            try
            {
                int width = Console.WindowWidth;
                return width < 10 ? 80 : width;
            }
            catch
            {
                return 80;
            }
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
                _ => ConsoleColor.White
            };
        }

        public static string RemoveNewLineSymbolFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            ReadOnlySpan<char> span = input.AsSpan();

            for (int i = 0; i < span.Length; i++)
            {
                if (span.Length == 0)
                    break;
                if (span[i] == '\r' || span[i] == '\n')
                    return span[.. i].ToString();
            }

            return input;
        }
    }
}
