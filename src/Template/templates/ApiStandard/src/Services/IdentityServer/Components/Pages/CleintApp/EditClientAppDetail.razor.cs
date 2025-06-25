using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using IdentityServer.Managers;

namespace IdentityServer.Components.Pages.CleintApp;

public partial class EditClientAppDetail : ComponentBase
{
    [Parameter]
    public string ClientId { get; set; } = string.Empty;

    [Inject]
    public ApplicationManager ApplicationManager { get; set; } = default!;
    [Inject]
    public Localizer Localizer { get; set; } = default!;

    public ClientAppDetailDto DetailDto { get; set; } = new();
    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;
    public List<string> grantTypeOptions = new() { "authorization_code", "client_credentials", "password", "refresh_token", "implicit", "device_code" };
    public List<string> scopeOptions = new() { "openid", "profile", "email", "phone", "roles" };

    protected override async Task OnInitializedAsync()
    {
        // TODO: 加载详细信息
        // var detail = await ApplicationManager.GetDetailAsync(ClientId);
        // if (detail != null) { ... }
    }

    private async Task SaveDetailAsync()
    {
        // TODO: 保存详细信息
        // await ApplicationManager.UpdateDetailAsync(ClientId, DetailDto);
    }

    public class ClientAppDetailDto
    {
        public string ClientName { get; set; } = string.Empty;
        public List<string> RedirectUris { get; set; } = [];
        public List<string> GrantTypes { get; set; } = [];
        public List<string> Scopes { get; set; } = [];
        public List<string> PostLogoutRedirectUris { get; set; } = [];
        public bool AllowOfflineAccess { get; set; }
    }
}
