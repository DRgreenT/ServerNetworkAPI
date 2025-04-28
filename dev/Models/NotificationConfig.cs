using ServerNetworkAPI.dev.Models.Enums;

namespace ServerNetworkAPI.dev.Models
{
    public class NotificationConfig
    {
        public string WebhookUrl { get; set; } = "https://maker.ifttt.com/trigger/new_device_detected/with/key/YOUR_IFTTT_KEY";
        public bool EnableNotifications { get; set; } = true;
        public NotificationLevel NotificationLevel { get; set; } =  NotificationLevel.All;
    }
}