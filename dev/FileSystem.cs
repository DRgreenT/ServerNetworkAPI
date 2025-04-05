using System.Text.Json;
using System.Threading;

namespace ServerNetworkAPI.dev
{
    public class FileSystem
    {
        public static bool DeviceFileExist()
        {
            return File.Exists(Init.SaveFilePath);
        }
        public void BuildFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }
        public static async Task LoadDeviceFromJson(object _lock)
        {
            if (FileSystem.DeviceFileExist())
            {
                try
                {
                    var json = await File.ReadAllTextAsync(Init.SaveFilePath);
                    var loaded = JsonSerializer.Deserialize<List<Device>>(json);
                    if (loaded != null)
                    {
                        lock (_lock)
                        {
                            Init.devices.Clear();
                            Init.devices.AddRange(loaded);
                        }

                        Output.Log(" Device list loaded.");
                        

                        List<string> localIPs = NetworkScan.GetLocalIPv4Addresses();
                        if (localIPs.Count > 0)
                        {
                            
                            Output.ShowLocalHostAddresses(localIPs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Output.Log($"# Error while loading device list: {ex.Message}");
                }
            }
        }
        public static async Task SaveDevicesToJson(List<Device> snapshot)
        {
            try
            {
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Init.SaveFilePath, json);
                Output.Log("# Device list saved.");
            }
            catch (Exception ex)
            {
                Output.Log($"# Error saving device list: {ex.Message}");
            }
        }
    }
}
