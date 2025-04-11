using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using System.Text.Json;

namespace ServerNetworkAPI.dev.IO
{
    public class DeviceRepository
    {
        private static JsonSerializerOptions JSerializeOption { get;} = new JsonSerializerOptions { WriteIndented = true };
        public static async Task LoadAsync()
        {
            if (!File.Exists(AppConfig.SaveFilePath))
            {
                Logger.Log(
                    LogData.NewData(
                    "DeviceRepository",
                    $"No device file found at {AppConfig.SaveFilePath}",
                    MessageType.Warning 
                    ));
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(AppConfig.SaveFilePath);
                var devices = JsonSerializer.Deserialize<List<Device>>(json);

                if (devices != null)
                {
                    NetworkContext.SetDevices(devices);

                    Logger.Log(
                        LogData.NewData(
                        "DeviceRepository",
                        $"Loaded {devices.Count} devices from {AppConfig.SaveFilePath}",
                        MessageType.Success
                        ));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(
                    LogData.NewData(
                    "DeviceRepository",
                    $"Failed to load devices from {AppConfig.SaveFilePath}",
                    MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                    ));
            }
        }

        public static async Task SaveAsync()
        {
            try
            {
                var snapshot = NetworkContext.Snapshot();
                var json = JsonSerializer.Serialize(snapshot, JSerializeOption);
                await File.WriteAllTextAsync(AppConfig.SaveFilePath, json);

                Logger.Log(LogData.NewData(
                    "DeviceRepository",
                    $"Saved {snapshot.Count} devices to {AppConfig.SaveFilePath}",
                    MessageType.Success
                     ));
            }
            catch (Exception ex)
            { 
                Logger.Log(
                    LogData.NewData(
                    "DeviceRepository",
                    $"Failed to save devices to {AppConfig.SaveFilePath}",
                    MessageType.Exception,
                    Logger.RemoveNewLineSymbolFromString(ex.Message)
                    ));
            }
        }


    }
}
