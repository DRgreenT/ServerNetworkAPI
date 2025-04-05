using ServerNetworkAPI.dev;

var startArgs = CLIArgsParser.GetArgs(args);

if (startArgs.ShowHelp)	
{
    CLIArgsParser.ShowHelp();
    return;
}
else
{

    Init.WebApiPort = startArgs.Port;
    Init.timeOut = startArgs.TimeoutSeconds;
    Init.WebApiName = startArgs.ControllerName;
    Init.isNmapScanActive = startArgs.NmapScanActive;
    Init.fallbackIpMask = startArgs.FallbackIpMask;
    OutputManager.EditRow(4,Init.GetParameterValues());
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.SetMinimumLevel(LogLevel.Error);
    builder.Services.AddControllers();
    builder.Services.AddSingleton<NetworkScan>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<NetworkScan>());
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
    app.Run("http://0.0.0.0:" + Init.WebApiPort);
}


