using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
    private ClientAppEditDto EditDto = default!;

    /// <summary>
    /// application types
    /// </summary>
    private readonly IReadOnlyList<string> _applicationTypes =
    [
        OpenIddictConstants.ApplicationTypes.Web,
        OpenIddictConstants.ApplicationTypes.Native,
    ];

    /// <summary>
    /// client types
    /// </summary>
    private readonly IReadOnlyList<string> _clientTypes =
    [
        OpenIddictConstants.ClientTypes.Public,
        OpenIddictConstants.ClientTypes.Confidential,
    ];

    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;
    public List<string> grantTypeOptions =
    [
        GrantTypes.AuthorizationCode,
        GrantTypes.ClientCredentials,
        GrantTypes.Password,
        GrantTypes.RefreshToken,
        GrantTypes.Implicit,
    ];
    public List<string> scopeOptions =
    [
        Scopes.Address,
        Scopes.Email,
        Scopes.OpenId,
        Scopes.OfflineAccess,
        Scopes.Phone,
        Scopes.Profile,
        Scopes.Roles,
    ];

    public IEnumerable<string>? selectedGrantTypes;
    public IEnumerable<string>? selectedScopes;

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
                EditDto.RedirectUris = RedirectUris
                    .Split(
                        separators,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .Distinct()
                    .ToList();
            }
            if (selectedGrantTypes is not null)
            {
                EditDto.GrantTypes = selectedGrantTypes.ToList();
            }
            if (selectedScopes is not null)
            {
                EditDto.Scopes = selectedScopes.ToList();
            }
            try
            {
                await ApplicationManager.UpdateAsync(ClientId, EditDto);
            }
            catch (Exception ex)
            {
                ToastService.ShowError(
                    Lang(LanguageKey.Edit, LanguageKey.Failed) + $":{ex.Message}"
                );
                return;
            }
        }
    }

    private void OnSearchScope(OptionsSearchEventArgs<string> e)
    {
        e.Items = scopeOptions
            .Where(s => s.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void OnSearchGrantType(OptionsSearchEventArgs<string> e)
    {
        e.Items = grantTypeOptions
            .Where(s => s.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/clientapp");
    }
}
