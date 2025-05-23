using AterStudio;
using AterStudio.Worker;
using Mapster;
using Share.Services;

TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "⏱️ HH:mm:ss ";
});

builder.AddFrameworkServices();
builder.AddMiddlewareServices();

builder.Services.AddManagers();

// services 
builder.Services.AddScoped<IProjectContext, ProjectContext>();

builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandService>();
builder.Services.AddScoped<SolutionService>();

// add MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

WebApplication app = builder.Build();
app.MapMcp("mcp");

app.UseMiddlewareServices();

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

