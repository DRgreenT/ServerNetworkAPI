using ServerNetworkAPI.dev.Models.Enums;


namespace ServerNetworkAPI.dev.Models
{
    public class LogData
    {
        public string? TimeStamp { get; private set; }
        public string Source { get; set; } = "";
        public string Message { get; set; } = "";
        public string ExeptionMessage { get; set; } = "";
        public MessageType MessageType { get; set; }

        public static List<LogData> logDatas { get; private set; } = [];

        public static List<LogData> GetLogData()
        {
            return logDatas;
        }
        public static void ClearLog()
        {
            logDatas.Clear();
        }
        public static void AddLog(LogData logData)
        {
            logDatas.Add(logData);
        }
        public static LogData NewData(string source, string message, MessageType messageType, string exeption = "")
        {
            return new LogData
            {
                TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Source = source,
                Message = message,
                ExeptionMessage = exeption,
                MessageType = messageType
            };
        }

    }

}
