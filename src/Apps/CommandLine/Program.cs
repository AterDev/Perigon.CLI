using CommandLine;
using CommandLine.Commands;
using CoreMod.Managers;
using CoreMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Share;
using Share.Helper;
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
var isAgentRawCommand = args.Length >= 2
    && args[0].Equals(SubCommand.Agent, StringComparison.OrdinalIgnoreCase)
    && (
        args[1].Equals(SubCommand.Init, StringComparison.OrdinalIgnoreCase)
        || args[1].Equals(SubCommand.Mcp, StringComparison.OrdinalIgnoreCase)
    );

if (isAgentRawCommand)
{
    // Make all early startup outputs protocol-safe for stdio clients.
    Environment.SetEnvironmentVariable("PERIGON_MCP_STDIO", "1");
}

var isHelpOrVersionCommand = args.Any(arg =>
    arg.Equals("-h", StringComparison.OrdinalIgnoreCase)
    || arg.Equals("--help", StringComparison.OrdinalIgnoreCase)
    || arg.Equals("-v", StringComparison.OrdinalIgnoreCase)
    || arg.Equals("--version", StringComparison.OrdinalIgnoreCase)
);

if (!isAgentRawCommand && isHelpOrVersionCommand)
{
    OutputHelper.ShowLogo();
}

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();

builder.AddFrameworkServices();

builder.Services.AddLocalization();
builder.Services.AddScoped<Localizer>();
builder.Services.AddScoped<SolutionContext>();
builder.Services.AddScoped<SolutionService>();
builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<AotJsonContextService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandService>();
builder.Services.AddScoped<ModulePackageService>();
builder.Services.AddScoped<OfficialModuleService>();
builder.Services.AddScoped<ModuleInstallService>();
builder.Services.AddSingleton<TemplateComparisonService>();
builder.Services.AddSingleton<ICommandRunner, ProcessCommandRunner>();
builder.Services.AddScoped<TemplateUpdateService>();
builder.Services.AddScoped<EntityInfoManager>();

builder.Services.AddScoped<NewCommand>();
builder.Services.AddScoped<StudioCommand>();
builder.Services.AddScoped<RequestCommand>();
builder.Services.AddScoped<GenerateEntityCommand>();
builder.Services.AddScoped<GenerateDtoCommand>();
builder.Services.AddScoped<GenerateManagerCommand>();
builder.Services.AddScoped<GenerateControllerCommand>();
builder.Services.AddScoped<AddModuleCommand>();
builder.Services.AddScoped<AddServiceCommand>();
builder.Services.AddScoped<PackCommand>();
builder.Services.AddScoped<InstallCommand>();
builder.Services.AddScoped<UpdateCommand>();
builder.Services.AddScoped<ModuleListCommand>();
builder.Services.AddScoped<AgentInitCommand>();
builder.Services.AddScoped<AgentMcpCommand>();
builder.Services.AddScoped<AotJsonContextCommand>();

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
    config.UseAssemblyInformationalVersion();
    config.SetApplicationCulture(systemCulture);

    config
        .AddCommand<NewCommand>(SubCommand.New)
        .WithDescription(localizer.Get(Localizer.NewDes))
        .WithExample(["new", "name"]);

    config
        .AddCommand<UpdateCommand>(SubCommand.Update)
        .WithDescription(localizer.Get(Localizer.UpdateDes))
        .WithExample([SubCommand.Update]);

    ConfiguratorExtensions
        .AddBranch(
            config,
            SubCommand.Add,
            add =>
            {
                add.SetDescription(localizer.Get(Localizer.AddDes));
                add
                    .AddCommand<AddModuleCommand>(SubCommand.Module)
                    .WithDescription(localizer.Get(Localizer.AddModuleDes))
                    .WithAlias("m")
                    .WithExample(["add", "module", "FileManagerMod"]);
                add
                    .AddCommand<AddServiceCommand>(SubCommand.Service)
                    .WithDescription(localizer.Get(Localizer.AddServiceDes))
                    .WithAlias("s")
                    .WithExample(["add", "service", "AdminService"]);
            }
        )
        .WithAlias("a");

    ConfiguratorExtensions.AddBranch(
        config,
        SubCommand.Studio,
        studio =>
        {
            studio.SetDescription(localizer.Get(Localizer.StudioDes));
            studio.SetDefaultCommand<StudioCommand>();
            studio
                .AddCommand<StudioUpdateCommand>(SubCommand.Update)
                .WithDescription(localizer.Get(Localizer.UpdateStudioDes));
        }
    );

    ConfiguratorExtensions
        .AddBranch(
            config,
            SubCommand.Generate,
            generate =>
            {
                generate.SetDescription(localizer.Get(Localizer.GenerateDes));

                generate
                    .AddCommand<GenerateEntityCommand>(SubCommand.Entity)
                    .WithDescription(localizer.Get(Localizer.GenerateEntity));

                generate
                    .AddCommand<GenerateDtoCommand>(SubCommand.Dto)
                    .WithDescription(localizer.Get(Localizer.GenerateDtos))
                    .WithExample(["generate", "dto", "./src/Share/Entity/User.cs"]);

                generate
                    .AddCommand<GenerateManagerCommand>(SubCommand.Manager)
                    .WithDescription(localizer.Get(Localizer.GenerateManagers))
                    .WithExample(["generate", "manager", "./src/Share/Entity/User.cs"]);

                generate
                    .AddCommand<GenerateControllerCommand>(SubCommand.Controller)
                    .WithDescription(localizer.Get(Localizer.GenerateController))
                    .WithAlias("api")
                    .WithExample([
                        "generate",
                        "controller",
                        "./src/Share/Entity/User.cs",
                        "AdminService"
                    ]);

                generate
                    .AddCommand<RequestCommand>(SubCommand.Request)
                    .WithDescription(localizer.Get(Localizer.RequestDes))
                    .WithExample(
                        ["generate", "request", "./openapi.json", "./src/services", "-t", "angular"]
                    );
            }
        )
        .WithAlias("g");

    ConfiguratorExtensions
        .AddBranch(
            config,
            SubCommand.Module,
            module =>
            {
                module.SetDescription(localizer.Get(Localizer.Modules));
                module
                    .AddCommand<ModuleListCommand>(SubCommand.List)
                    .WithDescription(localizer.Get(Localizer.ListOfficialModules))
                    .WithExample(["module", "list"]);

                module
                    .AddCommand<InstallCommand>(SubCommand.Install)
                    .WithDescription(localizer.Get(Localizer.InstallDes))
                    .WithExample([
                        "module",
                        "install",
                        "./package_modules/FileManagerMod.zip",
                        "AdminService",
                        "--front-path",
                        "src/ClientApp/WebApp"
                    ]);

                module
                    .AddCommand<PackCommand>(SubCommand.Pack)
                    .WithDescription(localizer.Get(Localizer.PackDes))
                    .WithExample([
                        "module",
                        "pack",
                        "FileManagerMod",
                        "AdminService",
                        "--version",
                        "1.0.0",
                        "--front-path",
                        "src/ClientApp/WebApp/src/app/modules/file-manager"
                    ]);
            }
        )
        .WithAlias("m");

    ConfiguratorExtensions.AddBranch(
        config,
        SubCommand.Agent,
        agent =>
        {
            agent.SetDescription(localizer.Get(Localizer.McpDes));
            agent
                .AddCommand<AgentInitCommand>(SubCommand.Init)
                .WithDescription(localizer.Get(Localizer.McpInitDes));
            agent
                .AddCommand<AgentMcpCommand>(SubCommand.Mcp)
                .WithDescription(localizer.Get(Localizer.AgentMcpDes));
        }
    );

    ConfiguratorExtensions.AddBranch(
        config,
        SubCommand.Aot,
        aot =>
        {
            aot.SetDescription("Native AOT tools");
            aot.AddCommand<AotJsonContextCommand>(SubCommand.JsonContext)
                .WithDescription("Generate ApiJsonSerializerContext for service projects")
                .WithExample(["aot", "jsonContext", "--path", "."]);
        }
    );

    config.SetExceptionHandler(
        (ex, resolver) =>
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        }
    );
});

return app.Run(args);
