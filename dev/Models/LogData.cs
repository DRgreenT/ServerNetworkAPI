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

        public static Stack<LogData> logDatas { get; private set; } = [];

        public static Stack<LogData> GetLogData()
        {
            return logDatas;
        }
        public static void ClearLog()
        {
            logDatas.Clear();
        }
        public static void AddLog(LogData logData)
        {
            logDatas.Push(logData);
        }
        public static LogData NewLogEvent(string source, string message, MessageType messageType, string exeption = "")
        {
            return new LogData
            {
                TimeStamp = DateTime.Now.ToString("dd-MM-yy HH:mm:ss"),
                Source = source,
                Message = message,
                ExeptionMessage = exeption,
                MessageType = messageType
            };
        }

    }

}
