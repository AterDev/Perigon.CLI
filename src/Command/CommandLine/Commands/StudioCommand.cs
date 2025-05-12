namespace CommandLine.Commands;
public class StudioCommand : AsyncCommand<StudioCommand.Settings>
{
    public class Settings : CommandSettings
    {

    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await CommandRunner.RunStudioAsync();
        return 0;
    }
}

