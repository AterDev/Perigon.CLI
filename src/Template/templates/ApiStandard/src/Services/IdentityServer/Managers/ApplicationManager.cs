using System.ComponentModel.DataAnnotations;
using Ater.Common.Utils;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityServer.Managers;

public class ApplicationManager(
    IOpenIddictApplicationManager applicationManager,
    ILogger<ApplicationManager> logger
) : ManagerBase(logger)
{
    public async Task<List<ClientAppItemDto>> ListAsync()
    {
        var applications = new List<ClientAppItemDto>();

        await foreach (var app in applicationManager.ListAsync())
        {
            var clientId = await applicationManager.GetClientIdAsync(app);
            var displayName = await applicationManager.GetDisplayNameAsync(app);
            var redirectUris = await applicationManager.GetRedirectUrisAsync(app);
            var grantTypes = await applicationManager.GetClientTypeAsync(app) is string type
                ? new List<string> { type }
                : [];
            applications.Add(
                new ClientAppItemDto
                {
                    ClientId = clientId!,
                    ClientName = displayName!,
                    RedirectUris = redirectUris.Select(u => u.ToString()!).ToList(),
                    GrantTypes = grantTypes,
                }
            );
        }
        return applications;
    }

    public async Task<OpenIddictEntityFrameworkCoreApplication?> GetClientAppAsync(string clientId)
    {
        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            return null;
        }
        return application as OpenIddictEntityFrameworkCoreApplication;
    }

    public async Task<OpenIddictEntityFrameworkCoreApplication?> CreateAsync(ClientAppAddDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientId))
        {
            dto.ClientId = Guid.NewGuid().ToString("N")[..32];
            dto.ClientSecret = Guid.CreateVersion7().ToString("N")[..16];
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId,
            DisplayName = dto.ClientName,
            ApplicationType = dto.ApplicationType,
            ClientType = dto.ClientType,
            ClientSecret = dto.ClientSecret,
        };
        var res = await applicationManager.CreateAsync(descriptor);
        return res as OpenIddictEntityFrameworkCoreApplication;
    }

    public async Task UpdateAsync(string clientId, ClientAppEditDto dto)
    {
        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            throw new InvalidOperationException("Application not found");
        }
        var descriptor = new OpenIddictApplicationDescriptor { ClientId = clientId };
        if (dto.ClientName is not null)
        {
            descriptor.DisplayName = dto.ClientName;
        }
        if (dto.RedirectUris != null)
        {
            foreach (var uri in dto.RedirectUris)
            {
                descriptor.RedirectUris.Add(new Uri(uri));
            }
        }
        if (dto.GrantTypes is not null && dto.GrantTypes.Count > 0)
        {
            foreach (var grant in dto.GrantTypes)
            {
                descriptor.Permissions.Add(grant);
                // 可根据实际需要添加更多类型映射
            }
        }
        if (dto.Scopes is not null && dto.Scopes.Count > 0)
        {
            foreach (var scope in dto.Scopes)
            {
                descriptor.Permissions.Add(scope);
            }
        }
        await applicationManager.UpdateAsync(application, descriptor);
    }

    public async Task DeleteAsync(string clientId)
    {
        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            throw new InvalidOperationException("Application not found");
        }
        await applicationManager.DeleteAsync(application);
    }
}

public class ClientAppItemDto
{
    [MaxLength(50)]
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "")]
    public string ClientName { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = default!;
    public string ClientType { get; set; } = default!;
    public List<string> RedirectUris { get; set; } = [];
    public List<string> GrantTypes { get; set; } = [];
    public List<string> Scopes { get; set; } = [];
    public List<string> PostLogoutRedirectUris { get; set; } = [];
    public bool AllowOfflineAccess { get; set; }
}

public class ClientAppAddDto
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "The max length is 50")]
    [MinLength(3, ErrorMessage = "The min length is 3")]
    public string ClientName { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = OpenIddictConstants.ApplicationTypes.Web;
    public string ClientType { get; set; } = OpenIddictConstants.ClientTypes.Confidential;
}

public class ClientAppEditDto
{
    [MaxLength(50, ErrorMessage = "The max length is 50")]
    [MinLength(3, ErrorMessage = "The min length is 3")]
    public string? ClientName { get; set; }
    public List<string>? RedirectUris { get; set; }
    public List<string>? GrantTypes { get; set; }
    public List<string>? Scopes { get; set; }
    public List<string>? PostLogoutRedirectUris { get; set; }
    public bool? AllowOfflineAccess { get; set; }
    public string? ApplicationType { get; set; }
    public string? ClientType { get; set; }
}
