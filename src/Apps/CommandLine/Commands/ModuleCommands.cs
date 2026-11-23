using CoreMod.Services;
using Share;
using Share.Helper;

namespace CommandLine.Commands;

public class ModuleListCommand(
    OfficialModuleService officialModuleService,
    Localizer localizer
)
{
    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        return ModuleCommandHelper.ShowOfficialModulesAsync(
            officialModuleService,
            localizer,
            cancellationToken
        );
    }
}

internal static class ModuleCommandHelper
{
    public static async Task<int> ShowOfficialModulesAsync(
        OfficialModuleService officialModuleService,
        Localizer localizer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var modules = await officialModuleService.GetOfficialModulesAsync(cancellationToken);
            if (modules.Count == 0)
            {
                OutputHelper.Warning(localizer.Get(Localizer.OfficialModuleListEmpty));
                return 0;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.Title = new TableTitle(localizer.Get(Localizer.OfficialModuleListTitle));
            table.AddColumn(localizer.Get(Localizer.Name));
            table.AddColumn(localizer.Get(Localizer.Version));
            table.AddColumn(localizer.Get(Localizer.Author));
            table.AddColumn(localizer.Get(Localizer.Description));

            foreach (var module in modules)
            {
                table.AddRow(
                    Markup.Escape($"Perigon.{module.ModuleName}"),
                    Markup.Escape(module.Version),
                    Markup.Escape(module.Author ?? "-"),
                    Markup.Escape(module.Description ?? "-")
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
        catch (Exception ex)
        {
            OutputHelper.Error(ex.Message);
            return 1;
        }
    }
}
