using CodeGenerator.Models;
using Microsoft.AspNetCore.Components;
using StudioMod.Models.ApiDocInfoDtos;

namespace AterStudio.Components.Pages.Workbench.OpenApi;

public partial class OpenApi
{
    [Inject]
    ApiDocInfoManager Manager { get; set; } = default!;

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
    bool Refreshing { get; set; }

    string ActiveId { get; set; } = "api";

    FluentTab? currentTab;
    FluentDialog? modelInfoDialog;
    bool ModelInfoDialogIsHidden { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        CheckProject();
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
        var res = await Manager.FilterAsync(
            new ApiDocInfoFilterDto { PageSize = 999, ProjectId = ProjectContext.SolutionId }
        );
        Docs = res.Data;
    }

    private async Task GetDocContentAsync(bool isFresh)
    {
        if (CurrentDoc?.Id != null)
        {
            Refreshing = true;
            await Task.Yield();
            var res = await Manager.GetContentAsync(CurrentDoc.Id, isFresh);

            if (res is not null)
            {
                RestApiGroups = res?.RestApiGroups ?? [];
                FilteredRestApiGroups = RestApiGroups;
                ModelList = res?.TypeMeta ?? [];
                FilteredModelList = ModelList;
                Tags = res?.OpenApiTags ?? [];
            }
            else
            {
                ToastService.ShowError(Manager.ErrorMsg);
            }
            if (CurrentApi is not null)
            {
                CurrentApi = RestApiGroups
                    .SelectMany(g => g.ApiInfos ?? [])
                    .FirstOrDefault(a => a.Router == CurrentApi?.Router);
            }
            Refreshing = false;
        }
    }

    private async Task OnDocSelectedAsync(ApiDocInfoItemDto doc)
    {
        IsLoading = true;
        CurrentDoc = doc;
        RestApiGroups.Clear();
        ModelList.Clear();
        FilteredModelList.Clear();
        Tags.Clear();
        CurrentApi = null;
        await GetDocContentAsync(true);

        IsLoading = false;
    }

    private void SelectApi(RestApiInfo api)
    {
        CurrentApi = api;
        SelectedNav = api.OperationId ?? api.Router;
    }

    private void SelectModel(TypeMeta model)
    {
        CurrentModel = model;
        SelectedNav = model.Name;
    }

    private void SearchApi()
    {
        FilteredRestApiGroups = !string.IsNullOrWhiteSpace(ApiSearchKeyword)
            ? RestApiGroups
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
                .ToList()
            : RestApiGroups;
    }

    private void SearchModel()
    {
        FilteredModelList = !string.IsNullOrWhiteSpace(ModelSearchKeyword)
            ? ModelList
                .Where(model =>
                    (
                        model.Name?.Contains(ModelSearchKeyword, StringComparison.OrdinalIgnoreCase)
                        ?? false
                    )
                    || ((
                        model.Comment?.Contains(
                            ModelSearchKeyword,
                            StringComparison.OrdinalIgnoreCase
                        ) ?? false
                    )
                        && model.IsEnum == false)
                )
                .ToList()
            : ModelList;
    }

    private void OpenModelInfoDialog(string modelName)
    {
        Console.WriteLine("modelName:" + modelName);
        var model = ModelList.Where(m => m.FullName == modelName).FirstOrDefault();

        Console.WriteLine(model?.Name);
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
        CurrentDoc = Docs.FirstOrDefault(d => d.Name == (result.Data as ApiDocInfoAddDto)?.Name);
        await GetDocContentAsync(false);
        StateHasChanged();
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
        CurrentDoc = Docs.FirstOrDefault(d => d.Name == (result.Data as ApiDocInfoUpdateDto)?.Name);
        await GetDocContentAsync(false);
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
                options.Body = string.Join(
                    "\n",
                    files.Select(f => f.FullName.Replace(ProjectContext.SolutionPath ?? "", ""))
                );
                options.Timestamp = DateTime.Now;
                options.Section = App.MESSAGES_NOTIFICATION_CENTER;
            });

            await GetApiDocsAsync();
            CurrentDoc = Docs.FirstOrDefault(d => d.Name == CurrentDoc.Name);
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
            LangWithArguments(Localizer.ConfirmDeleteMessage, Lang(Localizer.File)),
            Lang(Localizer.Delete),
            Lang(Localizer.Cancel)
        );

        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            var res = await Manager.DeleteAsync([CurrentDoc.Id], false);
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

    private static string GetApiTypeColor(HttpMethod type)
    {
        var color = type.Method switch
        {
            "GET" => "#318deb",
            "POST" => "#14cc78",
            "PUT" => "#fca130",
            "PATCH" => "#fca130",
            "DELETE" => "#f93e3e",
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
