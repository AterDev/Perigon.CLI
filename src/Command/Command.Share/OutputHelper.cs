
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

        //Console.WriteLine(logo);
        AnsiConsole.MarkupLine($"[bold green]{logo}[/]");
        AnsiConsole.MarkupLine($"[blue]{sign2}[/]");
        AnsiConsole.MarkupLine($"");
        AnsiConsole.MarkupLine($"[yellow]{sign1}[/]");
    }
}

public class SubCommand
{
    public const string New = "new";
    public const string Studio = "studio";
    public const string NewDes = "NewDes";
    public const string StudioDes = "StudioDes";
}
