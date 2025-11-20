using EntityFramework.AppDbFactory;
using SystemMod.Models.SystemPermissionGroupDtos;

namespace SystemMod.Managers;

public class SystemPermissionGroupManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemPermissionGroupManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemPermissionGroup>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<SystemPermissionGroupItemDto>> ToPageAsync(
        SystemPermissionGroupFilterDto filter
    )
    {
        Queryable = Queryable.WhereNotNull(filter.Name, q => q.Name.Contains(filter.Name!));
        return await PageListAsync<SystemPermissionGroupFilterDto, SystemPermissionGroupItemDto>(
            filter
        );
    }

    public async Task<SystemPermissionGroup?> GetGroupAsync(Guid id)
    {
        return await Queryable
            .Include(g => g.Permissions)
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.Id == id);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemPermissionGroup?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemPermissionGroup> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
