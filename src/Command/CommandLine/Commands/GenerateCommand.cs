using System.ComponentModel;
using Share.Helper;
using Share.Services;

namespace CommandLine.Commands;

/// <summary>
/// 生成请求
/// </summary>
public class RequestCommand(CommandService commandService) : AsyncCommand<RequestSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        RequestSettings settings,
        CancellationToken cancellationToken
    )
    {
        if (Enum.TryParse<RequestType>(settings.Type, true, out RequestType type))
        {
            var clientType = type switch
            {
                RequestType.CSharp => RequestClientType.CSharp,
                RequestType.Angular => RequestClientType.NgHttp,
                RequestType.Axios => RequestClientType.Axios,
                _ => RequestClientType.NgHttp
            };

            OutputHelper.Important($"Generate {type} request client from {settings.Path} to {settings.OutputPath}");

            await commandService.GenerateRequestClientAsync(
                settings.Path,
                settings.OutputPath,
                clientType,
                settings.OnlyModel
            );
            return 0;
        }
        else
        {
            OutputHelper.Error("Invalid type, only support: csharp, angular");
            return -1;
        }
    }
}

public sealed class RequestSettings : CommandSettings
{
    [CommandArgument(0, "<path|url>")]
    [Description("Local path or url, support json format")]
    public string Path { get; set; } = string.Empty;

    [CommandArgument(1, "<outputPath>")]
    [Description("The output path")]
    public required string OutputPath { get; set; }

    [CommandOption("-t|--type")]
    [DefaultValue("angular")]
    [Description("Support types: csharp/angular/axios, default: angular.")]
    public string Type { get; set; } = "angular";

    [CommandOption("-m|--only-model")]
    [DefaultValue("false")]
    [Description("Only generate model files")]
    public bool OnlyModel { get; set; }
}

public enum RequestType
{
    Angular,
    Axios,
    CSharp,
}
