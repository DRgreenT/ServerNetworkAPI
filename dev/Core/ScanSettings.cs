namespace ServerNetworkAPI.dev.Core
{
    public class ScanSettings
    {
        public int IntervalSeconds { get; init; } = 15;
        public bool UseNmap { get; init; } = false;
        public string FallbackIpMask { get; init; } = "192.168.178.";

        public string ControllerName { get; init; } = "network";
        public int ApiPort { get; init; } = 5050;

        public static ScanSettings FromAppConfig()
        {
            return new ScanSettings
            {
                IntervalSeconds = AppConfig.ScanIntervalSeconds,
                UseNmap = AppConfig.IsNmapEnabled,
                FallbackIpMask = AppConfig.FallbackIpMask,
                ControllerName = AppConfig.WebApiControllerName,
                ApiPort = AppConfig.WebApiPort
            };
        }

        public override string ToString()
        {
            return $"ScanInterval: {IntervalSeconds}s | Nmap: {UseNmap} | Fallback IP: {FallbackIpMask} | Port: {ApiPort} | Controller: {ControllerName}";
        }
    }
}
