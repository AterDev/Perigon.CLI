using Framework.Common.Models;
using SharedModule;
using SharedModule.Implement;
using SystemMod.Managers;
using SystemMod.Models.SystemLogsDtos;
namespace SystemMod.Controllers.AdminControllers;

/// <summary>
/// 系统日志
/// </summary>
/// <see cref="SystemLogsManager"/>
public class SystemLogsController(
    IUserContext user,
    ILogger<SystemLogsController> logger,
    SystemLogsManager manager
        ) : RestControllerBase<SystemLogsManager>(manager, user, logger)
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