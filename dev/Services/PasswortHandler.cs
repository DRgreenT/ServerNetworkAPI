﻿using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using ServerNetworkAPI.dev.IO;
using System.Diagnostics;

namespace ServerNetworkAPI.dev.Services
{
    public class PasswortHandler
    {
        private static char[] Password = Array.Empty<char>();

        public static char[] GetPasswordArray()
        {
            return Password;
        }

        public static void SetPasswordArray(char[] password)
        {
            Password = password;
        }

        public static void PasswortOverride(ref string? s, ref string? s2, ref char[] pw)
        {
            if (s != null)
                s = string.Empty;
            if (s2 != null)
                s2 = string.Empty;

            Array.Clear(pw, 0, pw.Length);
            pw = Array.Empty<char>();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static char[] PasswordInput(ParsedArgs args)
        {
            if (BashCmd.IsRunningAsRoot() || SystemInfoService.IsConsoleInactive)
            {
                Program.isInitArp = false;
                Program.isInitNmap = false;
                return Array.Empty<char>();
            }

            char[] password;

            if (!string.IsNullOrEmpty(args.Password))
            {
                password = args.Password.Trim().ToCharArray();
                Logger.Log(LogData.NewLogEvent(
                    "PasswortHandler",
                    $"Password from args {args.Password}",
                    MessageType.Warning,
                    ""));
            }
            else
            {
                int tries = 0;
                do
                {
                    password = GetPassword();
                    tries++;
                }
                while (!IsValidSudoPassword(password) && tries < 4);

                if (tries >= 3)
                {
                    Array.Clear(password, 0, password.Length);
                    Environment.Exit(0);
                }
            }
            return password;

        }

        public static bool IsValidSudoPassword(char[] password)
        {
            if (password.Length == 0)
                return false;

            string passwordStr = new string(password);
            string escaped = passwordStr.Replace("'", "'\\''");

            var command = $"echo '{escaped}' | sudo -S -v";

            try
            {
                using var process = Process.Start(BashCmd.CreateBashProcessStartInfo(command));
                string error = process!.StandardError.ReadToEnd();
                process.WaitForExit();

                string[] knownErrors = { "Sorry, try again.", "incorrect" };
                bool isError = knownErrors.Any(err => error.Contains(err));

                return process.ExitCode == 0 && !isError;
            }
            finally
            {
                PasswortOverride(ref passwordStr!, ref escaped!, ref password);
            }
        }

        private static char[] GetPassword()
        {
            var chars = new List<char>();
            Console.Write("Enter your sudo password: ");

            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (chars.Count > 0)
                    {
                        chars.RemoveAt(chars.Count - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    chars.Add(key.KeyChar);
                    Console.Write("*");
                }
            }

            return chars.ToArray();
        }
    }
}
