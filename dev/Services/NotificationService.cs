using System.Net.Http;
using System.Text;
using System.Text.Json;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.IO;

namespace ServerNetworkAPI.dev.Services
{
    public class NotificationService
    {

        private static readonly HttpClient _httpClient = new();
        public static void SendMessage(string message)
        {
            var config = ConfigManager.NotificationConfig;

            if (!config.EnableNotifications || string.IsNullOrWhiteSpace(config.WebhookUrl) || config.WebhookUrl.Contains("YOUR_IFTTT_KEY"))
            {
                Logger.Log("[NotificationService] SendMessage skipped (not configured).", false, ConsoleColor.Yellow);
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
                    Logger.Log("[Notification] Message sent.", true, ConsoleColor.Green);
                }
                else
                {
                    Logger.Log($"[Notification] Failed: {result.StatusCode}", true, ConsoleColor.Red);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Notification] Exception: {ex.Message}", true, ConsoleColor.Red);
            }
        }
        public static async Task SendDeviceNotificationAsync(Device device)
        {
            var config = ConfigManager.NotificationConfig;

            if (!config.EnableNotifications || string.IsNullOrWhiteSpace(config.WebhookUrl) || config.WebhookUrl.Contains("YOUR_IFTTT_KEY"))
            {
                Logger.Log("[NotificationService] Notification skipped (not configured).", false, ConsoleColor.Yellow);
                return;
            }

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
                    Logger.Log($"[Notification] Webhook sent: {device.IP}", true, ConsoleColor.Green);
                }
                else
                {
                    Logger.Log($"[Notification] Failed: {response.StatusCode}", true, ConsoleColor.Red);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Notification] Exception: {ex.Message}", true, ConsoleColor.Red);
            }
        }
    }
    
}
