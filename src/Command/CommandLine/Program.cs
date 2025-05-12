using System.Globalization;
using System.Text;
using CommandLine;
using CommandLine.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share;
using Share.Services;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;

var systemCulture = CultureInfo.InstalledUICulture;

if (!systemCulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
{
    CultureInfo.CurrentCulture = new CultureInfo("en-US");
    CultureInfo.CurrentUICulture = new CultureInfo("en-US");
}

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

    config.AddCommand<NewCommand>(SubCommand.New)
        .WithDescription(localizer.Get(SubCommand.NewDes))
        .WithExample(["new", "name"]);

    config.AddCommand<StudioCommand>(SubCommand.Studio)
    .WithDescription(localizer.Get(SubCommand.StudioDes));

    config.SetExceptionHandler((ex, resolver) =>
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        return -1;
    });
});

return app.Run(args);
