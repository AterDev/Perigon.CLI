using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AterStudio.Components.Pages.Home;

public partial class Home
{
    List<Solution> solutions = [];

    private async Task AddLocalProject(MouseEventArgs arg)
    {
        DialogParameters parameters = new()
        {
            Width = "400px",
            PreventScroll = true,
            Modal = false,
        };

        var dialog = await DialogService.ShowDialogAsync<AddProjectDialog>(parameters);

        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        await GetProjectListAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetProjectListAsync();
    }

    private void OpenSolution(Solution solution)
    {
        var path = Path.Combine(solution.Path, solution.Name);
        SolutionManager.OpenSolution(path);
    }

    private async Task GetProjectListAsync()
    {
        solutions = await SolutionManager.ListAsync();
    }

    private async Task OpenConfigDialogAsync(Solution project)
    {
        DialogParameters parameters = new()
        {
            Width = "400px",
            Modal = true,
        };
        var dialog = await DialogService.ShowDialogAsync<ConfigProjectDialog>(project, parameters);
        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        ToastService.ShowSuccess(Lang(Localizer.Save, Localizer.Success));
        await GetProjectListAsync();
    }

    private async Task DeleteProject(Solution project)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            LangWithArguments(Localizer.ConfirmDeleteMessage, Lang(Localizer.Project)),
            primaryText: Lang(Localizer.Yes),
            secondaryText: Lang(Localizer.No),
            title: Lang(Localizer.Delete, Localizer.Project)
        );

        var result = await dialog.Result;
        if (result.Cancelled)
            return;
        await SolutionManager.DeleteAsync([project.Id], false);
        await GetProjectListAsync();
    }

    private async Task ToSolution(Guid id)
    {
        await ProjectContext.SetProjectByIdAsync(id);
        NavigationManager.NavigateTo($"/workbench/entity");
    }
}
