using Share.Services;

namespace CommandLine.Commands;

/// <summary>
/// add command
/// </summary>
/// <param name="dbContext"></param>
public class AddCommand(CommandService commandService) : AsyncCommand<AddCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        public string Name { get; set; } = string.Empty;

        [CommandOption("--path")]
        public string Path { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        var path = settings.Path;
        var name = settings.Name;
        var projectId = await commandService.AddProjectAsync(name, path);
        return projectId == null ? 1 : 0;
    }
}
