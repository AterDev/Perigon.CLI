namespace CommandLine.Commands;

/// <summary>
/// studio command
/// </summary>
public class StudioCommand : AsyncCommand<StudioCommand.Settings>
{
    public class Settings : CommandSettings { }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        await StudioRunner.RunStudioAsync();
        return 0;
    }
}

public class StudioUpdateCommand : AsyncCommand
{
    public override Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        StudioRunner.UpdateStudio();
        return Task.FromResult(0);
    }
}
