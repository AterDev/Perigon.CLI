using CoreMod.Services;
using Share;
using Share.Helper;

namespace CommandLine.Commands;

/// <summary>
/// Install module command
/// </summary>
public class InstallCommand(
    ModuleInstallService moduleInstallService,
    OfficialModuleService officialModuleService,
    SolutionContext projectContext,
    Localizer localizer
)
{
    public class Settings
    {
        public string? PackagePath { get; set; }

        public string? ServiceName { get; set; }

        public string? FrontPath { get; set; }
    }

    public async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.PackagePath) || string.IsNullOrWhiteSpace(settings.ServiceName))
        {
            OutputHelper.Error(localizer.Get(Localizer.InstallArgumentsRequired));
            return 1;
        }

        if (!CommandSolutionHelper.TrySetSolution(projectContext, localizer))
        {
            return 1;
        }

        var success = await moduleInstallService.InstallModuleAsync(
            settings.PackagePath,
            settings.ServiceName,
            settings.FrontPath,
            cancellationToken
        );

        return success ? 0 : 1;
    }

    private async Task<int> ShowOfficialModulesAsync(CancellationToken cancellationToken)
    {
        return await ModuleCommandHelper.ShowOfficialModulesAsync(
            officialModuleService,
            localizer,
            cancellationToken
        );
    }
}
