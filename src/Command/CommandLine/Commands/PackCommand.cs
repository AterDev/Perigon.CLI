using System.ComponentModel;
using Share;
using Share.Services;

namespace CommandLine.Commands;

/// <summary>
/// Pack module command
/// </summary>
public class PackCommand(
    ModulePackageService modulePackageService,
    IProjectContext projectContext
) : AsyncCommand<PackCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<ModuleName>")]
        [Description("Module name (with Mod suffix)")]
        public required string ModuleName { get; set; }

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

        var packagePath = await modulePackageService.PackageModuleAsync(
            settings.ModuleName,
            settings.ServiceName
        );

        return packagePath != null ? 0 : 1;
    }
}
