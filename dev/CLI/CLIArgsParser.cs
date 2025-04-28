using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Services;

namespace ServerNetworkAPI.dev.CLI
{
    public class CLIArgsParser
    {
        public static ParsedArgs Parse(string[] args)
        {
            var result = new ParsedArgs();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--help":
                    case "-h":
                    case "-help":
                        result.ShowHelp = true;
                        return result;

                    case "--fip":
                        if (i + 1 < args.Length)
                        {
                            var value = args[i + 1]?.Trim();
                            if (!string.IsNullOrEmpty(value))
                            {
                                result.FallbackIpMask = value;
                                i++;
                            }
                        }
                        break;

                    case "--nmap":
                    case "-nm":
                        result.NmapScanActive = true;
                        break;

                    case "--t":
                    case "-t":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int timeout))
                        {
                            result.TimeoutSeconds = Math.Clamp(timeout, 1, 3600);
                            i++;
                        }
                        break;

                    case "--p":
                    case "-p":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int port))
                        {
                            result.Port = port;
                            i++;
                        }
                        break;
                    case "--headless":
                        SystemInfoService.IsHeadlessFromArgs = true;
                        break;
                }
            }

            return result;
        }

        public static void PrintHelp()
        {
            Console.WriteLine(@"
            Available parameters:
            --help/-help           Shows this help message
            --t {int}              Delay between scans in seconds (1–3600)
            --p {int}              Web API port (default: 5050)
            --nmap                 Enable nmap scan mode
            --fip {string}         Fallback IP mask (e.g., 192.168.178.)
            ");
        }
    }

    public class ParsedArgs
    {
        public bool ShowHelp { get; set; } = false;
        public bool NmapScanActive { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 15;
        public int Port { get; set; } = 5050;
        public string FallbackIpMask { get; set; } = "192.168.178.";
    }
}
