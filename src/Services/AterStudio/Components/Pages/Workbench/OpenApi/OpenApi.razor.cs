using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace AterStudio.Components.Pages.Workbench.OpenApi;

public partial class OpenApi
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private IProjectContext ProjectContext { get; set; } = default!;

    string? activeid = "api";
    FluentTab? changedto;

    protected override async Task OnInitializedAsync()
    {
        await ProjectContext.SetProjectByIdAsync(Id.ToString());
    }

    private void HandleOnTabChange(FluentTab tab)
    {
        changedto = tab;
    }
}
