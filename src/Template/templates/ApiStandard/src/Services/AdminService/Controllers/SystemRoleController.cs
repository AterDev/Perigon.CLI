using Ater.Common.Models;
using SystemMod.Models.SystemRoleDtos;

namespace AdminService.Controllers;

/// <summary>
/// 系统角色
/// <see cref="SystemRoleManager"/>
/// </summary>
[Authorize(WebConst.SuperAdmin)]
public class SystemRoleController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemRoleController> logger,
    SystemRoleManager manager
) : RestControllerBase<SystemRoleManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemRoleItemDto>>> FilterAsync(
        SystemRoleFilterDto filter
    )
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(SystemRoleAddDto dto)
    {
        var id = await _manager.AddAsync(dto);
        return id == null ? base.Problem(Localizer.AddFailed) : id;
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, SystemRoleUpdateDto dto)
    {
        SystemRole? current = await _manager.GetOwnedAsync(id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }

        return await _manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 角色菜单 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("menus")]
    public async Task<ActionResult<SystemRole?>> UpdateMenusAsync(
        [FromBody] SystemRoleSetMenusDto dto
    )
    {
        SystemRole? current = await _manager.GetCurrentAsync(dto.Id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }
        SystemRole? res = await _manager.SetMenusAsync(current, dto);
        return Ok(res) ?? Problem("菜单更新失败");
    }

    /// <summary>
    /// Set Permission Group ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("permissionGroups")]
    public async Task<ActionResult<SystemRole?>> UpdatePermissionGroupsAsync(
        [FromBody] SystemRoleSetPermissionGroupsDto dto
    )
    {
        SystemRole? current = await _manager.GetCurrentAsync(dto.Id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }
        SystemRole? res = await _manager.SetPermissionGroupsAsync(current, dto);
        return Ok(res) ?? Problem("权限组更新失败");
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SystemRoleDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<SystemRoleDetailDto>(d => d.Id == id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        SystemRole? entity = await _manager.GetOwnedAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        // return Forbid();
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}
