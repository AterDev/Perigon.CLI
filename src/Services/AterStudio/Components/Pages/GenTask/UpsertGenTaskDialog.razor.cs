using Ater.Common.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using StudioMod.Models.GenActionDtos;

namespace AterStudio.Components.Pages.GenTask;

public partial class UpsertGenTaskDialog
{
    [CascadingParameter]
    FluentDialog Dialog { get; set; } = null!;

    [Parameter]
    public GenActionItemDto? Content { get; set; }

    bool IsEdit => Content != null && Content.Id != Guid.Empty;

    [Inject]
    GenActionManager GenActionManager { get; set; } = null!;

    EditContext? editContext;

    GenAction Model { get; set; } = new() { Name = string.Empty };
    IEnumerable<GenSourceType> SourceTypes { get; set; } = Enum.GetValues<GenSourceType>();

    protected override void OnInitialized()
    {
        if (Content != null)
        {
            Model.Id = Content.Id;
            Model.Name = Content.Name;
            Model.Description = Content.Description;
            Model.SourceType = Content.SourceType;
            Model.Variables = Content.Variables ?? new List<Variable>();
        }
        editContext = new EditContext(Model);
    }

    private void AddVariable()
    {
        Model.Variables.Add(new Variable { Key = string.Empty, Value = string.Empty });
    }

    private void RemoveVariable(Variable variable)
    {
        Model.Variables.Remove(variable);
    }

    private async Task SaveAsync()
    {
        if (!editContext!.Validate())
        {
            ToastService.ShowError(Lang(Localizer.FormValidFailed));
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
            entity = entity.Merge(Model);
            var res = await GenActionManager.UpdateAsync(entity!);
            if (res)
            {
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
