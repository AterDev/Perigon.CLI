using Ater.AspNetCore.Models;
using SystemMod.Models.SystemLogsDtos;

namespace AdminService.Controllers;

/// <summary>
/// 系统日志
/// </summary>
/// <see cref="SystemLogsManager"/>
public class SystemLogsController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemLogsController> logger,
    SystemLogsManager manager
) : RestControllerBase<SystemLogsManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemLogsItemDto>>> FilterAsync(
        SystemLogsFilterDto filter
    )
    {
        return await _manager.ToPageAsync(filter);
    }
}
