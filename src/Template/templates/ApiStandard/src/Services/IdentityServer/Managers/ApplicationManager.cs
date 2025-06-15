using OpenIddict.Abstractions;

namespace IdentityServer.Managers;

public class ApplicationManager
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public ApplicationManager(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    public async Task<List<ClientAppDto>> ListAsync()
    {
        var applications = new List<ClientAppDto>();
        await foreach (var app in _applicationManager.ListAsync())
        {
            var clientId = await _applicationManager.GetClientIdAsync(app);
            var displayName = await _applicationManager.GetDisplayNameAsync(app);
            var redirectUris = await _applicationManager.GetRedirectUrisAsync(app);
            applications.Add(new ClientAppDto
            {
                ClientId = clientId!,
                DisplayName = displayName!,
                RedirectUris = redirectUris.Select(u => u.ToString()!).ToList()
            });
        }
        return applications;
    }

    public async Task CreateAsync(ClientAppEditModel dto)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId!,
            ClientSecret = dto.ClientSecret!,
            DisplayName = dto.DisplayName!,
        };
        if (!string.IsNullOrWhiteSpace(dto.RedirectUri))
        {
            descriptor.RedirectUris.Add(new Uri(dto.RedirectUri!));
        }
        await _applicationManager.CreateAsync(descriptor);
    }

    public async Task UpdateAsync(string clientId, ClientAppEditModel dto)
    {
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            throw new InvalidOperationException("Application not found");
        }
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId ?? clientId,
            ClientSecret = dto.ClientSecret!,
            DisplayName = dto.DisplayName!,
        };
        if (!string.IsNullOrWhiteSpace(dto.RedirectUri))
        {
            descriptor.RedirectUris.Add(new Uri(dto.RedirectUri!));
        }
        await _applicationManager.UpdateAsync(application, descriptor);
    }

    public async Task DeleteAsync(string clientId)
    {
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            throw new InvalidOperationException("Application not found");
        }
        await _applicationManager.DeleteAsync(application);
    }
}

public class ClientAppDto
{
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = [];
}

public class ClientAppEditModel
{
    public string? ClientId { get; set; }
    public string? DisplayName { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUri { get; set; }
}
