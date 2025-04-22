using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace IdentityServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public ApplicationController(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    /// <summary>
    /// 查询所有已注册的应用
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var applications = new List<object>();
        await foreach (var app in _applicationManager.ListAsync())
        {
            //applications.Add(new
            //{
            //    app.ClientId,
            //    app.DisplayName,
            //    RedirectUris = app.RedirectUris?.Select(uri => uri.ToString())
            //});
        }
        return Ok(applications);
    }

    /// <summary>
    /// 新增应用
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateApplicationDto dto)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId,
            ClientSecret = dto.ClientSecret,
            DisplayName = dto.DisplayName,
            RedirectUris = { new Uri(dto.RedirectUri) },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.Prefixes.Scope + "api"
            }
        };
        await _applicationManager.CreateAsync(descriptor);
        return Ok();
    }

    /// <summary>
    /// 更新应用
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{clientId}")]
    public async Task<IActionResult> UpdateAsync(string clientId, [FromBody] UpdateApplicationDto dto)
    {
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            return NotFound();
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId,
            ClientSecret = dto.ClientSecret,
            DisplayName = dto.DisplayName,
            RedirectUris = { new Uri(dto.RedirectUri) },
        };

        await _applicationManager.UpdateAsync(application, descriptor);
        return Ok();
    }

    /// <summary>
    /// 删除应用
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteAsync(string clientId)
    {
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            return NotFound();
        }

        await _applicationManager.DeleteAsync(application);
        return Ok();
    }
}

public class CreateApplicationDto
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string DisplayName { get; set; }
    public string RedirectUri { get; set; }
}

public class UpdateApplicationDto
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string DisplayName { get; set; }
    public string RedirectUri { get; set; }
    public List<string> Permissions { get; set; }
}
