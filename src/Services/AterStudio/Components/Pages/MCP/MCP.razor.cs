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
        await LoadMcpTools();
    }

    private async Task LoadMcpTools()
    {
        McpTools = await McpToolManager.ListAsync();
        SelectedMcpTool = null;
        StateHasChanged();
    }

    private void OpenAddDialog()
    {
        DialogContent = new McpTool
        {
            Name = string.Empty,
            Description = string.Empty,
            PromptPath = string.Empty,
        };
        DialogHidden = false;
    }

    private void OpenEditDialog(McpTool tool)
    {
        DialogContent = new McpTool
        {
            Id = tool.Id,
            Name = tool.Name,
            Description = tool.Description,
            PromptPath = tool.PromptPath,
            TemplatePaths = tool.TemplatePaths?.ToArray() ?? [],
            CreatedTime = tool.CreatedTime,
            UpdatedTime = tool.UpdatedTime,
            IsDeleted = tool.IsDeleted,
        };
        DialogHidden = false;
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
            await LoadMcpTools();
            ToastService.ShowSuccess("删除成功");
        }
    }

    private async Task OnDialogSaved(bool saved)
    {
        DialogHidden = true;
        if (saved)
        {
            await LoadMcpTools();
            ToastService.ShowSuccess("保存成功");
        }
    }
}
