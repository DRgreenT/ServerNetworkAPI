namespace NetworkAPI.Outputs
{   
    public class Output
    {
        public static readonly string baseDir = AppContext.BaseDirectory;
        private static readonly string logDir = Path.Combine(baseDir,"Log");
        private static readonly string logFilePath = Path.Combine(logDir,"scanlog.txt");

        private static bool isLogInitialized = false;
        public static void Log(string message, bool isDisplayMessage = true)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            if (isDisplayMessage)
                Console.WriteLine(logMessage);

            Directory.CreateDirectory(logDir);

            if (File.Exists(logFilePath) && isLogInitialized == false)
            {
                File.Delete(logFilePath);
                isLogInitialized = true;
            }

            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }

        public static void OverrideConsoleLine(int lineNumber)
        {
            Console.SetCursorPosition(0, lineNumber);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, lineNumber);           
        }
    }
}