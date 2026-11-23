using CoreMod.Services;
using Share;
using Share.Helper;

namespace CommandLine.Commands;

/// <summary>
/// add module command
/// </summary>
public class AddModuleCommand(
    SolutionService solutionService,
    SolutionContext projectContext,
    Localizer localizer
)
{
    public class Settings
    {
        public required string ModuleName { get; set; }
    }

    public async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
    {
        if (!CommandSolutionHelper.TrySetSolution(projectContext, localizer))
        {
            return 1;
        }

        var moduleName = settings.ModuleName.EndsWith(
            ConstVal.ModSuffix,
            StringComparison.OrdinalIgnoreCase
        )
            ? settings.ModuleName
            : $"{settings.ModuleName}{ConstVal.ModSuffix}";

        var modulePath = Path.Combine(projectContext.ModulesPath!, moduleName);
        if (Directory.Exists(modulePath))
        {
            OutputHelper.Warning(localizer.Get(Localizer.ModuleAlreadyExists, moduleName));
            return 1;
        }

        await solutionService.CreateModuleAsync(moduleName);
        OutputHelper.Success(localizer.Get(Localizer.AddModuleSuccess, moduleName));
        return 0;
    }
}

/// <summary>
/// add service command
/// </summary>
public class AddServiceCommand(
    SolutionService solutionService,
    SolutionContext projectContext,
    Localizer localizer
)
{
    public class Settings
    {
        public required string ServiceName { get; set; }
    }

    public async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
    {
        if (!CommandSolutionHelper.TrySetSolution(projectContext, localizer))
        {
            return 1;
        }

        var (success, errorMsg) = await solutionService.CreateServiceAsync(settings.ServiceName);
        if (!success)
        {
            OutputHelper.Error(
                localizer.Get(
                    Localizer.AddServiceFailed,
                    settings.ServiceName,
                    errorMsg ?? string.Empty
                )
            );
            return 1;
        }

        OutputHelper.Success(localizer.Get(Localizer.AddServiceSuccess, settings.ServiceName));
        return 0;
    }
}

internal static class CommandSolutionHelper
{
    public static bool TrySetSolution(
        SolutionContext projectContext,
        Localizer localizer
    )
    {
        var currentDirectory = Environment.CurrentDirectory;
        var currentDir = new DirectoryInfo(currentDirectory);
        var solutionFile = AssemblyHelper.GetSlnFile(currentDir, currentDir.Root);

        if (solutionFile?.DirectoryName == null)
        {
            OutputHelper.Error(localizer.Get(Localizer.NotInSolutionDirectory));
            return false;
        }

        projectContext.SetSolution(solutionFile.DirectoryName);
        return true;
    }
}
