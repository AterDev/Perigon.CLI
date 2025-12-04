using CodeGenerator.Models;
using Microsoft.AspNetCore.Components;

namespace AterStudio.Components.Pages.Workbench.Entity;

public partial class EntityList
{
    [Inject]
    private EntityInfoManager EntityInfoManager { get; set; } = default!;

    [Inject]
    private SolutionManager SolutionManager { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }
    bool isLoading = true;
    bool IsRefreshing { get; set; }

    bool IsModuleDeleting { get; set; }
    bool IsCleaning { get; set; }
    bool BatchOpen { get; set; }

    private PaginationState pagination = new() { ItemsPerPage = 20 };

    private IQueryable<EntityFile>? EntityFiles { get; set; }
    private IQueryable<EntityFile>? FilteredEntityFiles { get; set; }
    private List<SubProjectInfo> Modules { get; set; } = [];
    private List<SubProjectInfo> Services { get; set; } = [];

    private string? SelectedModule { get; set; }

    public string? SearchModelKey { get; set; }

    private IEnumerable<EntityFile> SelectedEntity { get; set; } = [];

    [CascadingParameter]
    public CancellationToken ComponentCancellationToken { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CheckProject(); await GetEntityListAsync();
        GetModules();
        GetServices();
        isLoading = false;
    }

    private void GetServices()
    {
        Services = SolutionManager.GetServices(false);
    }

    private async Task GetEntityListAsync(bool forceRefresh = false)
    {
        if (ComponentCancellationToken.IsCancellationRequested)
            return;

        IsRefreshing = true;
        await Task.Yield();
        try
        {
            if (ComponentCancellationToken.IsCancellationRequested)
                return;

            var entityFiles = EntityInfoManager.GetEntityFiles(
                ProjectContext.EntityPath!,
                forceRefresh
            );
            EntityFiles = entityFiles.AsQueryable();
            FilteredEntityFiles = EntityFiles;
        }
        catch (OperationCanceledException)
        {
            // 操作被取消
        }
        catch (ObjectDisposedException)
        {
            // DbContext 已释放
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task CleanSolutionAsync()
    {
        IsCleaning = true;
        await Task.Yield();
        var result = SolutionManager.CleanSolution();
        if (!result)
        {
            ToastService.ShowError(SolutionManager.ErrorMsg);
        }
        else
        {
            ToastService.ShowSuccess(Lang(Localizer.Clean, Localizer.Success));
        }
        IsCleaning = false;
    }

    private void SelectModule(string? module = null)
    {
        SelectedModule = module;
        FilterEntityFiles();
    }

    private void FilterEntityFiles()
    {
        FilteredEntityFiles = string.IsNullOrWhiteSpace(SelectedModule)
            ? EntityFiles
            : (
                EntityFiles?.Where(e =>
                    e.ModuleName != null
                    && e.ModuleName.Equals(SelectedModule, StringComparison.OrdinalIgnoreCase)
                )
            );
    }

    private void GetModules()
    {
        Modules = SolutionManager.GetModules();
    }

    private async Task OpenAddModuleDialogAsync()
    {
        var parameters = new DialogParameters { Modal = false, Width = "320px" };
        var dialog = await DialogService.ShowDialogAsync<AddModuleDialog>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Add, Localizer.Success));
            GetModules();
        }
    }

    private async Task OpenServicesDialog()
    {
        var parameters = new DialogParameters { Modal = true, Width = "560px" };
        var dialog = await DialogService.ShowDialogAsync<ServicesDialog>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            GetServices();
        }
    }

    private async Task OpenGenerateDialog(CommandType commandType, EntityFile? entity = null)
    {
        var parameters = new DialogParameters
        {
            Modal = true,
            Width = "360px",
            ShowDismiss = false,
        };
        var data = new GenerateDialogData { CommandType = commandType };

        if (entity is not null)
        {
            data.EntityPaths = [entity.FullName];
        }
        else if (SelectedEntity.Any())
        {
            data.EntityPaths = SelectedEntity.Select(e => e.FullName).ToArray();
        }

        var dialog = await DialogService.ShowDialogAsync<GenerateDialog>(data, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            if (result.Data is List<GenFileInfo> files)
            {
                // 发送全局通知
                MessageService.ShowMessageBar(options =>
                {
                    options.Intent = MessageIntent.Success;
                    options.Title = Lang(Localizer.Generate) + commandType.ToString();
                    options.Body = string.Join(
                        "\n",
                        files.Select(f => f.FullName.Replace(ProjectContext.SolutionPath ?? "", ""))
                    );
                    options.Timestamp = DateTime.Now;
                    options.Section = App.MESSAGES_NOTIFICATION_CENTER;
                });
            }
            ToastService.ShowSuccess(Lang(Localizer.Generate, Localizer.Success), timeout: 3000);
        }
    }

    private async Task DeleteModuleAsync()
    {
        if (!string.IsNullOrWhiteSpace(SelectedModule))
        {
            var dialog = await DialogService.ShowConfirmationAsync(
                Lang(Localizer.DeleteModuleDescription),
                Lang(Localizer.Confirm),
                Lang(Localizer.Cancel),
                LangWithArguments(Localizer.ConfirmDeleteMessage, Lang(Localizer.Modules))
            );

            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                if (IsModuleDeleting)
                {
                    return;
                }
                IsModuleDeleting = true;
                var res = SolutionManager.DeleteModule(SelectedModule);
                IsModuleDeleting = false;
                if (res)
                {
                    ToastService.ShowSuccess(Lang(Localizer.Delete, Localizer.Success));
                    GetModules();
                }
                else
                {
                    ToastService.ShowError(SolutionManager.ErrorMsg);
                }
            }
        }
        else
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
        }
    }

    private void OnSearch()
    {
        if (!string.IsNullOrWhiteSpace(SearchModelKey) && EntityFiles is not null)
        {
            FilteredEntityFiles = EntityFiles.Where(e =>
                e.Name.Contains(SearchModelKey, StringComparison.OrdinalIgnoreCase)
                || (
                    e.Comment != null
                    && e.Comment.Contains(SearchModelKey, StringComparison.OrdinalIgnoreCase)
                )
            );
        }
    }
}
