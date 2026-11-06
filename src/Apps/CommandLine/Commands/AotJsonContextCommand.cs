using CoreMod.Services;
using Share.Helper;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace CommandLine.Commands;

public sealed class AotJsonContextSettings : CommandSettings
{
    [CommandOption("--path <PATH>")]
    [Description("Project root containing src/Services; defaults to the current directory.")]
    public string? Path { get; set; }
}

public sealed class AotJsonContextCommand(AotJsonContextService service)
    : AsyncCommand<AotJsonContextSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        AotJsonContextSettings settings,
        CancellationToken cancellationToken)
    {
        var root = System.IO.Path.GetFullPath(
            string.IsNullOrWhiteSpace(settings.Path)
                ? Environment.CurrentDirectory
                : settings.Path);

        try
        {
            var generatedFiles = await service.GenerateAsync(root, cancellationToken);
            foreach (var generatedFile in generatedFiles)
            {
                OutputHelper.Success($"Generated {generatedFile}");
            }

            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OutputHelper.Warning("JSON context generation was cancelled.");
            return 1;
        }
        catch (Exception exception)
        {
            OutputHelper.Error(exception.Message);
            return 1;
        }
    }
}
