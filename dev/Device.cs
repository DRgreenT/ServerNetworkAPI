using System.Text.Json.Serialization;
namespace ServerNetworkAPI.dev
{
    public class Device
    {

        public string IP { get; set; } = "";
        public string Hostname { get; set; } = "";
        public string OS { get; set; } = "";
        public bool IsOnline { get; set; } = false;

        public List<OpenPorts> Ports { get; set; } = new List<OpenPorts>();

        [JsonIgnore]
        public int index { get; set; }
    }

    public class OpenPorts
    {        
        public int port { get; set; }
        public string protocolType { get; set; } = "";
        public string service { get; set; } = "";
    }
}