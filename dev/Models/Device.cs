using System.Text.Json.Serialization;

namespace ServerNetworkAPI.dev.Models
{
    public class Device
    {
        public string IP { get; set; } = string.Empty;
        public string Hostname { get; set; } = "-";
        public string OS { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public string LastSeen { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string HopDistance { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
        public bool isNmapScanned { get; set; } = false;
        public List<OpenPorts> Ports { get; set; } = new();


        [JsonIgnore]
        public int Index { get; set; }

        public Device Clone()
        {
            return new Device
            {
                IP = IP,
                Hostname = Hostname,
                OS = OS,
                IsOnline = IsOnline,
                LastSeen = LastSeen,
                MacAddress = MacAddress,
                HopDistance = HopDistance,
                Uptime = Uptime,
                isNmapScanned = isNmapScanned,

                Ports = new List<OpenPorts>(Ports),
            };
        }

        public Device CloneWithIndex()
        {
            var clone = Clone();
            if (int.TryParse(clone.IP[(clone.IP.LastIndexOf('.') + 1)..], out var idx))
                clone.Index = idx;
            else
                clone.Index = 0;

            return clone;
        }

    }
}