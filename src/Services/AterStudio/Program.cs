using AterStudio;
using AterStudio.Worker;
using Mapster;

TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "⏱️ HH:mm:ss ";
});

builder.AddDefaultComponents();
builder.AddDefaultWebServices();

// add MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

WebApplication app = builder.Build();
app.MapMcp();

app.UseDefaultWebServices();

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

