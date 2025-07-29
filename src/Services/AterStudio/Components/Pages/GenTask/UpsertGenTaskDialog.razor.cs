using Ater.Common.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using StudioMod.Models.GenActionDtos;
using StudioMod.Models.GenStepDtos;

namespace AterStudio.Components.Pages.GenTask;

public partial class UpsertGenTaskDialog : IDisposable
{
    [CascadingParameter]
    FluentDialog Dialog { get; set; } = null!;

    [Parameter]
    public GenActionItemDto? Content { get; set; }

    bool IsEdit => Content != null && Content.Id != Guid.Empty;

    [Inject]
    GenActionManager GenActionManager { get; set; } = null!;

    [Inject]
    GenStepManager GenStepManager { get; set; } = null!;

    EditContext? editContext;

    List<GenStepItemDto> GenSteps { get; set; } = [];
    List<GenStepItemDto> FilteredGenSteps { get; set; } = [];
    IEnumerable<GenStepItemDto> SelectedGenSteps { get; set; } = [];

    GenAction Model { get; set; } = new() { Name = string.Empty };
    IEnumerable<GenSourceType> SourceTypes { get; set; } = Enum.GetValues<GenSourceType>();

    protected override async Task OnInitializedAsync()
    {
        if (Content != null)
        {
            Model.Id = Content.Id;
            Model.Name = Content.Name;
            Model.Description = Content.Description;
            Model.SourceType = Content.SourceType;
            Model.Variables = Content.Variables ?? [];

            SelectedGenSteps = await GenActionManager.GetStepsAsync(Content.Id);
        }

        var page = await GenStepManager.ToPageAsync(
            new GenStepFilterDto { PageIndex = 1, PageSize = 100 }
        );
        GenSteps = page.Data ?? [];
        editContext = new EditContext(Model);
        editContext.OnFieldChanged += EditContext_OnFieldChanged;
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (editContext is not null)
        {
            editContext.Validate();
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        if (editContext != null)
            editContext.OnFieldChanged -= EditContext_OnFieldChanged;
    }

    private void AddVariable()
    {
        Model.Variables.Add(new Variable { Key = string.Empty, Value = string.Empty });
    }

    private void RemoveVariable(Variable variable)
    {
        Model.Variables.Remove(variable);
    }

    private void OnStepSearch(OptionsSearchEventArgs<GenStepItemDto> e)
    {
        e.Items = GenSteps
            .Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Name);
    }

    private async Task SaveAsync()
    {
        if (!editContext!.Validate())
        {
            return;
        }
        if (!SelectedGenSteps.Any())
        {
            ToastService.ShowError(Localizer.Get(Localizer.MustSelectItem, Localizer.Step));
            return;
        }
        Model.ProjectId = ProjectContext.ProjectId!.Value;
        if (IsEdit)
        {
            var entity = await GenActionManager.GetCurrentAsync(Model.Id);
            if (entity == null)
            {
                ToastService.ShowError(
                    Localizer.Get(Localizer.NotFoundWithName, Model.Id.ToString())
                );
                return;
            }
            entity.Merge(Model);
            var res = await GenActionManager.UpdateAsync(entity!);
            if (res)
            {
                // steps
                var stepIds = SelectedGenSteps.Select(s => s.Id).ToList();
                await GenActionManager.AddStepsAsync(Model.Id, stepIds);
                await Dialog.CloseAsync(Model);
            }
            else
            {
                ToastService.ShowError(Lang(Localizer.Edit, Localizer.Failed));
            }
        }
        else
        {
            Model.Variables = Model
                .Variables.Where(x =>
                    !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value)
                )
                .ToList();

            var res = await GenActionManager.AddAsync(Model);
            if (res)
            {
                // steps
                var stepIds = SelectedGenSteps.Select(s => s.Id).ToList();
                await GenActionManager.AddStepsAsync(Model.Id, stepIds);
                await Dialog.CloseAsync(Model);
            }
            else
            {
                ToastService.ShowError(Lang(Localizer.Add, Localizer.Failed));
            }
        }
    }

    private async Task CancelAsync()
    {
        await Dialog.CancelAsync();
    }
}

public class StepItemComparer : IEqualityComparer<GenStepItemDto>
{
    public static readonly StepItemComparer Instance = new();

    public bool Equals(GenStepItemDto? x, GenStepItemDto? y)
    {
        return ReferenceEquals(x, y) ? true
            : x is null || y is null ? false
            : x.Name == y.Name && x.TemplatePath == y.TemplatePath;
    }

    public int GetHashCode(GenStepItemDto obj) => HashCode.Combine(obj.Name, obj.TemplatePath);
}
