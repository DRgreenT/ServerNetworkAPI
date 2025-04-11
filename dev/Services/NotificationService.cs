using System.Text;
using System.Text.Json;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.IO;

namespace ServerNetworkAPI.dev.Services
{
    public class NotificationService
    {
        private static readonly HttpClient _httpClient = new();

        public static void SendMessage(string message, bool isWarning)
        {
            var config = ConfigManager.NotificationConfig;
            LogData log = new();

            if (!config.EnableNotifications || string.IsNullOrWhiteSpace(config.WebhookUrl) || config.WebhookUrl.Contains("YOUR_IFTTT_KEY"))
            {
                log = LogData.NewData(
                    "NotificationService",
                    "Notification skipped (not configured).",
                    Models.Enums.MessageType.Warning
                );

                Logger.Log(log);
                return;
            }

            // 💡 Level-Auswertung
            if (config.NotificationLevel == Models.Enums.NotificationLevel.Warnings && !isWarning)
            {
                log = LogData.NewData(
                    "NotificationService",
                    "Skipped (level = warnings, but message is not warning).",
                    Models.Enums.MessageType.Standard
                );

                Logger.Log(log);

                return;
            }

            var payload = new
            {
                content = message
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var result = _httpClient.PostAsync(config.WebhookUrl, content).Result;

                if (result.IsSuccessStatusCode)
                {
                    log = LogData.NewData(
                        "NotificationService",
                        $"Webhook sent: {message}",
                        Models.Enums.MessageType.Success
                    );
                }
                else
                {
                    log = LogData.NewData(
                        "NotificationService",
                        $"Failed: {result.StatusCode}",
                        Models.Enums.MessageType.Error
                    );                
                }
            }
            catch (Exception ex)
            {
                log = LogData.NewData(
                    "NotificationService",
                    $"Exeption:",
                    Models.Enums.MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                );

            }
            Logger.Log(log);
        }

        public static async Task SendDeviceNotificationAsync(Device device)
        {
            var config = ConfigManager.NotificationConfig;
            LogData log = new LogData();

            if (!config.EnableNotifications || string.IsNullOrWhiteSpace(config.WebhookUrl) || config.WebhookUrl.Contains("YOUR_IFTTT_KEY"))
            {
                log = LogData.NewData(
                    "NotificationService",
                    "Notification skipped (not configured).",
                    Models.Enums.MessageType.Warning
                );
                Logger.Log(log);

                return;
            }

            if (config.NotificationLevel == Models.Enums.NotificationLevel.Warnings|| config.NotificationLevel == Models.Enums.NotificationLevel.All)
            {
                var payload = new
                {
                    content = $"@everyone 📡 New device detected:\nIP: {device.IP}\nHostname: {device.Hostname}\nOS: {device.OS}"
                };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync(config.WebhookUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        log = LogData.NewData(
                            "NotificationService",
                            $"Webhook sent: {payload.content}",
                            Models.Enums.MessageType.Success
                        );
                    }
                    else
                    {
                        log = LogData.NewData(
                            "NotificationService",
                            $"Failed: {response.StatusCode}",
                            Models.Enums.MessageType.Error
                        );
                     
                    }
                }
                catch (Exception ex)
                {
                    log = LogData.NewData(
                        "NotificationService",
                        $"Exeption:",
                        Models.Enums.MessageType.Exception,
                        Logger.RemoveNewLineSymbolFromString(ex.Message)
                    );

                }
                Logger.Log(log);
            }
        }
    }
}
