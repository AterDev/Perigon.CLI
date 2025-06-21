using System.ComponentModel.DataAnnotations;

using Ater.Common.Utils;

using OpenIddict.Abstractions;

namespace IdentityServer.Managers;

public class ApplicationManager(
    IOpenIddictApplicationManager applicationManager,
    ILogger<ApplicationManager> logger) : ManagerBase(logger)
{
    public async Task<List<ClientAppItemDto>> ListAsync()
    {
        var applications = new List<ClientAppItemDto>();
        await foreach (var app in applicationManager.ListAsync())
        {
            var clientId = await applicationManager.GetClientIdAsync(app);
            var displayName = await applicationManager.GetDisplayNameAsync(app);
            var redirectUris = await applicationManager.GetRedirectUrisAsync(app);
            applications.Add(new ClientAppItemDto
            {
                ClientId = clientId!,
                ClientName = displayName!,
                RedirectUris = redirectUris.Select(u => u.ToString()!).ToList()
            });
        }
        return applications;
    }

    public async Task<object> CreateAsync(ClientAppAddDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientId))
        {
            dto.ClientId = Guid.NewGuid().ToString("N").Substring(0, 32);
        }
        if (string.IsNullOrWhiteSpace(dto.ClientSecret))
        {
            dto.ClientSecret = "Ater-" + HashCrypto.GetRnd(40, useLow: true, useSpe: true);
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId,
            ClientSecret = dto.ClientSecret,
            DisplayName = dto.ClientName,
        };

        if (dto.RedirectUris.Count > 0)
        {
            foreach (var uri in dto.RedirectUris)
            {
                if (Uri.TryCreate(uri, UriKind.Absolute, out var redirectUri))
                {
                    descriptor.RedirectUris.Add(redirectUri);
                }
            }
        }
        return await applicationManager.CreateAsync(descriptor);
    }

    public async Task UpdateAsync(string clientId, ClientAppEditDto dto)
    {
        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            throw new InvalidOperationException("Application not found");
        }
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId ?? clientId,
            ClientSecret = dto.ClientSecret!,
            DisplayName = dto.ClientName!,
        };
        if (!string.IsNullOrWhiteSpace(dto.RedirectUri))
        {
            descriptor.RedirectUris.Add(new Uri(dto.RedirectUri!));
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
    public List<string> RedirectUris { get; set; } = [];
}

public class ClientAppAddDto
{
    [MaxLength(50)]
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    [MaxLength(50, ErrorMessage = "The max length is 50")]
    public string ClientName { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = [];
}

public class ClientAppEditDto
{
    public string? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUri { get; set; }
}
