using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.UI;
using ServerNetworkAPI.dev.Models;

namespace ServerNetworkAPI.dev.Services
{
    public class TasksBackgroundService : BackgroundService
    {
        private readonly ScanService _scanService = new();
        private readonly CancellationTokenSource _displayTokenSource = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            NotificationService.SendMessage("@everyone Network scan service started.", false);

            _ = Task.Run(() => UpdateLogData(_displayTokenSource.Token));
            _ = Task.Run(() => PreventKeyInputByUser(_displayTokenSource.Token));

            await _scanService.RunAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _displayTokenSource.Cancel();


            OutputFormatter.PrintMessage("Network scan service stopped.", ConsoleColor.Yellow);
            NotificationService.SendMessage("@everyone Network scan service terminated.", true);

            await base.StopAsync(cancellationToken);
            await DeviceRepository.SaveAsync();

           
            OutputFormatter.PrintDeviceSummary();
            Console.WriteLine();

        }
        private static async Task UpdateLogData(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(200, token);
                LogData.GetLogData();
            }
        }
        private static async Task PreventKeyInputByUser(CancellationToken token)
        {
            SetConsoleKeySettings(false);

            while (!token.IsCancellationRequested)
            {
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                }

                await Task.Delay(200, token); ;
            }

            SetConsoleKeySettings(true);
        }

        private static void SetConsoleKeySettings(bool isState)
        {
            Console.TreatControlCAsInput = isState;
            Console.CursorVisible = isState;
        }
    }
}
