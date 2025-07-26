using Microsoft.AspNetCore.Components;

namespace AterStudio.Components.Pages.MCP;

public partial class MCP
{
    private List<McpTool> McpTools { get; set; } = [];
    private McpTool? SelectedMcpTool { get; set; }
    private bool DialogHidden { get; set; } = true;
    private McpTool DialogContent { get; set; } =
        new McpTool
        {
            Name = string.Empty,
            Description = string.Empty,
            PromptPath = string.Empty,
        };

    [Inject]
    private McpToolManager McpToolManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadMcpToolsAsync();
    }

    private async Task LoadMcpToolsAsync()
    {
        McpTools = await McpToolManager.ListAsync();

        Console.WriteLine(ToJson(McpTools));
        SelectedMcpTool = null;
        StateHasChanged();
    }

    private async Task OpenAddDialogAsync()
    {
        var dialog = await DialogService.ShowDialogAsync<UpsertMcpToolDialog>(
            new DialogParameters { Width = "400px", Modal = false }
        );
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Add, Localizer.Success));
            await LoadMcpToolsAsync();
        }
    }

    private async Task OpenEditDialogAsync(McpTool tool)
    {
        var dialog = await DialogService.ShowDialogAsync<UpsertMcpToolDialog>(
            tool,
            new DialogParameters { Width = "400px", Modal = false }
        );
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Edit, Localizer.Success));
            await LoadMcpToolsAsync();
        }
    }

    private async Task DeleteMcpTool(McpTool tool)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            "删除MCP工具",
            "确认",
            "取消",
            "确认要删除该工具吗？"
        );
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await McpToolManager.DeleteAsync([tool.Id], false);
            await LoadMcpToolsAsync();
            ToastService.ShowSuccess("删除成功");
        }
    }

    private async Task OnDialogSaved(bool saved)
    {
        DialogHidden = true;
        if (saved)
        {
            await LoadMcpToolsAsync();
            ToastService.ShowSuccess("保存成功");
        }
    }
}
