using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace AterStudio.Components.Pages.Workbench.Entity;

public partial class EntityList
{
    [Inject]
    private IProjectContext ProjectContext { get; set; } = default!;

    [Inject]
    private EntityInfoManager EntityInfoManager { get; set; } = default!;

    [Inject]
    private SolutionManager SolutionManager { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }

    private FluentDataGrid<EntityFile> grid = default!;
    private FluentDialog _dialog = default!;
    private PaginationState pagination = new() { ItemsPerPage = 50 };
    private StandaloneCodeEditor editor = default!;

    private IQueryable<EntityFile>? EntityFiles { get; set; }
    private List<SubProjectInfo> Modules { get; set; } = [];
    private List<SubProjectInfo> Services { get; set; } = [];

    string nameFilter = string.Empty;
    private StandaloneEditorConstructionOptions options = default!;
    string entityName = string.Empty;
    private bool Hidden { get; set; } = true;

    private string? SelectedModule { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Guid.TryParse(Id, out var id))
        {
            await ProjectContext.SetProjectByIdAsync(Id);
            GetEntityList();
            GetModules();
            GetServices();
        }
        else
        {
            ToastService.ShowError("invalid project id");
        }

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
    }

    private void GetServices()
    {
        Services = SolutionManager.GetServices();
        Console.WriteLine(ToJson(Services));
    }

    private void GetEntityList()
    {
        var entityFiles = EntityInfoManager.GetEntityFiles(ProjectContext.EntityPath!);
        EntityFiles = entityFiles.AsQueryable();
    }

    private void GetModules()
    {
        Modules = SolutionManager.GetModules();
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
}
