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
                    LogError(modul, $"stderr: {error.Trim()}", MessageType.Warning);
                }

                return output.Trim();
            }
            catch (Exception ex)
            {
                LogError(modul, $"Error executing command: {ex.Message}", MessageType.Exception);
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
                    LogError(modul, $"stderr: {error.Trim()}", MessageType.Warning);
                }

                return output.Trim();
            }
            catch (Exception ex)
            {
                LogError(modul, $"Error executing command: {ex.Message}", MessageType.Exception);
                return string.Empty;
            }
        }

        public static bool IsValidSudoPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var command = $"echo '{password.Replace("'", "'\\''")}' | sudo -S -v";
            using var process = Process.Start(CreateBashProcessStartInfo(command));

            string error = process!.StandardError.ReadToEnd();
            process.WaitForExit();

            string[] knownErrors = { "Sorry, try again.", "incorrect" };
            bool isError = knownErrors.Any(err => error.Contains(err));

            return process.ExitCode == 0 && !isError;
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
                return false;
            }
        }


        private static ProcessStartInfo CreateBashProcessStartInfo(string command)
        {
            return new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        private static void LogError(string modul, string message, MessageType type)
        {
            Logger.Log(LogData.NewData(
                $"BashCmd [{modul}]",
                message,
                type,
                ""));
        }

        private static void TryKillProcess(Process process, string modul)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                LogError(modul, "Process timeout, killed.", MessageType.Warning);
            }
            catch (Exception killEx)
            {
                LogError(modul, $"Error killing process: {killEx.Message}", MessageType.Exception);
            }
        }
    }
}
