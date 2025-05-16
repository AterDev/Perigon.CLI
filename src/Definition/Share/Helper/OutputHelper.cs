using Spectre.Console;

namespace Share.Helper;
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

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]✖️ {message}[/]");
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]✔️ {message}[/]");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠️ {message}[/]");
    }

    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"{message}");
    }
    public static void Important(string message)
    {
        AnsiConsole.MarkupLine($"[blue]{message}[/]");
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

