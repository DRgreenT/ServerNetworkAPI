namespace ServerNetworkAPI.dev.Models
{
    public class OpenPorts
    {
        public int Port { get; set; }
        public string ProtocolType { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Port}/{ProtocolType} ({Service})";
        }
    }
}
