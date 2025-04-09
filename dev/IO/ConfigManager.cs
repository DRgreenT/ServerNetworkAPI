using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using System.Text.Json;

namespace ServerNetworkAPI.dev.IO
{
    public class ConfigManager
    {
        
        private static readonly string ConfigPath = Path.Combine(AppConfig.ConfigBasePath, "NotificationConfig.json");

        public static NotificationConfig NotificationConfig { get; set; } = new();


        public static void LoadOrCreateNotificationConfig()
        {
            if (!Directory.Exists(AppConfig.ConfigBasePath))
                Directory.CreateDirectory(AppConfig.ConfigBasePath);

            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                NotificationConfig = JsonSerializer.Deserialize<NotificationConfig>(json) ?? new NotificationConfig();
            }
            else
            {
                SaveNotificationConfig(); 
            }
        }

        public static void SaveNotificationConfig()
        {
            var json = JsonSerializer.Serialize(NotificationConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

    }
}
