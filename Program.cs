using NetworkAPI.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.UseCors("AllowAll"); // Wichtig: Muss VOR UseAuthorization stehen

app.UseAuthorization();
app.MapControllers();
app.Run("http://0.0.0.0:5050");
