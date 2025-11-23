using EntityFramework.AppDbFactory;
using SystemMod.Models.SystemPermissionGroupDtos;

namespace SystemMod.Managers;

public class SystemPermissionGroupManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemPermissionGroupManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemPermissionGroup>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<SystemPermissionGroupItemDto>> FilterAsync(
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
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return query.AnyAsync();
    }

    // --- New API style methods (Create/Add, Edit, Get, Filter, Delete) ---
    public async Task<SystemPermissionGroup> AddAsync(SystemPermissionGroupAddDto dto)
    {
        var entity = dto.MapTo<SystemPermissionGroup>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, SystemPermissionGroupUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }

    public async Task<SystemPermissionGroupDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<SystemPermissionGroupDetailDto>(d => d.Id == id);
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var entity = await GetOwnedAsync(id);
        if (entity == null)
        {
            return 0;
        }
        return await DeleteOrUpdateAsync([id]);
    }
}
