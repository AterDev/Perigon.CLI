using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace AterStudio.Components.Pages.Workbench.OpenApi;

public partial class OpenApi
{
    [Inject]
    private IProjectContext ProjectContext { get; set; } = default!;

    private string? activeId = "api";
    private FluentTab? currentTab;

    protected override async Task OnInitializedAsync() { }

    private void HandleOnTabChange(FluentTab tab)
    {
        currentTab = tab;
    }
}
