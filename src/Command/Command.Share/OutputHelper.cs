
namespace Command.Share;
public class OutputHelper
{
    public static void ShowLogo()
    {
        string logo = """
               _____    _____   __     __
              |  __ \  |  __ \  \ \   / /
              | |  | | | |__) |  \ \_/ / 
              | |  | | |  _  /    \   /  
              | |__| | | | \ \     | |   
              |_____/  |_|  \_\    |_|
            """;
        string sign1 = "         —→ for freedom 🗽 ←—";
        string sign2 = "  🌐 [link]https://dusi.dev/docs[/]";

        AnsiConsole.MarkupLine($"[bold green]{logo}[/]");
        AnsiConsole.MarkupLine($"[blue]{sign2}[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine($"[yellow]{sign1}[/]");
    }

    public static void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✖️ {message}[/]");
    }

    public static void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✔️ {message}[/]");
    }

    public static void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"ℹ️ {message}");
    }
}

public class SubCommand
{
    public const string New = "new";
    public const string Studio = "studio";
    public const string Update = "update";
    public const string NewDes = "NewDes";
    public const string StudioDes = "StudioDes";
    public const string StudioUpdateDes = "StudioUpdateDes";
}
