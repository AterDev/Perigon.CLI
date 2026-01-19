using System.ComponentModel;
using Share;
using Share.Services;

namespace CommandLine.Commands;

/// <summary>
/// Install module command
/// </summary>
public class InstallCommand(
    ModuleInstallService moduleInstallService,
    IProjectContext projectContext
) : AsyncCommand<InstallCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PackagePath>")]
        [Description("Path to the module package zip file")]
        public required string PackagePath { get; set; }

        [CommandArgument(1, "<ServiceName>")]
        [Description("Service name in Services directory")]
        public required string ServiceName { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        // Ensure we have a valid project context
        if (string.IsNullOrEmpty(projectContext.SolutionPath))
        {
            AnsiConsole.MarkupLine("[red]Error: Not in a valid solution directory[/]");
            return 1;
        }

        var success = await moduleInstallService.InstallModuleAsync(
            settings.PackagePath,
            settings.ServiceName
        );

        return success ? 0 : 1;
    }
}
