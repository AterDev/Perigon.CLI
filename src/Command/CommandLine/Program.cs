using CommandLine;
using CommandLine.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share;
using Share.Helper;
using Share.Services;
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var systemCulture = CultureInfo.CurrentCulture;

OutputHelper.ShowLogo();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLocalization();
builder.Services.AddScoped<Localizer>();

builder.AddFrameworkServices();

builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandService>();

builder.Services.AddScoped<NewCommand>();
builder.Services.AddScoped<StudioCommand>();
builder.Services.AddScoped<AddCommand>();

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
    config.SetApplicationName("ater");
    config.SetApplicationVersion("10.0.0");
    config.SetApplicationCulture(systemCulture);

    config.AddCommand<NewCommand>(SubCommand.New)
        .WithDescription(localizer.Get(TipConst.NewDes))
        .WithExample(["new", "name"]);

    config.AddCommand<StudioCommand>(SubCommand.Studio)
        .WithDescription(localizer.Get(TipConst.StudioDes));

    config.AddBranch(SubCommand.Generate, config =>
    {
        config.SetDescription(localizer.Get(TipConst.GenerateDes));

        config.AddCommand<RequestCommand>(SubCommand.Request)
            .WithDescription(localizer.Get(TipConst.RequestDes))
            .WithExample(["generate", "request", "./openapi.json", "./src/services", "-t", "angular"]);

    }).WithAlias("g");

    config.SetExceptionHandler((ex, resolver) =>
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        return -1;
    });
});

return app.Run(args);
