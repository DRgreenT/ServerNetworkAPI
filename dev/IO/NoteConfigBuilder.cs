using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
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
            LogData log = new();

            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
                log = LogData.NewData(
                    "NoteConfigBuilder",
                    $"Created config directory: {ConfigDir}",
                    MessageType.Warning
                );

                Logger.Log(log);
            }
            if (!File.Exists(ConfigFile))
            {
                log = LogData.NewData(
                    "NoteConfigBuilder",
                    $"Config file not found, creating default: {ConfigFile}",
                    MessageType.Warning
                );
                Logger.Log(log);
                return CreateDefault();
            }

            try
            {
                var json = File.ReadAllText(ConfigFile);
                var config = JsonSerializer.Deserialize<NotificationConfig>(json, JsonOptions);

                if (config == null)
                {
                    log = LogData.NewData(
                        "NoteConfigBuilder",
                        $"Config file is empty or invalid, creating default: {ConfigFile}",
                        MessageType.Warning
                    );
                    Logger.Log(log);
                    return CreateDefault();
                }

                log = LogData.NewData(
                    "NoteConfigBuilder",
                    $"Loaded config from: {ConfigFile}",
                    MessageType.Success
                );

                Logger.Log(log);
                return config;
            }
            catch(Exception ex)
            {
                log = LogData.NewData(
                    "NoteConfigBuilder",
                    $"Error loading config (default created!))",
                    MessageType.Error,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                );
                Logger.Log(log);
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
