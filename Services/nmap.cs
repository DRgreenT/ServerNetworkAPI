public class Nmap{

    
        public static List<string> GetNmaps(string nmapInfo)
        {
            var info = new List<string>();

            if (string.IsNullOrWhiteSpace(nmapInfo) || nmapInfo.Contains("Note: Host seems down"))
                return info;

            var lines = nmapInfo.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("Running:") ||
                    trimmed.StartsWith("OS details:") ||
                    trimmed.StartsWith("MAC Address:") ||
                    trimmed.StartsWith("Aggressive OS guesses:") ||
                    trimmed.Contains("open"))
                {
                    info.Add(trimmed);
                }
            }

            return info;
        }

        public static async Task<string> GetNmapInfo(string ip)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo nmap -O -Pn {ip}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                string output = await process!.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return output;
            }
            catch
            {
                return "";
            }
        }
}