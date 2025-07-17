namespace AterStudio.Components.Pages.GenTask;

using CodeGenerator.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using StudioMod.Models.GenActionDtos;
using StudioMod.Models.GenStepDtos;

public partial class GenTask
{
    [Inject]
    GenActionManager GenActionManager { get; set; } = default!;

    [Inject]
    GenStepManager GenStepManager { get; set; } = default!;

    List<DataFile> Directories { get; set; } = [];
    List<DataFile> Files { get; set; } = [];
    string? SelectedDirectory { get; set; }
    DataFile? SelectedFile { get; set; }

    private bool isLoading = true;
    private List<GenActionItemDto> GenActions { get; set; } = new();
    private GenActionItemDto? SelectedAction { get; set; }
    private List<GenStepItemDto> GenSteps { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadActionsAsync();
        isLoading = false;
    }

    private async Task LoadActionsAsync()
    {
        var page = await GenActionManager.ToPageAsync(new GenActionFilterDto());
        // 假设分页属性为 Records 或 Data
        GenActions = page.Data ?? new List<GenActionItemDto>();
        if (GenActions.Count > 0)
        {
            SelectedAction = GenActions[0];
            await LoadStepsAsync();
        }
        else
        {
            SelectedAction = null;
            GenSteps.Clear();
        }
    }

    private async Task SelectAction(GenActionItemDto action)
    {
        SelectedAction = action;
        await LoadStepsAsync();
    }

    private async Task LoadStepsAsync()
    {
        if (SelectedAction != null)
        {
            GenSteps = await GenActionManager.GetStepsAsync(SelectedAction.Id) ?? new();
        }
        else
        {
            GenSteps.Clear();
        }
    }

    private async Task OpenAddActionDialogAsync()
    {
        var parameters = new DialogParameters();
        var dialog = await DialogService.ShowDialogAsync<UpsertGenAction>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await LoadActionsAsync();
        }
    }

    private async Task OpenEditActionDialogAsync()
    {
        if (SelectedAction == null)
            return;
        var parameters = new DialogParameters { { "Model", SelectedAction } };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenAction>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await LoadActionsAsync();
        }
    }

    private async Task DeleteActionAsync()
    {
        if (SelectedAction == null)
            return;
        await GenActionManager.DeleteAsync(new List<Guid> { SelectedAction.Id }, false);
        await LoadActionsAsync();
    }

    private async Task OpenAddStepDialogAsync()
    {
        if (SelectedAction == null)
            return;
        var parameters = new DialogParameters { { "ActionId", SelectedAction.Id } };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenStep>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await LoadStepsAsync();
        }
    }

    private async Task OpenEditStepDialogAsync(GenStepItemDto step)
    {
        var parameters = new DialogParameters { { "Model", step } };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenStep>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await LoadStepsAsync();
        }
    }

    private async Task DeleteStepAsync(GenStepItemDto step)
    {
        await GenStepManager.DeleteAsync(new List<Guid> { step.Id }, false);
        await LoadStepsAsync();
    }
}
