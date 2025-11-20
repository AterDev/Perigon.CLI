using Ater.AspNetCore.Models;
using SystemMod.Models.SystemConfigDtos;

namespace AdminService.Controllers;

/// <summary>
/// 系统配置
/// </summary>
/// <see cref="SystemConfigManager"/>
public class SystemConfigController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemConfigController> logger,
    SystemConfigManager manager
) : RestControllerBase<SystemConfigManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 获取配置列表 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemConfigItemDto>>> FilterAsync(
        SystemConfigFilterDto filter
    )
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 获取枚举信息 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet("enum")]
    public async Task<ActionResult<Dictionary<string, List<EnumDictionary>>>> GetEnumConfigsAsync()
    {
        return await _manager.GetEnumConfigsAsync();
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SystemConfig>> AddAsync(SystemConfigAddDto dto)
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
        SystemConfigUpdateDto dto
    )
    {
        // Use manager to perform edit which includes permission check
        await _manager.EditAsync(id, dto);
        return true;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SystemConfigDetailDto?>> GetDetailAsync([FromRoute] Guid id)
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
        var entity = await _manager.GetOwnedAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        if (entity.IsSystem)
        {
            return Problem("系统配置，无法删除!");
        }

        var deleted = await _manager.DeleteAsync(id);
        return deleted > 0;
    }
}
