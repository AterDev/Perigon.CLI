using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.OpenApi.Models;
using StudioMod.Models.ApiDocInfoDtos;

namespace AterStudio.Components.Pages.Workbench.OpenApi;

public partial class OpenApi
{
    [Inject]
    private ApiDocInfoManager _manager { get; set; } = default!;

    List<ApiDocInfoItemDto> Docs { get; set; } = [];
    ApiDocInfoItemDto? CurrentDoc { get; set; }

    List<RestApiGroup> RestApiGroups { get; set; } = [];
    List<RestApiGroup> FilteredRestApiGroups { get; set; } = [];

    List<TypeMeta> ModelList { get; set; } = [];
    List<TypeMeta> FilteredModelList { get; set; } = [];

    List<ApiDocTag> Tags { get; set; } = [];

    RestApiInfo? CurrentApi { get; set; }
    TypeMeta? CurrentModel { get; set; }
    TypeMeta? SelectedModel { get; set; }
    string? SearchKeyword { get; set; }

    bool isLoading = true;
    bool isSync = false;

    private string? activeId = "api";
    private FluentTab? currentTab;

    protected override async Task OnInitializedAsync()
    {
        await GetApiDocsAsync();
        if (Docs != null && Docs.Count > 0)
        {
            CurrentDoc = Docs[0];
            await GetDocContentAsync(false);
        }
    }

    private async Task GetApiDocsAsync()
    {
        var res = await _manager.FilterAsync(
            new ApiDocInfoFilterDto { PageSize = 999, ProjectId = ProjectContext.ProjectId }
        );
        Docs = res.Data;
        isLoading = false;
    }

    private async Task GetDocContentAsync(bool isFresh)
    {
        if (CurrentDoc?.Id != null)
        {
            var res = await _manager.GetContentAsync(CurrentDoc.Id, isFresh);

            if (res is not null)
            {
                RestApiGroups = res?.RestApiGroups ?? [];
                FilteredRestApiGroups = RestApiGroups;
                ModelList = res?.TypeMeta ?? [];
                FilteredModelList = ModelList;
                Tags = res?.OpenApiTags ?? [];
            }
            if (CurrentApi is not null)
            {
                CurrentApi = RestApiGroups
                    .SelectMany(g => g.ApiInfos ?? [])
                    .FirstOrDefault(a => a.Router == CurrentApi?.Router);
            }
            isLoading = false;
        }
    }

    private async Task OpenAddOpenApiDialog()
    {
        DialogParameters parameters = new()
        {
            Width = "400px",
            PreventScroll = true,
            Modal = true,
        };

        var dialog = await DialogService.ShowDialogAsync<AddOpenApiDialog>(parameters);
        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        await GetApiDocsAsync();
    }

    private async Task OpenEditOpenApiDialog()
    {
        if (CurrentDoc is null)
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
            return;
        }
        DialogParameters parameters = new()
        {
            Width = "400px",
            PreventScroll = true,
            Modal = true,
        };

        var dialog = await DialogService.ShowDialogAsync<EditOpenApiDialog>(CurrentDoc, parameters);
        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        await GetApiDocsAsync();
        StateHasChanged();
    }

    private async Task DeleteOpenApiAsync()
    {
        if (CurrentDoc is null)
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
            return;
        }
        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(Localizer.ConfirmDeleteMessage),
            Lang(Localizer.Delete),
            Lang(Localizer.Cancel)
        );

        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            var res = await _manager.DeleteAsync([CurrentDoc.Id], false);
            if (res)
            {
                ToastService.ShowSuccess(Lang(Localizer.Delete, Localizer.Success));
                await GetApiDocsAsync();
                CurrentDoc = null;
                RestApiGroups.Clear();
                ModelList.Clear();
                FilteredModelList.Clear();
                Tags.Clear();
            }
            else
            {
                ToastService.ShowError(Lang(Localizer.Delete, Localizer.Failed));
            }
        }
    }

    private String GetApiTip(RestApiInfo api)
    {
        return $"{nameof(api.OperationType).ToUpper()} {api.Router}";
    }

    private string GetApiTypeColor(OperationType type)
    {
        return type switch
        {
            OperationType.Get => "#318deb",
            OperationType.Post => "#14cc78",
            OperationType.Put => "#fca130",
            OperationType.Patch => "#fca130",
            OperationType.Delete => "#f93e3e",
            _ => "#888888",
        };
    }

    private async Task Refresh()
    {
        isLoading = true;
        await this.GetDocContentAsync(true);
    }

    private void HandleOnTabChange(FluentTab tab)
    {
        currentTab = tab;
    }
}
