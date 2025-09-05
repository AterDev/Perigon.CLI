using Ater.Web.Convention.Abstraction;
using AterStudio;
using AterStudio.Components.Pages;
using AterStudio.McpTools;
using AterStudio.Worker;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Share.Helper;
using Share.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "⏱️ HH:mm:ss ";
});

builder.AddFrameworkServices();
builder.AddMiddlewareServices();

builder.AddBlazorServices();

builder.Services.AddManagers();

// services
builder.Services.AddSingleton<IProjectContext, ProjectContext>();

builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandService>();
builder.Services.AddScoped<SolutionService>();
builder.Services.AddSingleton<StorageService>();

builder.Services.AddSingleton<EntityTaskQueue<EventQueueModel<McpTool>>>();

// add MCP Server
builder.Services.AddSingleton<ListToolsHandler>();

builder
    .Services.AddOptions<McpServerOptions>()
    .Configure<ListToolsHandler>(
        (opts, handler) =>
        {
            opts.Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapability
                {
                    ListToolsHandler = (req, ct) => handler.Handle(req, ct),
                },
            };
        }
    );

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();

//builder.Services.AddHostedService<McpHandlerService>();

WebApplication app = builder.Build();
app.MapMcp("mcp");

app.UseMiddlewareServices();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    var server = app.Services.GetRequiredService<IServer>();
    var addressesFeature = server.Features.Get<IServerAddressesFeature>();
    foreach (var address in addressesFeature?.Addresses ?? [])
    {
        if (address.StartsWith("http://"))
        {
            OutputHelper.Success($"🤖 Mcp Server: {address}/mcp");
        }
    }
});

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}
