using CoreMod.Services;
using Share;
using Share.Helper;

namespace CommandLine.Commands;

/// <summary>
/// Compares the current project with a freshly generated latest Perigon template.
/// </summary>
public sealed class UpdateCommand(
    TemplateUpdateService templateUpdateService,
    SolutionContext projectContext,
    Localizer localizer
)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!CommandSolutionHelper.TrySetSolution(projectContext, localizer))
        {
            return 1;
        }

        OutputHelper.Warning(localizer.Get(Localizer.UpdateRiskWarning));
        OutputHelper.Info(localizer.Get(Localizer.UpdatePreparing));

        try
        {
            using var plan = await templateUpdateService.CreatePlanAsync(cancellationToken);
            if (plan.Changes.Count == 0)
            {
                OutputHelper.Info(localizer.Get(Localizer.UpdateNoChanges));
                return 0;
            }

            var selection = await TemplateUpdateSelector.SelectAsync(
                AnsiConsole.Console,
                plan.Changes,
                localizer,
                cancellationToken
            );
            if (!selection.ShouldApply)
            {
                OutputHelper.Info(localizer.Get(Localizer.UpdateCancelled));
                return 0;
            }

            await templateUpdateService.ApplyAsync(
                plan,
                selection.Changes,
                cancellationToken
            );
            OutputHelper.Info(localizer.Get(Localizer.UpdateBuilding));
            await templateUpdateService.BuildAsync(plan, cancellationToken);
            OutputHelper.Success(localizer.Get(Localizer.UpdateSuccess, selection.Changes.Count));
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OutputHelper.Warning(localizer.Get(Localizer.UpdateCancelled));
            return 1;
        }
        catch (Exception ex)
        {
            OutputHelper.Error(localizer.Get(Localizer.UpdateFailed, ex.Message));
            return 1;
        }
    }

}
