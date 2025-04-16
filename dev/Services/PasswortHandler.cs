using System.Diagnostics;

namespace ServerNetworkAPI.dev.Services
{
    public class PasswortHandler
    {

        public static string PasswordOverrided()
        {
            return null!;
        }
        public static string PasswordInput()
        {   string password = string.Empty;
            int tries = 0;
            do
            {
                password = GetPassword();
                tries++;
            }
            while (!IsValidPassword(password) && tries < 4);
            if(tries >= 3)
            {
                Environment.Exit(0);
                return password = string.Empty;
            }
            return password;
        }

        private static string GetPassword()
        {
            string password = string.Empty;
            Console.Write("Enter your sudo password: ");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }
        private static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            else
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"echo '{password.Replace("'", "'\\''")}' | sudo -S -v\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                string error = process!.StandardError.ReadToEnd();
                process.WaitForExit();

                string[] errors = {"Sorry, try again.",
                    "incorrect"
                };

                bool isError = false;
                foreach (string err in errors)
                {
                    if (error.Contains(err))
                    {
                        isError = true;
                        break;
                    }
                }

                return process.ExitCode == 0 && !isError;
            }
        }
    }
}
