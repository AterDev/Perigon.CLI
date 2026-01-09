using Share;
using Share.Helper;
using Share.Models.CommandDtos;
using Share.Services;
using System.ComponentModel;

namespace CommandLine.Commands;

public class NewCommand(Localizer localizer, CommandService commandService)
    : AsyncCommand<NewCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        [Description("Solution Name")]
        public required string Name { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        AnsiConsole.WriteLine();
        // 1. 选择项目类型
        //var solutionType = AnsiConsole.Prompt(
        //    new SelectionPrompt<string>()
        //        .Title(localizer.Get(Localizer.SelectSolutionType))
        //        .AddChoices([Localizer.SolutionTypeStandard])
        //);
        var solutionType = localizer.Get(Localizer.SolutionTypeStandard);
        // 2. 选择数据库类型
        var dbType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(Localizer.SelectDatabaseProvider))
                .AddChoices(
                localizer.Get(Localizer.DatabasePostgreSql),
                localizer.Get(Localizer.DatabaseSqlServer))
        );

        // 4. 选择缓存类型
        var cacheType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(Localizer.SelectCacheType))
                .AddChoices([
                    localizer.Get(Localizer.CacheTypeHybrid),
                    localizer.Get(Localizer.CacheTypeMemory),
                    localizer.Get(Localizer.CacheTypeRedis)
                ])
        );

        // 5. 选择队列类型 (暂不支持)
        // 6. 选择授权类型 (暂不支持)
        // 7. 其他配置 (暂不支持)

        // 8. 模块选择(多选)
        //var options = ModuleInfo.GetModules();

        //var selectModules = AnsiConsole.Prompt(
        //    new MultiSelectionPrompt<ModuleInfo>()
        //        .Title(localizer.Get(Localizer.SelectModules))
        //        .InstructionsText(localizer.Get(Localizer.CommandSelectTip))
        //        .NotRequired()
        //        .AddChoices(options)
        //        .UseConverter(opt => $"{localizer.Get(opt.Description)}")
        //);

        var frontType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(Localizer.SelectFrontType))
                .AddChoices([Localizer.None, Localizer.Angular])
        );

        // 9. 输入目录
        var defaultDirectory = "./";
        var targetDirectory = AnsiConsole.Prompt(
            new TextPrompt<string>(localizer.Get(Localizer.InputDirectory))
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
                })
        );
        OutputHelper.ClearLine();

        AnsiConsole.Write(
            new Rule(localizer.Get(Localizer.SolutionSummary)).RuleStyle("yellow").Centered()
        ); // 打印一个规则线作为分隔和标题

        var summaryTable = new Table()
            .AddColumn(localizer.Get(Localizer.ConfigurationItem))
            .AddColumn(localizer.Get(Localizer.Values))
            .AddRow(localizer.Get(Localizer.SolutionType), solutionType)
            .AddRow(localizer.Get(Localizer.DatabaseProvider), dbType)
            .AddRow(localizer.Get(Localizer.CacheType), cacheType)
            //.AddRow(localizer.Get(Localizer.Modules), string.Join(", ", selectModules))
            .AddRow(localizer.Get(Localizer.FrontEnd), frontType)
            .AddRow(localizer.Get(Localizer.Directory), targetDirectory);

        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        // 确认创建
        var confirm = AnsiConsole.Prompt(
            new ConfirmationPrompt(localizer.Get(Localizer.RunSolutionCreate))
        );

        if (confirm)
        {
            OutputHelper.Info("creating solution...");

            // 具体创建逻辑
            var dto = new CreateSolutionDto
            {
                Name = settings.Name,
                Path = targetDirectory,
                IsLight = solutionType == Localizer.SolutionTypeMini,
                DBType =
                    dbType == localizer.Get(Localizer.DatabasePostgreSql) ? DBType.PostgreSQL : DBType.SQLServer,
                CacheType =
                    cacheType == localizer.Get(Localizer.CacheTypeHybrid) ? CacheType.Hybrid
                    : cacheType == localizer.Get(Localizer.CacheTypeMemory) ? CacheType.Memory
                    : CacheType.Redis,
                CommandDbConnStrings = string.Empty,
                QueryDbConnStrings = string.Empty,
                CacheConnStrings = string.Empty,
                FrontType = frontType == Localizer.None ? FrontType.None : FrontType.Angular,
            };

            if (dto.Path == "./")
            {
                dto.Path = Path.Combine(Environment.CurrentDirectory, dto.Path);
            }
            await commandService.CreateSolutionAsync(dto);
            OutputHelper.Success(localizer.Get(Localizer.CreateSolutionSuccess));
        }
        return 0;
    }
}
