using ServerNetworkAPI.dev.IO;

namespace ServerNetworkAPI.dev.Services
{
    public class PasswortHandler
    {

        public static string PasswordOverrided()
        {
            return null!;
        }
        public static string PasswordInput()
        {
            if (!BashCmd.IsRunningAsRoot())
            {
                string password = string.Empty;
                int tries = 0;
                do
                {
                    password = GetPassword();
                    tries++;
                }
                while (!BashCmd.IsValidSudoPassword(password) && tries < 4);
                if (tries >= 3)
                {
                    Environment.Exit(0);
                    return password = string.Empty;
                }
                return password;
            }
            else
            {
                Program.isInitArp = false;
                Program.isInitNmap = false;
                return string.Empty;
            }
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
       
    }
}
