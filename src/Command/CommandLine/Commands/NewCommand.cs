using System.ComponentModel;
using Share;

namespace CommandLine.Commands;
public class NewCommand(Localizer localizer) : AsyncCommand<NewCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        [Description("Solution Name")]
        public required string Name { get; set; }
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.WriteLine();
        // 1. 选择项目类型
        var solutionType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(TipConst.SelectSolutionType))
                .AddChoices([TipConst.SolutionTypeStandard, TipConst.SolutionTypeMini]));

        // 2. 选择数据库类型
        var dbType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(TipConst.SelectDatabaseProvider))
                .AddChoices(TipConst.DatabasePostgreSql, TipConst.DatabaseSqlServer));

        // 3. 输入数据库连接字符串
        var dbConnectionString = AnsiConsole.Prompt(
         new TextPrompt<string?>(localizer.Get(TipConst.InputDbConnectionString))
            .AllowEmpty()
            .PromptStyle("green"));

        OutputHelper.ClearLine();

        // 4. 选择缓存类型
        var cacheType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(TipConst.SelectCacheType))
                .AddChoices([TipConst.CacheTypeHybrid, TipConst.CacheTypeMemory, TipConst.CacheTypeRedis]));

        string? cacheConnectionString = null;
        // 如果缓存类型不是内存，则要求输入连接字符串
        if (cacheType != TipConst.CacheTypeMemory)
        {
            cacheConnectionString = AnsiConsole.Prompt(
                new TextPrompt<string?>(localizer.Get(TipConst.InputCacheConnectionString))
                   .AllowEmpty()
                   .PromptStyle("green"));
            OutputHelper.ClearLine();
        }

        // 5. 选择队列类型 (暂不支持)
        // 6. 选择授权类型 (暂不支持)
        // 7. 其他配置 (暂不支持)

        // 8. 模块选择(多选)
        var selectModules = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title(localizer.Get(TipConst.SelectModules))
                .NotRequired()
                .AddChoices([TipConst.ModuleCMS, TipConst.ModuleCustomer, TipConst.ModuleFileManager, TipConst.ModuleOrder]));

        // 9. 输入目录
        var defaultDirectory = "./";
        var targetDirectory = AnsiConsole.Prompt(
             new TextPrompt<string>(localizer.Get(TipConst.InputDirectory))
                .PromptStyle("green")
                .DefaultValue(defaultDirectory)
                .ValidationErrorMessage("[red]Invalid directory path.[/]")
                .Validate(input =>
                {
                    // 简单的路径验证，确保路径格式有效
                    try
                    {
                        var dummy = Path.GetFullPath(input); // Check if path is valid format
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }));
        OutputHelper.ClearLine();

        AnsiConsole.Write(new Rule(localizer.Get(TipConst.SolutionSummary))
            .RuleStyle("yellow").Centered()); // 打印一个规则线作为分隔和标题

        var summaryTable = new Table()
            .AddColumn(localizer.Get(FieldConst.ConfigurationItem))
            .AddColumn(localizer.Get(FieldConst.Values))
            .AddRow(localizer.Get(FieldConst.SolutionType), solutionType)
            .AddRow(localizer.Get(FieldConst.DatabaseProvider), dbType)
            .AddRow(localizer.Get(FieldConst.DbConnectionString), dbConnectionString ?? "")
            .AddRow(localizer.Get(FieldConst.CacheType), cacheType)
            .AddRow(localizer.Get(FieldConst.CacheConnectionString), cacheConnectionString ?? "")
            .AddRow(localizer.Get(FieldConst.Modules), string.Join(", ", selectModules))
            .AddRow(localizer.Get(FieldConst.Directory), targetDirectory);

        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        // 确认创建
        var confirm = AnsiConsole.Prompt(
            new ConfirmationPrompt(localizer.Get(TipConst.RunSolutionCreate)));

        if (confirm)
        {
            OutputHelper.Info("creating solution!");

            // TODO:具体创建逻辑
            OutputHelper.Success(localizer.Get(TipConst.CreateSolutionSuccess));
        }
        return Task.FromResult(0);
    }
}
