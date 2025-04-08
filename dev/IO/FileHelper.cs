using ServerNetworkAPI.dev.Core;

namespace ServerNetworkAPI.dev.IO
{
    public class FileHelper
    {
        public static void EnsureDirectoryExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"[FileHelper] Created directory: {folderPath}");
            }
        }

        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void EnsureApplicationDirectories()
        {
            EnsureDirectoryExists(AppConfig.LogDirectory);
            // Weitere Verzeichnisse bei Bedarf ergänzen
        }
    }
}
