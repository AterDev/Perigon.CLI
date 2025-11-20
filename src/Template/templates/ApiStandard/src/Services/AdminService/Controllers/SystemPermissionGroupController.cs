using Ater.AspNetCore.Models;
using SystemMod.Models.SystemPermissionGroupDtos;

namespace AdminService.Controllers;

/// <see cref="SystemPermissionGroupManager"/>
[Authorize(WebConst.SuperAdmin)]
public class SystemPermissionGroupController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemPermissionGroupController> logger,
    SystemPermissionGroupManager manager
) : RestControllerBase<SystemPermissionGroupManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemPermissionGroupItemDto>>> FilterAsync(
        SystemPermissionGroupFilterDto filter
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
    public async Task<ActionResult<SystemPermissionGroup>> AddAsync(SystemPermissionGroupAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(GetDetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool>> UpdateAsync(
        [FromRoute] Guid id,
        SystemPermissionGroupUpdateDto dto
    )
    {
        await _manager.EditAsync(id, dto);
        return true;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SystemPermissionGroupDetailDto?>> GetDetailAsync(
        [FromRoute] Guid id
    )
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
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        SystemPermissionGroup? entity = await _manager.GetGroupAsync(id);
        return entity == null ? NotFound() : await _manager.DeleteAsync(id) > 0;
    }
}
