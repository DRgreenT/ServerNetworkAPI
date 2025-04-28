using Microsoft.Extensions.FileProviders;
using ServerNetworkAPI.dev.CLI;
using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.IO;
using ServerNetworkAPI.dev.Models;
using ServerNetworkAPI.dev.Models.Enums;
using ServerNetworkAPI.dev.Network.Adapter;
using ServerNetworkAPI.dev.Services;
using ServerNetworkAPI.dev.UI;

namespace ServerNetworkAPI
{
    public class Program
    {
        

        public static bool isInitArp = true;
        public static bool isInitNmap = true;
        public static void Main(string[] args)
        {
            var parsedArgs = CLIArgsParser.Parse(args);

            if (parsedArgs.ShowHelp && !SystemInfoService.IsHeadlessServer())
            {
                CLIArgsParser.PrintHelp();
                return;
            }


            if (SystemInfoService.IsHeadlessServer())
            {
                AppConfig.SetUserInterface(true);
                isInitArp = false;
                isInitNmap = false;
            }
            else
            {
                AppConfig.SetUserInterface();
                PasswortHandler.SetPasswordArray(PasswortHandler.PasswordInput());
            }

            FileHelper.EnsureApplicationDirectories();
            AppConfig.InitializeFromArgs(parsedArgs);
            
            OutputLayout.Initialize();
            OutputFormatter.PrintStartupInfo();

            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();


            builder.Services.AddControllers();
            builder.Services.AddHostedService<TasksBackgroundService>();

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

            ConfigureStaticWebUI(app);

            var url = $"http://0.0.0.0:{AppConfig.WebApiPort}";
            app.Run(url);
        }

        private static void ConfigureStaticWebUI(WebApplication app)
        {
            string wwwRootPath = Path.Combine(AppContext.BaseDirectory, "wwwRoot");

            if (!Directory.Exists(wwwRootPath))
            {
                Logger.Log(
                    LogData.NewLogEvent(
                    "WebUI",
                    $"wwwRoot not found! ({wwwRootPath})",
                    MessageType.Error
                ));
                return;
            }

            var fileProvider = new PhysicalFileProvider(wwwRootPath);

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = ""
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = ""
            });
 
            Logger.Log(
                LogData.NewLogEvent(
                "WebUI",
                $"WebUI @ {LocalAdapterService.GetLocalIPv4Address()}:{AppConfig.WebApiPort}",
                MessageType.Success
                ));
        }
    }
}
