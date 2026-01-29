using AterStudio;
using AterStudio.Components.Pages;
using AterStudio.McpTools;
using CodeGenerator.Helper;
using Entity;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Localization;
using ModelContextProtocol.Server;
using Perigon.MiniDb;
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


// add MCP Server
builder.Services.AddSingleton<McpToolsHandler>();

builder
    .Services.AddOptions<McpServerOptions>()
    .Configure<McpToolsHandler>(
        (opts, handler) =>
        {
            opts.Handlers = new McpServerHandlers
            {
                ListToolsHandler = (req, ct) => handler.ListToolsHandler(req, ct),
                CallToolHandler = (req, ct) => handler.CallToolHandler(req, ct),
            };
        }
    );

builder.Services.AddMcpServer().WithHttpTransport();

WebApplication app = builder.Build();
app.MapMcp("mcp");
app.UseMiddlewareServices();

// 使用 Minimal API 处理语言切换
app.MapGet("/Culture/SetCulture", (string culture, string? redirectUri, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(culture))
    {
        culture = "zh-CN";
    }

    if (string.IsNullOrWhiteSpace(redirectUri))
    {
        redirectUri = "/";
    }

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(culture, culture)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        }
    );

    return Results.LocalRedirect(redirectUri);
});

app.Lifetime.ApplicationStarted.Register(() =>
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

// 添加应用程序关闭时的清理处理
app.Lifetime.ApplicationStopping.Register(() =>
{
    try
    {
        OutputHelper.Info("🛑 Application stopping, cleaning up resources...");
        // 正常垃圾回收以释放程序集引用
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var dir = AssemblyHelper.GetStudioPath();
        var path = Path.Combine(dir, ConstVal.DbName);
        MiniDbContext.ReleaseSharedCache(path);
        OutputHelper.Info("✅ Application resources cleaned up.");
    }
    catch (Exception ex)
    {
        OutputHelper.Warning($"⚠️ Warning during cleanup: {ex.Message}");
    }
});


await app.RunAsync();



