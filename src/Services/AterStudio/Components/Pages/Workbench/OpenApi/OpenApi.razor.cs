using Ater.Common.Utils;
using CodeGenerator.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.OpenApi.Models;
using StudioMod.Models.ApiDocInfoDtos;

namespace AterStudio.Components.Pages.Workbench.OpenApi;

public partial class OpenApi
{
    [Inject]
    ApiDocInfoManager _manager { get; set; } = default!;

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
    string? ApiSearchKeyword { get; set; }
    string? ModelSearchKeyword { get; set; }

    string? SelectedNav { get; set; }

    bool IsLoading { get; set; } = true;
    bool IsFreshing { get; set; } = false;

    string ActiveId { get; set; } = "api";

    FluentTab? currentTab;
    FluentDialog? modelInfoDialog;
    bool ModelInfoDialogIsHidden { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetApiDocsAsync();
        if (Docs != null && Docs.Count > 0)
        {
            CurrentDoc = Docs[0];
            await GetDocContentAsync(false);
        }
        IsLoading = false;
    }

    private async Task GetApiDocsAsync()
    {
        var res = await _manager.FilterAsync(
            new ApiDocInfoFilterDto { PageSize = 999, ProjectId = ProjectContext.ProjectId }
        );
        Docs = res.Data;
    }

    private async Task GetDocContentAsync(bool isFresh)
    {
        if (CurrentDoc?.Id != null)
        {
            IsFreshing = true;
            await Task.Yield();
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
            IsFreshing = false;
        }
    }

    private void SelectApi(RestApiInfo api)
    {
        CurrentApi = api;
        // TODO: OperationId
        SelectedNav = api.Router;
    }

    private void SelectModel(TypeMeta model)
    {
        CurrentModel = model;
        SelectedNav = model.Name;
    }

    private void SearchApi()
    {
        if (!string.IsNullOrWhiteSpace(ApiSearchKeyword))
        {
            FilteredRestApiGroups = RestApiGroups
                .Where(group =>
                    (
                        group.Name?.Contains(ApiSearchKeyword, StringComparison.OrdinalIgnoreCase)
                        ?? false
                    )
                    || (
                        group.ApiInfos?.Any(api =>
                            (
                                api.Router?.Contains(
                                    ApiSearchKeyword,
                                    StringComparison.OrdinalIgnoreCase
                                ) ?? false
                            )
                            || (
                                api.Summary?.Contains(
                                    ApiSearchKeyword,
                                    StringComparison.OrdinalIgnoreCase
                                ) ?? false
                            )
                            || (
                                api.Tag?.Contains(
                                    ApiSearchKeyword,
                                    StringComparison.OrdinalIgnoreCase
                                ) ?? false
                            )
                        ) ?? false
                    )
                )
                .Select(group => new RestApiGroup
                {
                    Name = group.Name,
                    ApiInfos =
                        group
                            .ApiInfos?.Where(api =>
                                (
                                    api.Router?.Contains(
                                        ApiSearchKeyword,
                                        StringComparison.OrdinalIgnoreCase
                                    ) ?? false
                                )
                                || (api.Summary?.Contains(ApiSearchKeyword) ?? false)
                                || (api.Tag?.Contains(ApiSearchKeyword) ?? false)
                            )
                            .ToList() ?? [],
                })
                .ToList();
        }
        else
        {
            FilteredRestApiGroups = RestApiGroups;
        }
    }

    private void SearchModel()
    {
        if (!string.IsNullOrWhiteSpace(ModelSearchKeyword))
        {
            FilteredModelList = ModelList
                .Where(model =>
                    (
                        model.Name?.Contains(ModelSearchKeyword, StringComparison.OrdinalIgnoreCase)
                        ?? false
                    )
                    || (
                        model.Comment?.Contains(
                            ModelSearchKeyword,
                            StringComparison.OrdinalIgnoreCase
                        ) ?? false
                    )
                        && model.IsEnum == false
                )
                .ToList();
        }
        else
        {
            FilteredModelList = ModelList;
        }
    }

    private void OpenModelInfoDialog(string modelName)
    {
        var model = ModelList.Where(m => m.Name == modelName).FirstOrDefault();
        if (model != null)
        {
            SelectedModel = model;
            modelInfoDialog?.Hide();
            modelInfoDialog?.Show();
        }
        StateHasChanged();
        //ModelInfoDialogIsHidden = false;
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

    private async Task OpenRequestClientDialog()
    {
        if (CurrentDoc is null)
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
            return;
        }
        DialogParameters parameters = new()
        {
            Width = "500px",
            PreventScroll = true,
            Modal = true,
        };
        var dialog = await DialogService.ShowDialogAsync<RequestClientDialog>(
            CurrentDoc,
            parameters
        );
        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        if (result.Data is List<GenFileInfo> files)
        {
            // 发送全局通知
            MessageService.ShowMessageBar(options =>
            {
                options.Intent = MessageIntent.Success;
                options.Title = Lang(Localizer.Generate, Localizer.RequestClient);
                options.Body = string.Join("\n", files.Select(f => f.FullName));
                options.Timestamp = DateTime.Now;
                options.Section = App.MESSAGES_NOTIFICATION_CENTER;
            });
        }
        ToastService.ShowSuccess(Lang(Localizer.Generate, Localizer.Success), timeout: 3000);
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
        var color = type switch
        {
            OperationType.Get => "#318deb",
            OperationType.Post => "#14cc78",
            OperationType.Put => "#fca130",
            OperationType.Patch => "#fca130",
            OperationType.Delete => "#f93e3e",
            _ => "#888888",
        };

        return $"color:{color};font-weight:bold";
    }

    private async Task Refresh()
    {
        IsLoading = true;
        await this.GetDocContentAsync(true);
    }

    private void HandleOnTabChange(FluentTab tab)
    {
        currentTab = tab;
    }
}
