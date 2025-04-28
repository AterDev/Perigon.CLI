using Framework.Common.Models;
using Framework.Common.Utils;
using SharedModule;
using SharedModule.Implement;
using SystemMod.Models.SystemLogsDtos;

namespace SystemMod.Managers;
/// <summary>
/// 系统日志
/// </summary>
public class SystemLogsManager(
    DataAccessContext<SystemLogs> dataContext,
    ILogger<SystemLogsManager> logger,
    UserContext userContext) : ManagerBase<SystemLogs>(dataContext, logger)
{
    private readonly UserContext _userContext = userContext;

    public async Task<PageList<SystemLogsItemDto>> ToPageAsync(SystemLogsFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.ActionUserName, q => q.ActionUserName == filter.ActionUserName)
            .WhereNotNull(filter.TargetName, q => q.TargetName == filter.TargetName)
            .WhereNotNull(filter.ActionType, q => q.ActionType == filter.ActionType);

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            // 包含今天
            var endDate = filter.EndDate.Value.AddDays(1);
            Queryable = Queryable.Between(q => q.CreatedTime, filter.StartDate.Value, endDate);
        }
        return await ToPageAsync<SystemLogsFilterDto, SystemLogsItemDto>(filter);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemLogs?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemLogs> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }
}
