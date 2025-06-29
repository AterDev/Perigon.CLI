using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace IdentityServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScopeController : ControllerBase
{
    private readonly IOpenIddictScopeManager _scopeManager;

    public ScopeController(IOpenIddictScopeManager scopeManager)
    {
        _scopeManager = scopeManager;
    }

    /// <summary>
    /// 查询所有Scope
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var scopes = new List<object>();
        await foreach (var scope in _scopeManager.ListAsync())
        {
            var name = await _scopeManager.GetNameAsync(scope);
            var displayName = await _scopeManager.GetDisplayNameAsync(scope);
            var resources = await _scopeManager.GetResourcesAsync(scope);
            scopes.Add(new
            {
                Name = name,
                DisplayName = displayName,
                Resources = resources.ToList()
            });
        }
        return Ok(scopes);
    }

    /// <summary>
    /// 新增Scope
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateScopeDto dto)
    {
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName,
        };
        if (dto.Resources != null)
        {
            foreach (var r in dto.Resources)
            {
                descriptor.Resources.Add(r);
            }
        }

        await _scopeManager.CreateAsync(descriptor);
        return Ok();
    }

    /// <summary>
    /// 更新Scope
    /// </summary>
    [HttpPut("{name}")]
    public async Task<IActionResult> UpdateAsync(string name, [FromBody] UpdateScopeDto dto)
    {
        var scope = await _scopeManager.FindByNameAsync(name);
        if (scope == null)
        {
            return NotFound();
        }
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = dto.Name ?? name,
            DisplayName = dto.DisplayName,
        };
        if (dto.Resources != null)
        {
            foreach (var r in dto.Resources)
            {
                descriptor.Resources.Add(r);
            }
        }

        await _scopeManager.UpdateAsync(scope, descriptor);
        return Ok();
    }

    /// <summary>
    /// 删除Scope
    /// </summary>
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteAsync(string name)
    {
        var scope = await _scopeManager.FindByNameAsync(name);
        if (scope == null)
        {
            return NotFound();
        }
        await _scopeManager.DeleteAsync(scope);
        return Ok();
    }
}

public class CreateScopeDto
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public List<string>? Resources { get; set; }
}

public class UpdateScopeDto
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public List<string>? Resources { get; set; }
}
