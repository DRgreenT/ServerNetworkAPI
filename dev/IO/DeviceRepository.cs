using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Models;

using System.Text.Json;

namespace ServerNetworkAPI.dev.IO
{
    public class DeviceRepository
    {
        public static async Task LoadAsync()
        {
            if (!File.Exists(AppConfig.SaveFilePath))
            {
                Logger.Log("[DeviceRepository] No device file found.",false);
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(AppConfig.SaveFilePath);
                var devices = JsonSerializer.Deserialize<List<Device>>(json);

                if (devices != null)
                {
                    NetworkContext.SetDevices(devices);
                    Logger.Log($"[DeviceRepository] Loaded {devices.Count} devices.",true);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[DeviceRepository] Failed to load devices: {ex.Message}", false);
            }
        }

        public static async Task SaveAsync()
        {
            try
            {
                var snapshot = NetworkContext.Snapshot();
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(AppConfig.SaveFilePath, json);
                Logger.Log($"[DeviceRepository] Saved {snapshot.Count} devices.", true);
            }
            catch (Exception ex)
            {
                Logger.Log($"[DeviceRepository] Failed to save devices: {ex.Message}",false);
            }
        }


    }
}
