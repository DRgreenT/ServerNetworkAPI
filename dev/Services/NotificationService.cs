using System.Text;
using System.Text.Json;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.IO;

namespace ServerNetworkAPI.dev.Services
{
    public class NotificationService
    {
        private static readonly HttpClient _httpClient = new();

        private static readonly string sendMessagePrefix = "@everyone ";

        public static void SendMessage(string message, bool isWarning)
        {
            var config = ConfigManager.NotificationConfig;

            if (!config.EnableNotifications || string.IsNullOrWhiteSpace(config.WebhookUrl) || config.WebhookUrl.Contains("YOUR_IFTTT_KEY"))
            {
                Logger.Log(LogData.NewLogEvent(
                    "NotificationService",
                    "Notification skipped (not configured).",
                    Models.Enums.MessageType.Warning
                    ));
                return;
            }

            if (config.NotificationLevel == Models.Enums.NotificationLevel.Warnings && !isWarning)
            {
                Logger.Log(
                    LogData.NewLogEvent(
                    "NotificationService",
                    "Skipped (level = warnings, but message is not warning).",
                    Models.Enums.MessageType.Standard
                    ));

                return;
            }

            var payload = new
            {
                content = sendMessagePrefix + message
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var result = _httpClient.PostAsync(config.WebhookUrl, content).Result;

                if (result.IsSuccessStatusCode)
                {
                    Logger.Log(
                        LogData.NewLogEvent(
                        "NotificationService",
                        $"Webhook sent: {message}",
                        Models.Enums.MessageType.Success
                    ));

                    if (payload != null)
                    {
                        NotificationData.AddNotification(
                            new NotificationData
                            {
                                Message = payload.ToString()!,
                            });
                    }
                }
                else
                {
                    Logger.Log(
                        LogData.NewLogEvent(
                        "NotificationService",
                        $"Failed: {result.StatusCode}",
                        Models.Enums.MessageType.Error
                    ));           
                }
            }
            catch (Exception ex)
            {
                Logger.Log(
                    LogData.NewLogEvent(
                    "NotificationService",
                    $"Exeption:",
                    Models.Enums.MessageType.Exception,
                    "In SendMessage: " + Logger.RemoveNewLineSymbolFromString(ex.Message)
                ));
            }           
        }

        public static async Task SendDeviceNotificationAsync(Device device)
        {
            var config = ConfigManager.NotificationConfig;

            if (!config.EnableNotifications || string.IsNullOrWhiteSpace(config.WebhookUrl) || config.WebhookUrl.Contains("YOUR_IFTTT_KEY"))
            {
                Logger.Log(
                    LogData.NewLogEvent(
                    "NotificationService",
                    "Notification skipped (not configured).",
                    Models.Enums.MessageType.Warning
                ));

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
                        Logger.Log(
                            LogData.NewLogEvent(
                            "NotificationService",
                            $"Webhook sent: {payload.content}",
                            Models.Enums.MessageType.Success
                        ));
                        if(payload != null)
                        {
                            NotificationData.AddNotification(
                                new NotificationData
                                {
                                    Message = payload.ToString()!,
                                });
                        }
                    }
                    else
                    {
                        Logger.Log(
                            LogData.NewLogEvent(
                            "NotificationService",
                            $"Failed: {response.StatusCode}",
                            Models.Enums.MessageType.Error
                            ));
                     
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(
                        LogData.NewLogEvent(
                        "NotificationService",
                        $"Exeption:",
                        Models.Enums.MessageType.Exception,
                        "In SendDeviceNotificationAsync: " + Logger.RemoveNewLineSymbolFromString(ex.Message)
                    ));
                }
            }
        }
    }
}
