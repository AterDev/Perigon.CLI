using Microsoft.AspNetCore.Components;

namespace IdentityServer.Components.Pages.CleintApp;

public partial class ClientApp : PageBase
{
    protected FluentDataGrid<ClientAppItemDto>? grid;
    protected List<ClientAppItemDto> apps = [];
    protected ClientAppEditDto editModel = new();
    protected RenderFragment template = (builder) => builder.OpenElement(0, "span");

    protected PaginationState pagination = new() { ItemsPerPage = 12 };

    [Inject]
    protected ApplicationManager AppManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected async Task LoadData()
    {
        grid?.SetLoadingState(true);
        apps = await AppManager.ListAsync() ?? [];
        grid?.SetLoadingState(false);
    }

    protected async Task ShowAddDialog()
    {
        DialogParameters parameters = new()
        {
            Title = Localizer.Get(LanguageKey.AddClientApp),
            PrimaryAction = "Yes",
            PrimaryActionEnabled = false,
            SecondaryAction = "No",
            Width = "400px",
            PreventScroll = true
        };

        var dialog = await DialogService.ShowDialogAsync<AddClientApp>(parameters);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            if (result.Data is null)
            {
                ToastService.ShowSuccess(Lang(LanguageKey.Add, LanguageKey.Success));
                await LoadData();

            }
            else
            {
                ToastService.ShowError(Lang(LanguageKey.Add, LanguageKey.Failed));
            }
        }
    }

    protected async Task EditAsync(ClientAppItemDto app)
    {
        DialogParameters parameters = new()
        {
            Title = Lang(LanguageKey.Edit, LanguageKey.ClientApp, " "),
            PrimaryAction = "Yes",
            PrimaryActionEnabled = false,
            SecondaryAction = "No",
            Width = "400px",
            PreventScroll = true
        };
        var dialog = await DialogService.ShowDialogAsync<EditClientApp>(app, parameters);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            if (result.Data is null)
            {
                ToastService.ShowSuccess(Lang(LanguageKey.Edit, LanguageKey.Success));
                await LoadData();
            }
            else
            {
                ToastService.ShowError(Lang(LanguageKey.Edit, LanguageKey.Failed));
            }
        }
    }

    protected async Task Delete(string clientId)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(LanguageKey.ConfirmDeleteMessage),
            Lang(LanguageKey.Yes),
            Lang(LanguageKey.No),
            Lang(LanguageKey.Delete, LanguageKey.ClientApp, " "));
        var result = await dialog.Result;
        if (result.Cancelled == false)
        {
            await AppManager.DeleteAsync(clientId);
            await LoadData();
        }
    }
}
