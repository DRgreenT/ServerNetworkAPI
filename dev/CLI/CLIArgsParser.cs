using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.Models;

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
                        result.NmapScanActive = true;
                        break;

                    case "--t":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int timeout))
                        {
                            result.TimeoutSeconds = Math.Clamp(timeout, 1, 3600);
                            i++;
                        }
                        break;

                    case "--p":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int port))
                        {
                            result.Port = port;
                            i++;
                        }
                        break;
                    case "--headless":
                        SystemInfoService.IsHeadlessFromArgs = true;
                        
                        break;
                    case "--pw":
                        if (i + 1 < args.Length)
                        {
                            var value = args[i + 1]?.Trim();
                            if (!string.IsNullOrEmpty(value))
                            {
                                result.Password = value.Trim();
                                i++;
                            }
                        }
                        break;
                }
            }
            SystemInfoService.SetConsoleState();

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
            --headless             Run in headless mode (no console interaction)
            --pw {string}          Sudo Password - not recomended (default: empty)
            ");
        }
    }
}
