
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

    public const string NewDes = "NewDes";
    public const string StudioDes = "StudioDes";
    public const string UpdateDes = "UpdateDes";
    public const string StudioUpdateDes = "StudioUpdateDes";
    public const string GenerateDes = "GenerateDes";
    public const string RequestDes = "RequestDes";
}
public class TipConst
{
    public const string SelectSolutionType = "SelectSolutionType";
    public const string SolutionTypeStandard = "Standard";
    public const string SolutionTypeMini = "Mini";

    public const string SelectDatabaseProvider = "SelectDatabaseProvider";
    public const string DatabaseSqlServer = "SqlServer";
    public const string DatabasePostgreSql = "PostgreSql";

    public const string InputDbConnectionString = "InputDbConnectionString";
    public const string SelectCacheType = "SelectCacheType";
    public const string CacheTypeMemory = "Memory";
    public const string CacheTypeRedis = "Redis";
    public const string CacheTypeHybrid = "Hybrid";

    public const string InputCacheConnectionString = "InputCacheConnectionString";

    public const string SelectModules = "SelectModules";
    public const string ModuleCMS = "ModuleCMS";
    public const string ModuleCustomer = "ModuleCustomer";
    public const string ModuleOrder = "ModuleOrder";
    public const string ModuleFileManager = "ModuleFileManager";

    public const string InputDirectory = "InputDirectory";
    public const string SolutionSummary = "SolutionSummary";

    public const string RunSolutionCreate = "RunSolutionCreate";
    public const string CreateSolutionSuccess = "CreateSolutionSuccess";
}

public class FieldConst
{
    public const string ConfigurationItem = "ConfigurationItem";
    public const string Values = "Values";
    public const string SolutionType = "SolutionType";
    public const string DatabaseProvider = "DatabaseProvider";
    public const string DbConnectionString = "DbConnectionString";
    public const string CacheType = "CacheType";
    public const string CacheConnectionString = "CacheConnectionString";
    public const string Modules = "Modules";
    public const string Directory = "Directory";
}
