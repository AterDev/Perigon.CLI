using SystemMod.Models.SystemPermissionGroupDtos;

namespace SystemMod.Managers;

public class SystemPermissionGroupManager(
    DefaultDbContext dbContext,
    ILogger<SystemPermissionGroupManager> logger
) : ManagerBase<DefaultDbContext, SystemPermissionGroup>(dbContext, logger)
{
    public async Task<PageList<SystemPermissionGroupItemDto>> ToPageAsync(
        SystemPermissionGroupFilterDto filter
    )
    {
        Queryable = Queryable.WhereNotNull(filter.Name, q => q.Name.Contains(filter.Name!));
        return await ToPageAsync<SystemPermissionGroupFilterDto, SystemPermissionGroupItemDto>(
            filter
        );
    }

    public override async Task<SystemPermissionGroup?> FindAsync(Guid id)
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
}
