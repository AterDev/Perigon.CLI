using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using OpenIddict.EntityFrameworkCore.Models;
namespace IdentityServer.Components.Pages.ClientApp;

public partial class ClientAppDetail
{
    [Parameter]
    public string ClientId { get; set; } = default!;

    public OpenIddictEntityFrameworkCoreApplication? ClientApp { get; set; }

    [Inject]
    public ApplicationManager ApplicationManager { get; set; } = default!;

    private bool formValid;
    private EditContext? editContext;
    private ClientAppEditDto? EditDto;

    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;
    public List<string> grantTypeOptions = new() { "authorization_code", "client_credentials", "password", "refresh_token", "implicit", "device_code" };
    public List<string> scopeOptions = new() { "openid", "profile", "email", "phone", "roles" };

    protected override async Task OnInitializedAsync()
    {
        ClientApp = await ApplicationManager.GetClientAppAsync(ClientId);
        if (ClientApp is null)
        {
            return;
        }
        EditDto ??= new ClientAppEditDto
        {
            ClientName = ClientApp.DisplayName,
            ApplicationType = ClientApp.ApplicationType,
            ClientType = ClientApp.ClientType,
        };

        editContext = new(EditDto);
        editContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (editContext is not null)
        {
            formValid = editContext.Validate();
            StateHasChanged();
        }
    }
    public void Dispose()
    {
        if (editContext is not null)
        {
            editContext.OnFieldChanged -= HandleFieldChanged;
        }
    }

    private async Task SaveAsync()
    {
        if (!editContext!.Validate())
        {
            ToastService.ShowError(Lang(LanguageKey.FormValidFailed));

            editContext.NotifyValidationStateChanged();
            return;
        }
        if (EditDto is not null)
        {
            if (!string.IsNullOrWhiteSpace(RedirectUris))
            {
                var separators = new[] { "\r\n", "\n" };
                EditDto.RedirectUris = RedirectUris.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
            }
            try
            {
                await ApplicationManager.UpdateAsync(ClientId, EditDto);
            }
            catch (Exception ex)
            {
                ToastService.ShowError(Lang(LanguageKey.Edit, LanguageKey.Failed) + $":{ex.Message}");
                return;
            }
        }
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/clientapp");
    }
}
