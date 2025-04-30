using Share;
using SystemMod.Models.SystemLogsDtos;
namespace SystemMod.Controllers.AdminControllers;

/// <summary>
/// 系统日志
/// </summary>
/// <see cref="SystemLogsManager"/>
public class SystemLogsController(
    Localizer localizer,
    UserContext user,
    ILogger<SystemLogsController> logger,
    SystemLogsManager manager
        ) : AdminControllerBase<SystemLogsManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemLogsItemDto>>> FilterAsync(SystemLogsFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

}