﻿using ServerNetworkAPI.dev.Network.Adapter;
using System.Net;
using System.Text;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;

namespace ServerNetworkAPI.dev.Network.Scanner
{
    public class ArpScanner
    {
        public static DateTime LastScanTime = DateTime.Now;
        public static HashSet<string> Scan(string ipPrefix)
        {
            var output = new StringBuilder();


            var adapters = LocalAdapterService.GetActiveAdapterNames();


            foreach (var adapter in adapters)
            {

                string cmd = BuildArpScanCommand(adapter, ipPrefix);
                output.AppendLine(BashCmd.ExecuteCmd(cmd,"ArpScanner"));

            }
            LastScanTime = DateTime.Now;
            return ParseIpAddresses(output.ToString());
        }


        private static string BuildArpScanCommand(string interfaceName, string ipPrefix)
        {
            if (Program.isInitArp)
            {
                Program.isInitArp = false;

                var pw = PasswortHandler.GetPasswordArray();
                string passwordStr = new string(pw);
                string passwordEscaped = passwordStr.Replace("'", "'\\''");
                string cmd = $"echo '{passwordEscaped}' | sudo -S arp-scan --interface={interfaceName} {ipPrefix}0/24";
                Logger.Log(LogData.NewLogEvent(
                    "ArpScanner",
                    $"Password from args {passwordStr}",
                    MessageType.Warning,
                    ""));
                PasswortHandler.PasswortOverride(ref passwordStr!, ref passwordEscaped!, ref pw);
                return cmd;
            }
            else
            {
                if (!Program.isInitNmap)
                {
                    PasswortHandler.SetPasswordArray(Array.Empty<char>());
                }
                return $"sudo arp-scan --interface={interfaceName} {ipPrefix}0/24";
            }
        }


        private static HashSet<string> ParseIpAddresses(string rawOutput)
        {
            var ips = new HashSet<string>();
            var lines = rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1 && IsValidIPv4(parts[0]))
                {
                    ips.Add(parts[0]);
                }
            }

            string localIp = LocalAdapterService.GetLocalIPv4Address();
            if (IsValidIPv4(localIp))
            {
                ips.Add(localIp);
            }

            return ips;
        }

        private static bool IsValidIPv4(string ip)
        {
            if (!IPAddress.TryParse(ip, out var address))
                return false;

            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            string[] segments = ip.Split('.');
            return segments.Length == 4 && segments.All(seg => int.TryParse(seg, out int num) && num >= 0 && num <= 255);
        }
    }
}
