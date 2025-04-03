using NetworkAPI.Services;
using NetworkAPI; 

var builder = WebApplication.CreateBuilder(args);

var startArgs = CLIArgsParser.GetArgs(args);

if (startArgs.ShowHelp)	
{
    CLIArgsParser.ShowHelp();
    return;
}
else
{
    Settings.WebApiPort = startArgs.Port;
    Settings.timeOut = startArgs.TimeoutSeconds;
    Settings.WebApiName = startArgs.ControllerName;

    builder.Logging.SetMinimumLevel(LogLevel.Error);
    builder.Services.AddControllers();
    builder.Services.AddSingleton<NetworkScannerService>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<NetworkScannerService>());
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
    app.Run("http://0.0.0.0:" + Settings.WebApiPort);
}


