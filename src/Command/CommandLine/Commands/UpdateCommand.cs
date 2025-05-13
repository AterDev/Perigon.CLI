namespace CommandLine.Commands;
public class UpdateCommand : Command<UpdateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        public required string Name { get; set; }
    }

    public enum UpdateType
    {
        Studio
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (Enum.TryParse<UpdateType>(settings.Name, true, out UpdateType type))
        {
            if (type == UpdateType.Studio)
            {
                StudioRunner.UpdateStudio();
            }
            return 0;
        }
        else
        {
            OutputHelper.Error("Invalid name, only support: studio");
            return 1;
        }
    }
}
