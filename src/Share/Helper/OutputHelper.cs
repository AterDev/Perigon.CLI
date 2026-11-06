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
            ██┌──██┐██┌────┘██┌──██┐██│██┌────┘ ██┌───██┐████┐  ██│
            ██████┌┘█████┐  ██████┌┘██│██│  ███┐██│   ██│██┌██┐ ██│
            ██┌───┘ ██┌──┘  ██┌──██┐██│██│   ██│██│   ██│██│└██┐██│
            ██│     ███████┐██│  ██│██│└██████┌┘└██████┌┘██│ └████│
            └─┘     └──────┘└─┘  └─┘└─┘ └─────┘  └─────┘ └─┘  └───┘
            """;
        string version = AssemblyHelper.GetCurrentToolVersion();
        string sign1 = $"🗽 for freedom.                                 {version}";
        string docsLine = "[[docs]]  : [link]https://dusi.dev/docs/Perigon.html[/]";
        string gitHubLine = "[[GitHub]]: [link]https://github.com/AterDev/Perigon.CLI[/]";

        AnsiConsole.Write(
            new Panel(
                new Rows(
                    new Markup($"[bold purple]{Markup.Escape(logo.Trim())}[/]"),
                    new Markup($"[yellow]{Markup.Escape(sign1)}[/]")
                )
            )
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
        );
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
    public const string Add = "add";
    public const string Module = "module";
    public const string Service = "service";
    public const string New = "new";
    public const string Studio = "studio";
    public const string Update = "update";
    public const string Generate = "generate";
    public const string Entity = "entity";
    public const string Dto = "dto";
    public const string Manager = "manager";
    public const string Controller = "controller";
    public const string Request = "request";
    public const string Pack = "pack";
    public const string Install = "install";
    public const string List = "list";
    public const string Mcp = "mcp";
    public const string Agent = "agent";
    public const string Aot = "aot";
    public const string JsonContext = "jsonContext";
    public const string Config = "config";
    public const string Init = "init";
}
