using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;

namespace AterStudio.Components.Pages.Home;
public partial class Home
{
    List<Project> projects = [];

    private async Task AddLocalProject(MouseEventArgs arg)
    {
        DialogParameters parameters = new()
        {
            Width = "400px",
            PreventScroll = true,
            Modal = true,
        };

        var dialog = await DialogService.ShowDialogAsync<AddProjectDialog>(parameters);

        var result = await dialog.Result;
        if (result.Cancelled) return;

        await GetProjectListAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetProjectListAsync();
    }

    private async Task GetProjectListAsync()
    {
        projects = await ProjectManager.ListAsync();
    }

    private async Task DeleteProject(Project project)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(Localizer.ConfirmDeleteMessage),
            primaryText: Lang(Localizer.Yes), secondaryText: Lang(Localizer.No),
            title: Lang(Localizer.Delete, Localizer.Project));

        var result = await dialog.Result;
        if (result.Cancelled) return;
        await ProjectManager.DeleteAsync([project.Id], false);
        await GetProjectListAsync();
    }
}