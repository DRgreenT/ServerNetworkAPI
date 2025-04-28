using System.Text.Json;
using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;

namespace ServerNetworkAPI.dev.IO
{
    public class AppConfigBuilder
    {
        private static readonly string ConfigDir = Path.Combine(AppConfig.BaseDirectory, "Configs");
        private static readonly string ConfigFile = Path.Combine(ConfigDir, "AppConfig.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static AppConfigData LoadOrCreate()
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            if (!File.Exists(ConfigFile))
            {
                var defaultConfig = new AppConfigData();
                Save(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(ConfigFile);
                var config = JsonSerializer.Deserialize<AppConfigData>(json, JsonOptions);
                return config ?? new AppConfigData();
            }
            catch
            {
                return new AppConfigData();
            }
        }

        public static void Save(AppConfigData config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigFile, json);
        }
    }
}
