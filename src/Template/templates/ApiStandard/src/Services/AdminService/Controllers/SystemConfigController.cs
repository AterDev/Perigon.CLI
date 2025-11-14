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
        return await _manager.ToPageAsync(filter);
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
        SystemConfig entity = dto.MapTo<SystemConfig>();
        await _manager.UpsertAsync(entity);
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
        SystemConfig? current = await _manager.FindAsync(id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }

        current.Merge(dto);
        await _manager.UpsertAsync(current);
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
        var res = await _manager.FindAsync<SystemConfigDetailDto>(c => c.Id == id);
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
        SystemConfig? entity = await _manager.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        return entity.IsSystem
            ? Problem("系统配置，无法删除!")
            : await _manager.DeleteAsync([id], false);
    }
}
