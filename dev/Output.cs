namespace ServerNetworkAPI.dev
{    
    public class Output
    {


        private static bool isLogInitialized = false;
        public static void Log(string message, bool isDisplayMessage = true)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            if (isDisplayMessage)
                Console.WriteLine(logMessage);

            Directory.CreateDirectory(Init.logDir);

            if (File.Exists(Init.logFilePath) && isLogInitialized == false)
            {
                File.Delete(Init.logFilePath);
                isLogInitialized = true;
            }

            File.AppendAllText(Init.logFilePath, logMessage + Environment.NewLine);
        }

        public static void OverrideConsoleLine(int lineNumber)
        {
            Console.SetCursorPosition(0, lineNumber);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, lineNumber);           
        }
    }
}