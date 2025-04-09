namespace ServerNetworkAPI.dev.Models
{
    public class AppConfigData
    {
        public string FallbackIpMask { get; set; } = "192.168.178.";
        public int ScanIntervalSeconds { get; set; } = 15;
        public bool IsNmapEnabled { get; set; } = false;
        public int WebApiPort { get; set; } = 5050;
        public string WebApiControllerName { get; set; } = "network";
        public int MaxIPv4AddressWithoutWarning { get; set; } = 190;
    }
}
