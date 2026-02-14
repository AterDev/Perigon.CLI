using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Entity;
using Share;
using Share.Helper;
using Share.Services;
using StudioMod.Managers;
using StudioMod.McpTools;
using System.Text.Json;

namespace CommandLine.Commands;

public class McpConfigCommand : AsyncCommand
{
    public override Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        var config = new
        {
            mcpServers = new Dictionary<string, object>
            {
                [ConstVal.CommandName] = new
                {
                    command = ConstVal.CommandName,
                    args = new[] { SubCommand.Mcp, SubCommand.Start }
                }
            }
        };

        Console.WriteLine(
            JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            )
        );

        return Task.FromResult(0);
    }
}

public class McpStartCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        Environment.SetEnvironmentVariable("PERIGON_MCP_STDIO", "1");

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();

        builder.AddFrameworkServices();
        builder.Services.AddLocalization();
        builder.Services.AddScoped<Localizer>();
        builder.Services.AddScoped<IProjectContext, ProjectContext>();
        builder.Services.AddScoped<SolutionService>();
        builder.Services.AddScoped<CodeAnalysisService>();
        builder.Services.AddScoped<CodeGenService>();
        builder.Services.AddScoped<CommandService>();
        builder.Services.AddScoped<EntityInfoManager>();
        builder.Services.AddScoped<GenActionManager>();
        builder.Services.AddScoped<ActionRunModelService>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(CodeTools).Assembly);

        using var host = builder.Build();
        await host.RunAsync(cancellationToken);
        return 0;
    }
}