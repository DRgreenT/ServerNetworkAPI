using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerNetworkAPI.dev.IO
{
    public class NoteConfigBuilder
    {
        private static readonly string ConfigDir = Path.Combine(AppConfig.BaseDirectory, "Configs");
        private static readonly string ConfigFile = Path.Combine(ConfigDir, "NotificationConfig.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static NotificationConfig LoadOrCreate()
        {
            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
                Logger.Log($"[NoteConfigBuilder] Created config directory: {ConfigDir}", false, ConsoleColor.Yellow);
            }
            if (!File.Exists(ConfigFile))
            {
                Logger.Log($"[NoteConfigBuilder] Config file not found, creating default: {ConfigFile}", false, ConsoleColor.Yellow);
                return CreateDefault();
            }

            try
            {
                var json = File.ReadAllText(ConfigFile);
                var config = JsonSerializer.Deserialize<NotificationConfig>(json, JsonOptions);

                if (config == null)
                {
                    Logger.Log($"[NoteConfigBuilder] Config file is empty or invalid, creating default: {ConfigFile}", false, ConsoleColor.Yellow);
                    return CreateDefault();
                }
                Logger.Log($"[NoteConfigBuilder] Loaded config from: {ConfigFile}", false, ConsoleColor.Green);
                return config;
            }
            catch(Exception ex)
            {
                Logger.Log($"[NoteConfigBuilder] {ex} -> Delault created!", false, ConsoleColor.Red);
                return CreateDefault();
            }
        }

        private static NotificationConfig CreateDefault()
        {
            var config = new NotificationConfig
            {
                WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_HERE",
                EnableNotifications = true,
                NotificationLevel = Models.Enums.NotificationLevel.All
            };

            Save(config);
            return config;
        }

        public static void Save(NotificationConfig config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigFile, json);
        }
    }
}
