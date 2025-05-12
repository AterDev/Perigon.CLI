using Spectre.Console.Cli;

namespace CommandLine.Commands;
public class AddCommand : AsyncCommand<AddCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        public string Name { get; set; } = string.Empty;
        [CommandOption("--path")]
        public string Path { get; set; } = string.Empty;
    }


    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}
