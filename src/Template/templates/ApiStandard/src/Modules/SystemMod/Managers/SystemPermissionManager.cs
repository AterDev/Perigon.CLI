using EntityFramework.AppDbFactory;
using SystemMod.Models.SystemPermissionDtos;

namespace SystemMod.Managers;

/// <summary>
/// 权限
/// </summary>
public class SystemPermissionManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemPermissionManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemPermission>(dbContextFactory, userContext, logger)
{
    public Task<SystemPermission?> GetSystemPermissionAsync(Guid id)
    {
        return _dbSet.Where(p => p.Id == id).Include(p => p.Group).FirstOrDefaultAsync();
    }

    public async Task<PageList<SystemPermissionItemDto>> FilterAsync(
        SystemPermissionFilterDto filter
    )
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.PermissionType, q => q.PermissionType == filter.PermissionType)
            .WhereNotNull(filter.GroupId, q => q.Group.Id == filter.GroupId);

        return await PageListAsync<SystemPermissionFilterDto, SystemPermissionItemDto>(filter);
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    // New business methods
    public async Task<SystemPermission> AddAsync(SystemPermissionAddDto dto)
    {
        var entity = dto.MapTo<SystemPermission>();
        entity.GroupId = dto.SystemPermissionGroupId;
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, SystemPermissionUpdateDto dto)
    {
        // load current entity to handle group change
        var current = await _dbSet.Where(q => q.Id == id).Include(q => q.Group).FirstOrDefaultAsync();
        if (current == null)
        {
            throw new BusinessException(Localizer.NotFoundResource);
        }

        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        if (dto.SystemPermissionGroupId != null && (current.Group == null || current.Group.Id != dto.SystemPermissionGroupId.Value))
        {
            var group = await _dbContext.SystemPermissionGroups.FindAsync(dto.SystemPermissionGroupId.Value);
            if (group == null)
            {
                throw new BusinessException(Localizer.NotFoundResource);
            }
            current.Group = group;
        }

        // apply updates via partial update
        return await UpdateAsync(id, dto);
    }

    public async Task<SystemPermissionDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<SystemPermissionDetailDto>(d => d.Id == id);
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await DeleteOrUpdateAsync([id], false);
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }
}
