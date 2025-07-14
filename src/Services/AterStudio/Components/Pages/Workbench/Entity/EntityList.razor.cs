using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

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
    bool IsProcessing { get; set; }
    bool IsRefreshing { get; set; }

    bool IsCleaning { get; set; }
    bool BatchOpen { get; set; } = false;

    private FluentDialog _dialog = default!;
    private PaginationState pagination = new() { ItemsPerPage = 20 };
    private StandaloneCodeEditor editor = default!;

    private IQueryable<EntityFile>? EntityFiles { get; set; }
    private IQueryable<EntityFile>? FilteredEntityFiles { get; set; }
    private List<SubProjectInfo> Modules { get; set; } = [];
    private List<SubProjectInfo> Services { get; set; } = [];

    string moduleFilter = string.Empty;
    private StandaloneEditorConstructionOptions options = default!;
    string entityName = string.Empty;
    private bool Hidden { get; set; } = true;

    private string? SelectedModule { get; set; }

    public string? SearchModelKey { get; set; }

    private IEnumerable<EntityFile> SelectedEntity { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await GetEntityListAsync();
        GetModules();
        GetServices();
        isLoading = false;
        options = new StandaloneEditorConstructionOptions
        {
            Language = "csharp",
            Theme = "vs-dark",
            AutomaticLayout = true,
            ReadOnly = false,
            Minimap = new EditorMinimapOptions() { Enabled = false },
            FontSize = 14,
            LineNumbers = "on",
            ScrollBeyondLastLine = false,
            WordWrap = "on",
            WrappingStrategy = "advanced",
        };
        await base.OnInitializedAsync();
    }

    private void GetServices()
    {
        Services = SolutionManager.GetServices(false);
    }

    private async Task GetEntityListAsync()
    {
        IsRefreshing = true;
        await Task.Yield();
        var entityFiles = EntityInfoManager.GetEntityFiles(ProjectContext.EntityPath!);
        EntityFiles = entityFiles.AsQueryable();
        FilteredEntityFiles = EntityFiles;
        IsRefreshing = false;
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

    private void SelectModel(string? module = null)
    {
        SelectedModule = module;
        FilterEntityFiles();
    }

    private void FilterEntityFiles()
    {
        if (string.IsNullOrWhiteSpace(SelectedModule))
        {
            FilteredEntityFiles = EntityFiles;
        }
        else
        {
            FilteredEntityFiles = EntityFiles?.Where(e =>
                e.ModuleName != null
                && e.ModuleName.Equals(SelectedModule, StringComparison.OrdinalIgnoreCase)
            );
        }
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
        var parameters = new DialogParameters { Modal = false, Width = "560px" };
        var dialog = await DialogService.ShowDialogAsync<ServicesDialog>(parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            GetServices();
        }
    }

    private async Task OpenGenerateDialog(EntityFile entity, CommandType commandType)
    {
        var parameters = new DialogParameters
        {
            Modal = true,
            Width = "360px",
            ShowDismiss = false,
        };

        var data = new GenerateDialogData
        {
            CommandType = commandType,
            EntityPaths = [entity.FullName],
        };

        var dialog = await DialogService.ShowDialogAsync<GenerateDialog>(data, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            // TODO: Handle the result if needed

            ToastService.ShowSuccess(Lang(Localizer.Generate, Localizer.Success));
        }
    }

    private async Task DeleteModuleAsync()
    {
        if (!string.IsNullOrWhiteSpace(SelectedModule))
        {
            var dialog = await DialogService.ShowWarningAsync(
                Lang(Localizer.ConfirmDeleteMessage),
                Lang(Localizer.Delete, Localizer.Modules),
                Lang(Localizer.Confirm)
            );

            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                var res = SolutionManager.DeleteModule(SelectedModule);
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

    private async Task EditCodeAsync(EntityFile entity)
    {
        entityName = entity.Name;
        await editor.SetValue(entity.Content);
        Hidden = false;
    }

    private StandaloneEditorConstructionOptions SetCodeEditorOptions(StandaloneCodeEditor editor)
    {
        this.editor = editor;
        return options;
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
