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
    private List<GenActionItemDto> GenActions { get; set; } = [];
    private GenActionItemDto? SelectedAction { get; set; }
    private List<GenStepItemDto> GenSteps { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        CheckProject();
        await LoadActionsAsync();
        await LoadStepsAsync();
        isLoading = false;
    }

    private async Task LoadActionsAsync()
    {
        var page = await GenActionManager.ToPageAsync(
            new GenActionFilterDto { PageIndex = 1, PageSize = 100 }
        );
        GenActions = page.Data ?? [];
        if (GenActions.Count > 0)
        {
            SelectedAction = GenActions[0];
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
        var page = await GenStepManager.ToPageAsync(
            new GenStepFilterDto { PageIndex = 1, PageSize = 100 }
        );
        GenSteps = page.Data ?? [];
    }

    private async Task OpenAddActionDialogAsync()
    {
        var parameters = new DialogParameters { Modal = false };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenTaskDialog>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Save, Localizer.Success));
            await LoadActionsAsync();
        }
    }

    private async Task OpenEditActionDialogAsync(GenActionItemDto item)
    {
        if (SelectedAction == null)
            return;
        var parameters = new DialogParameters { Modal = false };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenTaskDialog>(item, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Save, Localizer.Success));
            await LoadActionsAsync();
        }
    }

    private async Task OpenExecuteTaskDialog(GenActionItemDto item)
    {
        if (item == null)
            return;
        var parameters = new DialogParameters { Modal = false };

        var dialog = await DialogService.ShowDialogAsync<ExecuteTaskDialog>(item, parameters);
        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        if (result.Data is List<ModelFileItemDto> data)
        {
            ToastService.ShowSuccess(Lang(Localizer.Execute, Localizer.Success));
            MessageService.ShowMessageBar(options =>
            {
                options.Intent = MessageIntent.Success;
                options.Title = Lang(Localizer.Execute) + item.Name;
                options.Body = string.Join(
                    "\n",
                    data.Select(f => f.FullName.Replace(ProjectContext.SolutionPath ?? "", ""))
                );
                options.Timestamp = DateTime.Now;
                options.Section = App.MESSAGES_NOTIFICATION_CENTER;
            });
        }
    }

    private async Task DeleteActionAsync(GenActionItemDto item)
    {
        if (item == null)
            return;

        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(Localizer.ConfirmDeleteMessage),
            Lang(Localizer.Confirm),
            Lang(Localizer.Cancel),
            Lang(Localizer.Delete, Localizer.Task)
        );
        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        var res = await GenActionManager.DeleteAsync([item.Id], false);
        if (res)
        {
            ToastService.ShowSuccess(Lang(Localizer.Delete, Localizer.Success));
            await LoadActionsAsync();
        }
    }

    private async Task OpenAddStepDialogAsync()
    {
        //if (SelectedAction == null)
        //    return;
        var parameters = new DialogParameters { Modal = false };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenStepDialog>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Add, Localizer.Success));
            await LoadStepsAsync();
        }
    }

    private async Task OpenEditStepDialogAsync(GenStepItemDto step)
    {
        var parameters = new DialogParameters { Modal = false };
        var dialog = await DialogService.ShowDialogAsync<UpsertGenStepDialog>(step, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Edit, Localizer.Success));
            await LoadStepsAsync();
        }
    }

    private async Task DeleteStepAsync(GenStepItemDto step)
    {
        if (step == null)
            return;

        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(Localizer.ConfirmDeleteMessage),
            Lang(Localizer.Confirm),
            Lang(Localizer.Cancel),
            Lang(Localizer.Delete, Localizer.Step)
        );
        var result = await dialog.Result;
        if (result.Cancelled)
            return;
        var res = await GenStepManager.DeleteAsync([step.Id], false);
        if (res)
        {
            ToastService.ShowSuccess(Lang(Localizer.Delete, Localizer.Success));
            await LoadActionsAsync();
        }
    }
}
