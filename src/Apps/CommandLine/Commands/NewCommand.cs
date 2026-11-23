using CoreMod.Services;
using Share;
using Share.Helper;
using Share.Models;
using Share.Models.CommandDtos;
using Share.Services;
using System.Net.Http;

namespace CommandLine.Commands;

public class NewCommand(
    Localizer localizer,
    CommandService commandService,
    OfficialModuleService officialModuleService
)
{
    public class Settings
    {
        public required string Name { get; set; }
    }

    public async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine();
        // 1. 选择项目模板类型
        var solutionType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(localizer.Get(Localizer.SelectSolutionType))
                .AddChoices([
                    localizer.Get(Localizer.SolutionTypeStandard),
                    localizer.Get(Localizer.SolutionTypeMini)
                ])
        );
        var isLight = solutionType == localizer.Get(Localizer.SolutionTypeMini);

        // 2. 选择数据库类型
        var dbTypePrompt = new SelectionPrompt<string>()
            .Title(localizer.Get(Localizer.SelectDatabaseProvider));

        if (isLight)
        {
            dbTypePrompt.AddChoice(localizer.Get(Localizer.DatabasePostgreSql));
        }
        else
        {
            dbTypePrompt.AddChoices(
                localizer.Get(Localizer.DatabasePostgreSql),
                localizer.Get(Localizer.DatabaseSqlServer));
        }

        var dbType = AnsiConsole.Prompt(dbTypePrompt);

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

        // 8. 官方模块选择(多选)
        List<PackageMetadata> selectedOfficialModules = [];
        if (!isLight)
        {
            var officialModules = await GetOfficialModulesAsync(cancellationToken);
            selectedOfficialModules = officialModules.Count == 0
                ? []
                : AnsiConsole.Prompt(
                    new MultiSelectionPrompt<PackageMetadata>()
                        .Title(localizer.Get(Localizer.SelectOfficialModules))
                        .InstructionsText(localizer.Get(Localizer.CommandSelectTip))
                        .NotRequired()
                        .AddChoices(officialModules)
                        .UseConverter(FormatOfficialModuleOption)
                );
        }

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
            .AddRow(
                localizer.Get(Localizer.FeatureModules),
                selectedOfficialModules.Count == 0
                    ? localizer.Get(Localizer.None)
                    : string.Join(", ", selectedOfficialModules.Select(m => $"Perigon.{m.ModuleName}"))
            )
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
                IsLight = isLight,
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
                OfficialModules = selectedOfficialModules
                    .Select(m => $"Perigon.{m.ModuleName}")
                    .ToList(),
            };

            if (dto.Path == "./")
            {
                dto.Path = Path.Combine(Environment.CurrentDirectory, dto.Path);
            }
            var success = await commandService.CreateSolutionAsync(dto);
            if (!success)
            {
                return 1;
            }
            OutputHelper.Success(localizer.Get(Localizer.CreateSolutionSuccess));
        }
        return 0;
    }

    private async Task<IReadOnlyList<PackageMetadata>> GetOfficialModulesAsync(
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await officialModuleService.GetOfficialModulesAsync(cancellationToken);
        }
        catch (Exception ex) when (ContainsHttpRequestException(ex))
        {
            OutputHelper.Warning(localizer.Get(Localizer.GitHubConnectionFailed));
            return [];
        }
        catch (Exception ex)
        {
            OutputHelper.Warning(ex.Message);
            return [];
        }
    }

    private static bool ContainsHttpRequestException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is HttpRequestException)
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatOfficialModuleOption(PackageMetadata module)
    {
        var name = string.IsNullOrWhiteSpace(module.DisplayName)
            ? module.ModuleName
            : module.DisplayName;
        var description = string.IsNullOrWhiteSpace(module.Description)
            ? "-"
            : module.Description;

        return $"{name} ({module.Version}) - {description}";
    }
}
