﻿namespace ServerNetworkAPI.dev.Models
{
    public class ParsedArgs
    {
        public bool ShowHelp { get; set; } = false;
        public bool NmapScanActive { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 15;
        public int Port { get; set; } = 5050;
        public string FallbackIpMask { get; set; } = "192.168.178.";
        public bool HeadlessMode { get; set; } = false;
        public string Password { get; set; } = string.Empty;
    }
}
