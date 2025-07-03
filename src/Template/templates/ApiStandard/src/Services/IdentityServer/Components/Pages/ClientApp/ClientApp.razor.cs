using Microsoft.AspNetCore.Components;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityServer.Components.Pages.ClientApp;

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
            Title = Localizer.Get(Localizer.AddClientApp),
            PrimaryAction = "Yes",
            PrimaryActionEnabled = false,
            SecondaryAction = "No",
            Width = "400px",
            PreventScroll = true,
        };

        var dialog = await DialogService.ShowDialogAsync<AddClientApp>(parameters);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            if (result.Data is OpenIddictEntityFrameworkCoreApplication app)
            {
                await LoadData();
                await ShowResultDialog(app);
            }
            else
            {
                ToastService.ShowError(Lang(Localizer.Add, Localizer.Failed));
            }
        }
    }

    protected async Task ShowResultDialog(OpenIddictEntityFrameworkCoreApplication descriptor)
    {
        DialogParameters parameters = new()
        {
            Title = Lang(Localizer.Add, Localizer.Success, " "),
            PrimaryAction = "Yes",
            PrimaryActionEnabled = false,
            SecondaryAction = "No",
            PreventScroll = true,
            Width = "auto",
            Modal = false,
        };
        var dialog = await DialogService.ShowDialogAsync<AddResultDialog>(descriptor, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled) { }
    }

    protected void ToDetailPage(ClientAppItemDto app)
    {
        NavigationManager.NavigateTo($"/client-app/{app.ClientId}", true);
    }

    protected async Task Delete(string clientId)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(Localizer.ConfirmDeleteMessage),
            Lang(Localizer.Yes),
            Lang(Localizer.No),
            Lang(Localizer.Delete, Localizer.ClientApp, " ")
        );
        var result = await dialog.Result;
        if (result.Cancelled == false)
        {
            await AppManager.DeleteAsync(clientId);
            await LoadData();
        }
    }
}
