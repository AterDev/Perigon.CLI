using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace AterStudio.Components.Pages.Workbench.Entity;

public partial class EntityList
{
    [Inject]
    private IProjectContext project { get; set; } = default!;

    [Inject]
    private EntityInfoManager entityInfoManager { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }

    FluentDataGrid<EntityFile> grid = default!;
    FluentDialog _dialog = default!;
    PaginationState pagination = new() { ItemsPerPage = 50 };
    StandaloneCodeEditor editor = default!;

    private IQueryable<EntityFile>? files;
    string nameFilter = string.Empty;
    private StandaloneEditorConstructionOptions options = default!;
    string entityName = string.Empty;
    private bool Hidden { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        if (Guid.TryParse(Id, out var id))
        {
            await project.SetProjectByIdAsync(Id);
            GetEntityList();
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

    private void GetEntityList()
    {
        var entityFiles = entityInfoManager.GetEntityFiles(project.EntityPath!);
        files = entityFiles.AsQueryable();
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
