namespace ServerNetworkAPI.dev.Models
{
    public class NmapData
    {
        public string OS { get; set; } = string.Empty;
        public string MACAddress { get; set; } = string.Empty;
        public string NetworkDistance { get; set; } = string.Empty;
        public List<OpenPorts> OpenPorts { get; set; } = new();

    }
}
