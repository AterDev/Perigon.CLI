using CoreMod.Services;
using Share.Helper;

namespace CommandLine.Commands;

public sealed class AotJsonContextSettings
{
    public string? Path { get; set; }
}

public sealed class AotJsonContextCommand(AotJsonContextService service)
{
    public async Task<int> ExecuteAsync(AotJsonContextSettings settings, CancellationToken cancellationToken)
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
