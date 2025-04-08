using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.UI;

namespace ServerNetworkAPI.dev.Services
{
    public class NetworkBackgroundService : BackgroundService
    {
        private readonly ScanService _scanService = new();
        private readonly CancellationTokenSource _displayTokenSource = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => DisplayLoop(_displayTokenSource.Token));

            Console.TreatControlCAsInput = false;
            Console.CursorVisible = false;

            await _scanService.RunAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _displayTokenSource.Cancel();

            await DeviceRepository.SaveAsync();
            OutputFormatter.PrintMessage("Network scan service stopped.", ConsoleColor.Yellow);
            OutputFormatter.PrintDeviceSummary();

            Console.TreatControlCAsInput = true;
            Console.CursorVisible = true;
            Console.WriteLine();

            await base.StopAsync(cancellationToken);
        }

        private static async Task DisplayLoop(CancellationToken token)
        {
            _ = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(intercept: true); 
                    }

                    Thread.Sleep(100); 
                }
            });

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(200, token);
            }
        }
    }
}
