using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.UI;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Core;

namespace ServerNetworkAPI.dev.Services
{
    public class TasksBackgroundService : BackgroundService
    {
        private readonly ScanService _scanService = new();
        private readonly CancellationTokenSource _TokenSource = new();

        public static DateTime ServiceStartTime { get; private set; } = DateTime.Now;
        public static bool IsInternetAvailable { get; private set; } = false;

        public static ApiOutputData apiData = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            NotificationService.SendMessage("Network scan service started.", false);

            _ = Task.Factory.StartNew(() => UpdateLogData(_TokenSource.Token),
                                  _TokenSource.Token,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default);

            _ = Task.Factory.StartNew(() => PreventKeyInputByUser(_TokenSource.Token),
                                  _TokenSource.Token,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default);

            _ = Task.Factory.StartNew(() => UpdateInternetStatus(_TokenSource.Token),
                                  _TokenSource.Token,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default);

            _ = Task.Factory.StartNew(() => UpdateApiData(_TokenSource.Token),
                                    _TokenSource.Token,
                                    TaskCreationOptions.LongRunning,
                                    TaskScheduler.Default);
            _ = Task.Factory.StartNew(() => LogActiveDevices(_TokenSource.Token),
                        _TokenSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);

            _ = Task.Factory.StartNew(() => ScanService.ScanAnimation(_TokenSource.Token),
                                  _TokenSource.Token,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default);

            await _scanService.RunAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _TokenSource.Cancel();

            OutputFormatter.PrintMessage("Network scan service stopped.", ConsoleColor.Yellow);
            NotificationService.SendMessage("Network scan service terminated.", true);

            await base.StopAsync(cancellationToken);
            await DeviceRepository.SaveAsync();

            OutputFormatter.PrintDeviceSummary();
            Console.WriteLine();
        }

        public static List<int> activeDevicesCounts = [];
        private static async Task LogActiveDevices(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                activeDevicesCounts.Add(NetworkContext.GetActiveDevices().Count());
                await Task.Delay(2000, token);
                
            }
        }

        private static async Task UpdateApiData(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(2000, token);
                apiData = ApiOutputData.GetData();
            }
        }
        private static async Task UpdateLogData(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(200, token);
                LogData.GetLogData();
            }
        }

        private static async Task UpdateInternetStatus(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                IsInternetAvailable = await InternetConnectivityService.IsInternetAvailableAsync();
                await Task.Delay(3600, token);
            }
        }
        private static async Task PreventKeyInputByUser(CancellationToken token)
        {
            DisableConsoleInput();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    while (Console.KeyAvailable)
                        Console.ReadKey(intercept: true);

                    await Task.Delay(200, token);
                }
            }
            finally
            {
                EnableConsoleInput();
            }
        }

        private static void DisableConsoleInput()
        {
            Console.TreatControlCAsInput = false;
            Console.CursorVisible = false;
        }

        private static void EnableConsoleInput()
        {
            Console.TreatControlCAsInput = true;
            Console.CursorVisible = true;
        }       
    }
}
