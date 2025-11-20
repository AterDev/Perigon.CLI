using EntityFramework.AppDbFactory;
using SystemMod.Models.SystemLogsDtos;

namespace SystemMod.Managers;

/// <summary>
/// 系统日志
/// </summary>
public class SystemLogsManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemLogsManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemLogs>(dbContextFactory, userContext, logger)
{
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
        return await PageListAsync<SystemLogsFilterDto, SystemLogsItemDto>(filter);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemLogs?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemLogs> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }
}
