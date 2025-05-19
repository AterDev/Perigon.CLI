using AterStudio;
using AterStudio.Worker;
using Mapster;
using StudioMod;

TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "⏱️ HH:mm:ss ";
});

builder.AddDefaultComponents();

builder.AddDefaultWebServices();

WebApplication app = builder.Build();
app.UseDefaultWebServices();

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

