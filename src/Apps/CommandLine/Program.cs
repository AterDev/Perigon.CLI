using CommandLine.Commands;
using CoreMod.Managers;
using CoreMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Share;
using Share.Helper;
using System.CommandLine;
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

if (args.Any(arg => arg.Equals("-h", StringComparison.OrdinalIgnoreCase)
    || arg.Equals("--help", StringComparison.OrdinalIgnoreCase)
    || arg.Equals("-v", StringComparison.OrdinalIgnoreCase)
    || arg.Equals("--version", StringComparison.OrdinalIgnoreCase)))
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
builder.Services.AddScoped<AotJsonContextCommand>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
var localizer = services.GetRequiredService<Localizer>();
var root = new RootCommand("Perigon CLI") { TreatUnmatchedTokensAsErrors = true };
root.HelpName = ConstVal.CommandName;
var versionOption = root.Options.OfType<VersionOption>().FirstOrDefault();
if (versionOption is null)
{
    versionOption = new VersionOption();
    root.Options.Add(versionOption);
}
versionOption.Aliases.Add("-v");

var newName = new Argument<string>("name") { Description = "Solution Name" };
var newCommand = new Command(SubCommand.New, localizer.Get(Localizer.NewDes));
newCommand.Add(newName);
newCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<NewCommand>()
    .ExecuteAsync(new NewCommand.Settings { Name = parseResult.GetRequiredValue(newName) }, cancellationToken));
root.Add(newCommand);

var updateCommand = new Command(SubCommand.Update, localizer.Get(Localizer.UpdateDes));
updateCommand.SetAction((_, cancellationToken) => services.GetRequiredService<UpdateCommand>().ExecuteAsync(cancellationToken));
root.Add(updateCommand);

var addCommand = new Command(SubCommand.Add, localizer.Get(Localizer.AddDes));
addCommand.Aliases.Add("a");
var addModuleName = new Argument<string>("ModuleName") { Description = "Module name, `Mod` suffix is optional / 模块名称，可省略 `Mod` 后缀" };
var addModuleCommand = new Command(SubCommand.Module, localizer.Get(Localizer.AddModuleDes));
addModuleCommand.Aliases.Add("m");
addModuleCommand.Add(addModuleName);
addModuleCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<AddModuleCommand>()
    .ExecuteAsync(new AddModuleCommand.Settings { ModuleName = parseResult.GetRequiredValue(addModuleName) }, cancellationToken));
addCommand.Add(addModuleCommand);
var addServiceName = new Argument<string>("ServiceName") { Description = "Service name / 服务名称" };
var addServiceCommand = new Command(SubCommand.Service, localizer.Get(Localizer.AddServiceDes));
addServiceCommand.Aliases.Add("s");
addServiceCommand.Add(addServiceName);
addServiceCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<AddServiceCommand>()
    .ExecuteAsync(new AddServiceCommand.Settings { ServiceName = parseResult.GetRequiredValue(addServiceName) }, cancellationToken));
addCommand.Add(addServiceCommand);
root.Add(addCommand);

var generateCommand = new Command(SubCommand.Generate, localizer.Get(Localizer.GenerateDes));
generateCommand.Aliases.Add("g");
var generateEntityCommand = new Command(SubCommand.Entity, localizer.Get(Localizer.GenerateEntity));
generateEntityCommand.SetAction(_ => services.GetRequiredService<GenerateEntityCommand>().Execute(new GenerateEntitySettings()));
generateCommand.Add(generateEntityCommand);

var dtoEntityPath = new Argument<string>("EntityPath") { Description = "Entity file path" };
var dtoForce = new Option<bool>("--force", ["-f"]) { Description = "Overwrite generated files" };
var generateDtoCommand = new Command(SubCommand.Dto, localizer.Get(Localizer.GenerateDtos));
generateDtoCommand.Add(dtoEntityPath);
generateDtoCommand.Add(dtoForce);
generateDtoCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<GenerateDtoCommand>()
    .ExecuteAsync(new EntityGenerateSettings { EntityPath = parseResult.GetRequiredValue(dtoEntityPath), Force = parseResult.GetValue(dtoForce) }, cancellationToken));
generateCommand.Add(generateDtoCommand);

var managerEntityPath = new Argument<string>("EntityPath") { Description = "Entity file path" };
var managerForce = new Option<bool>("--force", ["-f"]) { Description = "Overwrite generated files" };
var generateManagerCommand = new Command(SubCommand.Manager, localizer.Get(Localizer.GenerateManagers));
generateManagerCommand.Add(managerEntityPath);
generateManagerCommand.Add(managerForce);
generateManagerCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<GenerateManagerCommand>()
    .ExecuteAsync(new EntityGenerateSettings { EntityPath = parseResult.GetRequiredValue(managerEntityPath), Force = parseResult.GetValue(managerForce) }, cancellationToken));
generateCommand.Add(generateManagerCommand);

var controllerEntityPath = new Argument<string>("EntityPath") { Description = "Entity file path" };
var controllerServicePath = new Argument<string>("ServicePath") { Description = "Target service path, csproj path, or service name" };
var controllerForce = new Option<bool>("--force", ["-f"]) { Description = "Overwrite generated files" };
var generateControllerCommand = new Command(SubCommand.Controller, localizer.Get(Localizer.GenerateController));
generateControllerCommand.Aliases.Add("api");
generateControllerCommand.Add(controllerEntityPath);
generateControllerCommand.Add(controllerServicePath);
generateControllerCommand.Add(controllerForce);
generateControllerCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<GenerateControllerCommand>()
    .ExecuteAsync(new GenerateControllerSettings { EntityPath = parseResult.GetRequiredValue(controllerEntityPath), ServicePath = parseResult.GetRequiredValue(controllerServicePath), Force = parseResult.GetValue(controllerForce) }, cancellationToken));
generateCommand.Add(generateControllerCommand);

var requestPath = new Argument<string>("path|url") { Description = "Local path or url, support json format" };
var requestOutputPath = new Argument<string>("outputPath") { Description = "The output path" };
var requestType = new Option<string>("--type", ["-t"]) { Description = "Support types: csharp/angular/axios, default: angular", DefaultValueFactory = _ => "angular" };
var onlyModel = new Option<bool>("--only-model", ["-m"]) { Description = "Only generate model files" };
var coverBaseService = new Option<bool>("--cover-base-service", ["-c"]) { Description = "Overwrite generated base.service.ts" };
var requestCommand = new Command(SubCommand.Request, localizer.Get(Localizer.RequestDes));
requestCommand.Add(requestPath);
requestCommand.Add(requestOutputPath);
requestCommand.Add(requestType);
requestCommand.Add(onlyModel);
requestCommand.Add(coverBaseService);
requestCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<RequestCommand>()
    .ExecuteAsync(new RequestSettings { Path = parseResult.GetRequiredValue(requestPath), OutputPath = parseResult.GetRequiredValue(requestOutputPath), Type = parseResult.GetValue(requestType) ?? "angular", OnlyModel = parseResult.GetValue(onlyModel), CoverBaseService = parseResult.GetValue(coverBaseService) }, cancellationToken));
generateCommand.Add(requestCommand);
root.Add(generateCommand);

var moduleCommand = new Command(SubCommand.Module, localizer.Get(Localizer.Modules));
moduleCommand.Aliases.Add("m");
var moduleListCommand = new Command(SubCommand.List, localizer.Get(Localizer.ListOfficialModules));
moduleListCommand.SetAction((_, cancellationToken) => services.GetRequiredService<ModuleListCommand>().ExecuteAsync(cancellationToken));
moduleCommand.Add(moduleListCommand);

var installPackagePath = new Argument<string?>("PackagePath") { Description = "Path to the module package zip file, or official package name like Perigon.SystemMod", Arity = ArgumentArity.ZeroOrOne };
var installServiceName = new Argument<string?>("ServiceName") { Description = "Service name in Services directory", Arity = ArgumentArity.ZeroOrOne };
var installFrontPath = new Option<string>("--front-path") { Description = "Frontend project root; the module is restored under its src/app/modules directory" };
var installCommand = new Command(SubCommand.Install, localizer.Get(Localizer.InstallDes));
installCommand.Add(installPackagePath);
installCommand.Add(installServiceName);
installCommand.Add(installFrontPath);
installCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<InstallCommand>()
    .ExecuteAsync(new InstallCommand.Settings { PackagePath = parseResult.GetValue(installPackagePath), ServiceName = parseResult.GetValue(installServiceName), FrontPath = parseResult.GetValue(installFrontPath) }, cancellationToken));
moduleCommand.Add(installCommand);

var packModuleName = new Argument<string>("ModuleName") { Description = "Module name (with Mod suffix)" };
var packServiceName = new Argument<string>("ServiceName") { Description = "Service name in Services directory" };
var packVersion = new Option<string>("--version", ["-v"]) { Description = "Package version; defaults to 1.0.0 when omitted / 包版本号；省略时默认使用 1.0.0" };
var packFrontPath = new Option<string>("--front-path") { Description = "Frontend module directory to include in the package" };
var packCommand = new Command(SubCommand.Pack, localizer.Get(Localizer.PackDes));
packCommand.Add(packModuleName);
packCommand.Add(packServiceName);
packCommand.Add(packVersion);
packCommand.Add(packFrontPath);
packCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<PackCommand>()
    .ExecuteAsync(new PackCommand.Settings { ModuleName = parseResult.GetRequiredValue(packModuleName), ServiceName = parseResult.GetRequiredValue(packServiceName), Version = parseResult.GetValue(packVersion), FrontPath = parseResult.GetValue(packFrontPath) }, cancellationToken));
moduleCommand.Add(packCommand);
root.Add(moduleCommand);

var agentCommand = new Command(SubCommand.Agent, localizer.Get(Localizer.AgentInitDescription));
var agentInitCommand = new Command(SubCommand.Init, localizer.Get(Localizer.AgentInitDescription));
agentInitCommand.SetAction((_, cancellationToken) => services.GetRequiredService<AgentInitCommand>().ExecuteAsync(cancellationToken));
agentCommand.Add(agentInitCommand);
root.Add(agentCommand);

var aotCommand = new Command(SubCommand.Aot, "Native AOT tools");
var jsonContextPath = new Option<string>("--path") { Description = "Project root containing src/Services; defaults to the current directory." };
var jsonContextCommand = new Command(SubCommand.JsonContext, "Generate ApiJsonSerializerContext for service projects");
jsonContextCommand.Add(jsonContextPath);
jsonContextCommand.SetAction((parseResult, cancellationToken) => services.GetRequiredService<AotJsonContextCommand>()
    .ExecuteAsync(new AotJsonContextSettings { Path = parseResult.GetValue(jsonContextPath) }, cancellationToken));
aotCommand.Add(jsonContextCommand);
root.Add(aotCommand);

try
{
    return await root.Parse(args).InvokeAsync();
}
catch (Exception exception)
{
    OutputHelper.Error(exception.Message);
    return -1;
}
