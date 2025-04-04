namespace ServerNetworkAPI.dev
{ 
    public class CLIArgsParser
    {
        public const string HelpText = 
        @"
        Available parameters:
        --help/-help           Shows this help message
        --t {int}              Delay between loops in seconds (default: 5 min: 1 max:3600)
        --p {int}              Web API port (default: 5050)
        --c {string}           Controller name (default: 'network')
        --nmap                 Set nmap scan active (default ping only)
        --fip {string}         Fallback IP mask 192.168.178. 
        ";

        public static void ShowHelp()
        {
            Console.WriteLine(HelpText);
        }
        public static ParsedArgs GetArgs(string[] args)
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
                        if(i + 1 < args.Length)
                        {
                            var subStr = args[i + 1]?.Trim();
                            if(String.IsNullOrEmpty(subStr))
                            {
                                i++;
                                Console.WriteLine("--fip needs a value!");
                                break;
                            }
                            result.FallbackIpMask = subStr;
                            i++;
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
                            result.Port = port.ToString();
                            i++;
                        }
                        break;

                    case "--c":
                    case "-c":
                        if (i + 1 < args.Length)
                        {
                            result.ControllerName = args[i + 1];
                            i++;
                        }
                        break;
                }
            }

            return result;
        }
    }
    public class ParsedArgs
    {
        public bool ShowHelp { get; set; } = false;
        public bool NmapScanActive { get; set; } = Init.isNmapScanActive;
        public int TimeoutSeconds { get; set; } = Init.timeOut;
        public string  Port { get; set; } = Init.WebApiPort;
        public string ControllerName { get; set; } = Init.WebApiName;
        public string FallbackIpMask { get; set; } = Init.fallbackIpMask;
    }
}
