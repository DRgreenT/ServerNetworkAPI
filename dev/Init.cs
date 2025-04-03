namespace ServerNetworkAPI.dev
{
    public class Init
    {
        public static readonly string baseDir = AppContext.BaseDirectory;
        public static readonly string logDir = Path.Combine(baseDir, "Log");
        public static readonly string logFilePath = Path.Combine(logDir, "scanlog.txt");
        public static readonly string SaveFilePath = baseDir + "devices.json";
       
        public static List<Device> devices = new();

        // Default start values
        public static string WebApiPort = "5050";
        public static string WebApiName = "network";
        public static int timeOut = 15;
        public static bool isNmapScanActive = true;
        public static string fallbackIpMask = "192.168.178.";

    }
}
