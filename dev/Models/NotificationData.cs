namespace ServerNetworkAPI.dev.Models
{
    public class NotificationData
    {
        public string? TimeStamp { get; private set; } = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
        public string Message { get; set; } = "";

        public static Stack<NotificationData> notificationDatas { get; private set; } = [];

        public static Stack<NotificationData> GetNotificationData()
        {
            return notificationDatas;
        }

        public static void ClearNotification()
        {
            notificationDatas.Clear();
        }

        public static void AddNotification(NotificationData notificationData)
        {
            notificationData.Message = FormatMessage(notificationData.Message);
            notificationDatas.Push(notificationData);
        }

        private static string FormatMessage(string message)
        {
            return message = message.Replace("{ content = ", "").Replace(" }","");
        }
    }
}
