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
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SystemPermissionGroup>> AddAsync(SystemPermissionGroupAddDto dto)
    {
        SystemPermissionGroup entity = dto.MapTo<SystemPermissionGroup>();
        await _manager.InsertAsync(entity);
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
        SystemPermissionGroup? current = await _manager.GetGroupAsync(id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }

        current.Merge(dto);
        await _manager.InsertAsync(current);
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
        var res = await _manager.FindAsync<SystemPermissionGroupDetailDto>(d => d.Id == id);
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
        return entity == null ? NotFound() : await _manager.DeleteOrUpdateAsync([id]) > 0;
    }
}
