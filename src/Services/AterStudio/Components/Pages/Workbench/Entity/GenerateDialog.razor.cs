using CodeGenerator.Models;
using Microsoft.AspNetCore.Components;

namespace AterStudio.Components.Pages.Workbench.Entity;

public partial class GenerateDialog
{
    [CascadingParameter]
    FluentDialog Dialog { get; set; } = null!;

    [Parameter]
    public GenerateDialogData Content { get; set; } = default!;
    GenerateDto GenerateDto { get; set; } = default!;

    [Inject]
    EntityInfoManager EntityInfoManager { get; set; } = default!;

    [Inject]
    SolutionManager SolutionManager { get; set; } = default!;

    List<SubProjectInfo> Services { get; set; } = [];
    IEnumerable<SubProjectInfo> SelectedServices { get; set; } = [];
    string? SelectedValue;
    bool IsProcessing { get; set; }

    protected override void OnInitialized()
    {
        GenerateDto = new GenerateDto
        {
            EntityPath = string.Empty,
            CommandType = Content.CommandType,
            Force = false,
        };
        GetServices();
    }

    private void GetServices()
    {
        Services = SolutionManager.GetServices(true);
    }

    private async Task GenerateAsync()
    {
        if (Content.EntityPaths == null || Content.EntityPaths.Length == 0)
        {
            ToastService.ShowError("No entity paths specified.");
            return;
        }
        // selectedServices is required when generate controller
        if (Content.CommandType == CommandType.API)
        {
            if (SelectedServices.Count() == 0)
            {
                ToastService.ShowError("Please select a service.");
                return;
            }
        }

        IsProcessing = true;
        await Task.Yield();
        GenerateDto.ServicePath = SelectedServices
            .Where(s => Path.GetDirectoryName(s.Path) != null)
            .Select(s => Path.GetDirectoryName(s.Path)!)
            .ToArray();

        List<GenFileInfo> resFiles = [];
        try
        {
            foreach (var entityPath in Content.EntityPaths)
            {
                GenerateDto.EntityPath = entityPath;
                var res = await EntityInfoManager.GenerateAsync(GenerateDto);
                resFiles.AddRange(res);
            }
            IsProcessing = false;
            await Dialog.CloseAsync(resFiles);
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Generate failed: {ex.Message}");
        }
        IsProcessing = false;
    }

    private async Task CancelAsync()
    {
        await Dialog.CancelAsync();
    }
}

public class GenerateDialogData
{
    public CommandType CommandType { get; set; }
    public string[] EntityPaths { get; set; } = [];
}
