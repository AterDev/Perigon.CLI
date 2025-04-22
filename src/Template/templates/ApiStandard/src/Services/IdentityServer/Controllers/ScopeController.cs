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

    // 查询所有已注册的Scope
    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var scopes = new List<object>();
        await foreach (var scope in _scopeManager.ListAsync())
        {
            //scopes.Add(new
            //{
            //    scope.Name,
            //    scope.DisplayName,
            //    scope.Resources
            //});
        }
        return Ok(scopes);
    }

    // 新增Scope
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateScopeDto dto)
    {
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            //Resources = dto.Resources
        };
        await _scopeManager.CreateAsync(descriptor);
        return Ok();
    }

    // 更新Scope
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
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            //Resources = dto.Resources
        };

        await _scopeManager.UpdateAsync(scope, descriptor);
        return Ok();
    }

    // 删除Scope
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
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public List<string> Resources { get; set; }
}

public class UpdateScopeDto
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public List<string> Resources { get; set; }
}
