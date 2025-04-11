using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using System.Text.Json;

namespace ServerNetworkAPI.dev.IO
{
    public class DeviceRepository
    {
        public static async Task LoadAsync()
        {
            LogData log = new();
            if (!File.Exists(AppConfig.SaveFilePath))
            {
                log = LogData.NewData(
                    "DeviceRepository",
                    $"No device file found at {AppConfig.SaveFilePath}",
                    MessageType.Warning
                );
                Logger.Log(log);
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(AppConfig.SaveFilePath);
                var devices = JsonSerializer.Deserialize<List<Device>>(json);

                if (devices != null)
                {
                    NetworkContext.SetDevices(devices);
                    log = LogData.NewData(
                        "DeviceRepository",
                        $"Loaded {devices.Count} devices from {AppConfig.SaveFilePath}",
                        MessageType.Success
                    );
                    Logger.Log(log);
                }
            }
            catch (Exception ex)
            {
                log = LogData.NewData(
                    "DeviceRepository",
                    $"Failed to load devices from {AppConfig.SaveFilePath}",
                    MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                );
                Logger.Log(log);
            }
        }

        public static async Task SaveAsync()
        {
            LogData log = new();
            try
            {
                var snapshot = NetworkContext.Snapshot();
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(AppConfig.SaveFilePath, json);

                log = LogData.NewData(
                    "DeviceRepository",
                    $"Saved {snapshot.Count} devices to {AppConfig.SaveFilePath}",
                    MessageType.Success
                );
                Logger.Log(log);
            }
            catch (Exception ex)
            {
                log = LogData.NewData(
                    "DeviceRepository",
                    $"Failed to save devices to {AppConfig.SaveFilePath}",
                    MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                );
                Logger.Log(log);
            }
        }


    }
}
