using CodeGenerator.Helper;
using CommandLine;
using CommandLine.Commands;
using Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Share;
using Share.Helper;
using Share.Services;
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var cultureName = Environment.GetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE");
var systemCulture = !string.IsNullOrWhiteSpace(cultureName)
    ? new CultureInfo(cultureName)
    : CultureInfo.CurrentUICulture;

CultureInfo.DefaultThreadCurrentCulture = systemCulture;
CultureInfo.DefaultThreadCurrentUICulture = systemCulture;
CultureInfo.CurrentCulture = systemCulture;
CultureInfo.CurrentUICulture = systemCulture;
var isMcpRawCommand = args.Length >= 2
    && args[0].Equals(SubCommand.Mcp, StringComparison.OrdinalIgnoreCase)
    && (
        args[1].Equals(SubCommand.Config, StringComparison.OrdinalIgnoreCase)
        || args[1].Equals(SubCommand.Start, StringComparison.OrdinalIgnoreCase)
    );

if (!isMcpRawCommand)
{
    OutputHelper.ShowLogo();
}

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddLocalization();
builder.AddDbContext();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<Localizer>();
builder.Services.AddScoped<IProjectContext, ProjectContext>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<SolutionService>();
builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandService>();
builder.Services.AddScoped<ModulePackageService>();
builder.Services.AddScoped<ModuleInstallService>();

builder.Services.AddScoped<NewCommand>();
builder.Services.AddScoped<StudioCommand>();
builder.Services.AddScoped<RequestCommand>();
builder.Services.AddScoped<PackCommand>();
builder.Services.AddScoped<InstallCommand>();
builder.Services.AddScoped<McpConfigCommand>();
builder.Services.AddScoped<McpStartCommand>();

var host = builder.Build();

var registrar = new DITypeRegistrar(host.Services);
var app = new CommandApp(registrar);

var localizer = host.Services.GetRequiredService<Localizer>();
app.Configure(config =>
{
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
    var version = AssemblyHelper.GetCurrentToolVersion();
    config.SetApplicationName(ConstVal.CommandName);
    config.SetApplicationVersion(version);
    config.SetApplicationCulture(systemCulture);

    config
        .AddCommand<NewCommand>(SubCommand.New)
        .WithDescription(localizer.Get(Localizer.NewDes))
        .WithExample(["new", "name"]);

    ConfiguratorExtensions.AddBranch(
        config,
        SubCommand.Studio,
        studio =>
        {
            studio.SetDescription(localizer.Get(Localizer.StudioDes));
            studio.SetDefaultCommand<StudioCommand>();
            studio
                .AddCommand<StudioUpdateCommand>(SubCommand.Update)
                .WithDescription(Localizer.UpdateStudioDes);
        }
    );

    ConfiguratorExtensions
        .AddBranch(
            config,
            SubCommand.Generate,
            config =>
            {
                config.SetDescription(localizer.Get(Localizer.GenerateDes));

                config
                    .AddCommand<RequestCommand>(SubCommand.Request)
                    .WithDescription(localizer.Get(Localizer.RequestDes))
                    .WithExample(
                        ["generate", "request", "./openapi.json", "./src/services", "-t", "angular"]
                    );
            }
        )
        .WithAlias("g");

    ConfiguratorExtensions.AddBranch(
        config,
        SubCommand.Mcp,
        mcp =>
        {
            mcp.SetDescription(localizer.Get(Localizer.McpDes));
            mcp
                .AddCommand<McpConfigCommand>(SubCommand.Config)
                .WithDescription(localizer.Get(Localizer.McpConfigDes));
            mcp
                .AddCommand<McpStartCommand>(SubCommand.Start)
                .WithDescription(localizer.Get(Localizer.McpStartDes));
        }
    );

    config
        .AddCommand<PackCommand>(SubCommand.Pack)
        .WithDescription(localizer.Get(Localizer.PackDes))
        .WithExample(["pack", "FileManagerMod", "AdminService"]);

    config
        .AddCommand<InstallCommand>(SubCommand.Install)
        .WithDescription(localizer.Get(Localizer.InstallDes))
        .WithExample(["install", "./package_modules/FileManagerMod.zip", "AdminService"]);

    config.SetExceptionHandler(
        (ex, resolver) =>
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        }
    );
});

return app.Run(args);
