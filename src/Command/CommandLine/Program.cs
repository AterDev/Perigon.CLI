using System.Globalization;
using System.Text;
using CommandLine;
using CommandLine.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share;
using Share.Services;

Console.OutputEncoding = Encoding.UTF8;

var systemCulture = CultureInfo.CurrentCulture;

OutputHelper.ShowLogo();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLocalization();

builder.Services.AddScoped<Localizer>();
builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandRunner>();
var host = builder.Build();

var localizer = host.Services.GetRequiredService<Localizer>();
var registrar = new DITypeRegistrar(host.Services);

var app = new CommandApp(registrar);
app.Configure(config =>
{

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
    config.SetApplicationName("dry");
    config.SetApplicationVersion("1.0.0");
    config.SetApplicationCulture(systemCulture);

    config.AddCommand<NewCommand>(SubCommand.New)
        .WithDescription(localizer.Get(SubCommand.NewDes))
        .WithExample(["new", "name"]);

    config.AddCommand<StudioCommand>(SubCommand.Studio)
        .WithDescription(localizer.Get(SubCommand.StudioDes));

    config.AddCommand<UpdateCommand>(SubCommand.Update)
        .WithDescription(localizer.Get(SubCommand.UpdateDes));

    config.AddBranch(SubCommand.Generate, config =>
    {
        config.SetDescription(localizer.Get(SubCommand.GenerateDes));

        config.AddCommand<RequestCommand>(SubCommand.Request)
            .WithDescription(localizer.Get(SubCommand.RequestDes))
            .WithExample(["generate", "request", "./openapi.json", "./src/services", "-t", "angular"]);

    }).WithAlias("g");

    config.SetExceptionHandler((ex, resolver) =>
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        return -1;
    });
});

return app.Run(args);
