using Spectre.Console;

namespace Share.Helper;

public class OutputHelper
{
    public static void ShowLogo()
    {
        string logo = """

            ██████╗ ███████╗██████╗ ██╗ ██████╗  ██████╗ ███╗   ██╗
            ██╔══██╗██╔════╝██╔══██╗██║██╔════╝ ██╔═══██╗████╗  ██║
            ██████╔╝█████╗  ██████╔╝██║██║  ███╗██║   ██║██╔██╗ ██║
            ██╔═══╝ ██╔══╝  ██╔══██╗██║██║   ██║██║   ██║██║╚██╗██║
            ██║     ███████╗██║  ██║██║╚██████╔╝╚██████╔╝██║ ╚████║
            ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═══╝
            """;
        string sign1 = "                 —→ for freedom 🗽 ←—";
        string docsLine = "[[docs]]:   [link]https://dusi.dev/docs[/]";
        string gitHubLine = "[[GitHub]]: [link]https://github.com/AterDev/Perigon.CLI[/]";

        AnsiConsole.MarkupLine($"[bold green]{logo}[/]");
        AnsiConsole.MarkupLine($"[yellow]{sign1}[/]");
        AnsiConsole.MarkupLine($"[blue]{docsLine}[/]");
        AnsiConsole.MarkupLine($"[blue]{gitHubLine}[/]");
        AnsiConsole.MarkupLine("");

    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]✖️ {message}[/]");
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[green]✅ {message}[/]");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[yellow]⚠️ {message}[/]");
    }

    public static void Info(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"{message}");
    }

    public static void Important(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[blue]{message}[/]");
    }

    public static void ClearLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, currentLineCursor - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor - 1);
    }
}

public class SubCommand
{
    public const string New = "new";
    public const string Studio = "studio";
    public const string Update = "update";
    public const string Generate = "generate";
    public const string Request = "request";
}
