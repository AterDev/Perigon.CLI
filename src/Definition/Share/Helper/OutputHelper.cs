using Spectre.Console;

namespace Share.Helper;

public class OutputHelper
{
    private static bool IsMcpStdioMode =>
        Environment.GetEnvironmentVariable("PERIGON_MCP_STDIO") == "1";

    public static void ShowLogo()
    {
        if (IsMcpStdioMode)
        {
            return;
        }

        string logo = """

            ██████┐ ███████┐██████┐ ██┐ ██████┐  ██████┐ ███┐   ██┐
            ██┌──██┐██┌────┐██┌──██┐██│██┌────┘ ██┌───██┐████┐  ██│
            ██████┌┘█████┐  ██████┌┘██│██│  ███┐██│   ██│██┌██┐ ██│
            ██┌───┘ ██┌──┘  ██┌──██┐██│██│   ██│██│   ██│██│└██┐██│
            ██│     ███████┐██│  ██│██│└██████┌┘└██████┌┘██│ └████│
            └─┘     └──────┘└─┘  └─┘└─┘ └─────┘  └─────┘ └─┘  └───┘
            """;
        string sign1 = "                 —→ for freedom 🗽 ←—";
        string docsLine = "[[docs]]:   [link]https://dusi.dev/docs/Perigon.html[/]";
        string gitHubLine = "[[GitHub]]: [link]https://github.com/AterDev/Perigon.CLI[/]";

        AnsiConsole.MarkupLine($"[bold green]{logo}[/]");
        AnsiConsole.MarkupLine($"[yellow]{sign1}[/]");
        AnsiConsole.MarkupLine($"[blue]{docsLine}[/]");
        AnsiConsole.MarkupLine($"[blue]{gitHubLine}[/]");
        AnsiConsole.MarkupLine("");

    }

    public static void Error(string message)
    {
        if (IsMcpStdioMode)
        {
            Console.Error.WriteLine($"✖️ {message}");
            return;
        }
        AnsiConsole.MarkupLineInterpolated($"[red]✖️ {message}[/]");
    }

    public static void Success(string message)
    {
        if (IsMcpStdioMode)
        {
            Console.Error.WriteLine($"✅ {message}");
            return;
        }
        AnsiConsole.MarkupLineInterpolated($"[green]✅ {message}[/]");
    }

    public static void Warning(string message)
    {
        if (IsMcpStdioMode)
        {
            Console.Error.WriteLine($"⚠️ {message}");
            return;
        }
        AnsiConsole.MarkupLineInterpolated($"[yellow]⚠️ {message}[/]");
    }

    public static void Info(string message)
    {
        if (IsMcpStdioMode)
        {
            Console.Error.WriteLine(message);
            return;
        }
        AnsiConsole.MarkupLineInterpolated($"{message}");
    }
    public static void Debug(string message)
    {
        if (IsMcpStdioMode)
        {
            Console.Error.WriteLine($"[Dbg] {message}");
            return;
        }
        AnsiConsole.MarkupLineInterpolated($"[[Dbg]] [gray]{message}[/]");
    }

    public static void Important(string message)
    {
        if (IsMcpStdioMode)
        {
            Console.Error.WriteLine(message);
            return;
        }
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
    public const string Pack = "pack";
    public const string Install = "install";
    public const string Mcp = "mcp";
    public const string Config = "config";
    public const string Start = "start";
}
