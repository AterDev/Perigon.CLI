using CoreMod.Services;
using Share;
using Share.Helper;
using Share.Models;

namespace CommandLine.Commands;

/// <summary>
/// Pack module command
/// </summary>
public class PackCommand(
    ModulePackageService modulePackageService,
    SolutionContext projectContext,
    Localizer localizer
)
{
    public class Settings
    {
        public required string ModuleName { get; set; }

        public required string ServiceName { get; set; }

        public string? Version { get; set; }

        public string? FrontPath { get; set; }
    }

    public async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
    {
        var currentDirectory = Environment.CurrentDirectory;
        projectContext.SetSolution(currentDirectory);

        var version = settings.Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            version = PackageMetadata.DefaultVersion;
            OutputHelper.Warning(
                localizer.Get("PackageVersionDefaultWarning", version)
            );
        }

        var packagePath = await modulePackageService.PackageModuleAsync(
            settings.ModuleName,
            settings.ServiceName,
            settings.FrontPath,
            version
        );

        return packagePath != null ? 0 : 1;
    }
}
