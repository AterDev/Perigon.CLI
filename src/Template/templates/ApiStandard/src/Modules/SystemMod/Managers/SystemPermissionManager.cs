using EntityFramework.DBProvider;
using SystemMod.Models.SystemPermissionDtos;

namespace SystemMod.Managers;

/// <summary>
/// 权限
/// </summary>
public class SystemPermissionManager(
    DefaultDbContext dbContext,
    ILogger<SystemPermissionManager> logger
) : ManagerBase<DefaultDbContext, SystemPermission>(dbContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemPermissionAddDto dto)
    {
        SystemPermission entity = dto.MapTo<SystemPermission>();
        entity.GroupId = dto.SystemPermissionGroupId;
        return await AddAsync(entity) ? entity.Id : null;
    }

    public override Task<SystemPermission?> GetCurrentAsync(Guid id)
    {
        return _dbSet.Where(p => p.Id == id).Include(p => p.Group).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(SystemPermission entity, SystemPermissionUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<SystemPermissionItemDto>> ToPageAsync(
        SystemPermissionFilterDto filter
    )
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.PermissionType, q => q.PermissionType == filter.PermissionType)
            .WhereNotNull(filter.GroupId, q => q.Group.Id == filter.GroupId);

        return await ToPageAsync<SystemPermissionFilterDto, SystemPermissionItemDto>(filter);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemPermission?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemPermission> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }
}
