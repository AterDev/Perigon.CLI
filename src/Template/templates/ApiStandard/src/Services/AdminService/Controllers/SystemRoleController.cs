using Ater.AspNetCore.Models;
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
    [HttpGet]
    public async Task<ActionResult<PageList<SystemRoleItemDto>>> ListAsync(
        [FromQuery] SystemRoleFilterDto filter
    )
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SystemRole>> AddAsync(SystemRoleAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<SystemRole>> UpdateAsync(
        [FromRoute] Guid id,
        SystemRoleUpdateDto dto
    )
    {
        var entity = await _manager.UpdateAsync(id, dto);
        return Ok(entity);
    }

    /// <summary>
    /// 角色菜单 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("menus")]
    public async Task<ActionResult<SystemRole>> UpdateMenusAsync(
        [FromBody] SystemRoleSetMenusDto dto
    )
    {
        var result = await _manager.SetMenusAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Set Permission Group ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("permissionGroups")]
    public async Task<ActionResult<SystemRole>> UpdatePermissionGroupsAsync(
        [FromBody] SystemRoleSetPermissionGroupsDto dto
    )
    {
        var result = await _manager.SetPermissionGroupsAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SystemRoleDetailDto?>> DetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.GetAsync(id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync(id);
        return NoContent();
    }
}
