
using ServerNetworkAPI.dev.CLI;
using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Network.Adapter;
using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.UI;

namespace ServerNetworkAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var parsedArgs = CLIArgsParser.Parse(args);

            if (parsedArgs.ShowHelp)
            {
                CLIArgsParser.PrintHelp();
                return;
            }

            FileHelper.EnsureApplicationDirectories();
            AppConfig.InitializeFromArgs(parsedArgs);
            OutputLayout.Initialize();
            OutputFormatter.PrintStartupInfo();

            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Error);

            builder.Services.AddControllers();
            builder.Services.AddHostedService<NetworkBackgroundService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            var url = $"http://0.0.0.0:{AppConfig.WebApiPort}";
            app.Run(url);
        }
    }
}
