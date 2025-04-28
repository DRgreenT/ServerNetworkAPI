using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using System.Diagnostics;

namespace ServerNetworkAPI.dev.IO
{
    public static class BashCmd
    {
        public static string ExecuteCmd(string command, string modul)
        {
            try
            {
                using var process = Process.Start(CreateBashProcessStartInfo(command));
                string output = process!.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    LogData.NewLogEvent(modul, $"stderr: {error.Trim()}", MessageType.Warning);
                }

                return output.Trim();
            }
            catch (Exception ex)
            {
                LogData.NewLogEvent(modul, $"Error executing command: {ex.Message}", MessageType.Exception);
                return string.Empty;
            }
        }

        public static async Task<string> ExecuteCmdAsync(string command, string modul, int timeoutSeconds = 15)
        {
            try
            {
                using var process = new Process { StartInfo = CreateBashProcessStartInfo(command) };
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                var waitTask = process.WaitForExitAsync(cts.Token);

                var completed = await Task.WhenAny(waitTask, Task.Delay(-1, cts.Token));
                if (completed != waitTask)
                {
                    TryKillProcess(process, modul);
                    return string.Empty;
                }

                string output = await outputTask;
                string error = await errorTask;

                if (!string.IsNullOrWhiteSpace(error))
                {
                    LogData.NewLogEvent(modul, $"{error.Trim()}", MessageType.Warning);
                }

                return output.Trim();
            }
            catch (Exception ex)
            {
                LogData.NewLogEvent(modul, $"Error executing command: {ex.Message}", MessageType.Exception);
                return string.Empty;
            }
        }

        public static bool IsRunningAsRoot()
        {
            try
            {
                using var process = Process.Start(CreateBashProcessStartInfo("id -u"));
                string output = process!.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return output == "0";
            }
            catch
            {
                LogData.NewLogEvent("BashCmd", "Error checking if running as root.", MessageType.Exception);
                return false;
            }
        }


        public static ProcessStartInfo CreateBashProcessStartInfo(string command)
        {
            return new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = "/tmp"
            };
        }

        private static void TryKillProcess(Process process, string modul)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                LogData.NewLogEvent(modul, "Process timeout, killed.", MessageType.Warning);
            }
            catch (Exception killEx)
            {
                LogData.NewLogEvent(modul, $"Error killing process: {killEx.Message}", MessageType.Exception);
            }
        }
    }
}
