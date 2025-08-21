using System.ComponentModel;
using Share.Helper;

namespace CommandLine.Commands;

/// <summary>
/// 生成请求
/// </summary>
public class RequestCommand : AsyncCommand<RequestSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, RequestSettings settings)
    {
        if (Enum.TryParse<RequestType>(settings.Type, true, out RequestType type))
        {
            if (type == RequestType.Angular)
            {
                //StudioRunner.UpdateStudio();
            }
            return Task.FromResult(0);
        }
        else
        {
            OutputHelper.Error("Invalid type, only support: axios, csharp, angular");
            return Task.FromResult(-1);
        }
    }
}

public sealed class RequestSettings : CommandSettings
{
    [CommandArgument(0, "<path|url>")]
    [Description("local path or url, support json format")]
    public string Path { get; set; } = string.Empty;

    [CommandArgument(1, "<outputPath>")]
    [Description("your client project path")]
    public required string OutputPath { get; set; }

    [CommandOption("-t|--type")]
    [DefaultValue("axios")]
    [Description("support types: axios, csharp, angular, default: axios")]
    public string Type { get; set; } = "axios";
}

public enum RequestType
{
    Axios,
    Csharp,
    Angular,
}
