using System.Linq.Expressions;
using EntityFramework.AppDbFactory;
using SystemMod.Models.SystemRoleDtos;

namespace SystemMod.Managers;

public class SystemRoleManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemRoleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemRole>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemRole> AddAsync(SystemRoleAddDto dto)
    {
        var entity = dto.MapTo<SystemRole>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<PageList<SystemRoleItemDto>> FilterAsync(SystemRoleFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.NameValue, q => q.NameValue == filter.NameValue);
        return await PageListAsync<SystemRoleFilterDto, SystemRoleItemDto>(filter);
    }

    /// <summary>
    /// 获取菜单
    /// </summary>
    /// <param name="systemRoles"></param>
    /// <returns></returns>
    public async Task<List<SystemMenu>> GetSystemMenusAsync(List<SystemRole> systemRoles)
    {
        IEnumerable<Guid> ids = systemRoles.Select(r => r.Id);
        return await _dbContext
            .SystemMenus.Where(m => m.Roles.Any(r => ids.Contains(r.Id)))
            .ToListAsync();
    }

    /// <summary>
    /// Set PermissionGroups
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemRole> SetPermissionGroupsAsync(SystemRoleSetPermissionGroupsDto dto)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var current =
                await FindAsync(dto.Id) ?? throw new BusinessException(Localizer.RoleNotFound);

            if (!CanUserModifyRole(current))
            {
                throw new BusinessException(
                    Localizer.InsufficientPermissions,
                    StatusCodes.Status403Forbidden
                );
            }

            await _dbContext.Entry(current).Collection(r => r.PermissionGroups).LoadAsync();

            var groups = await _dbContext
                .SystemPermissionGroups.Where(m => dto.PermissionGroupIds.Contains(m.Id))
                .ToListAsync();

            current.PermissionGroups = groups;
            await InsertAsync(current);

            return current;
        });
    }

    /// <summary>
    /// 获取权限组
    /// </summary>
    /// <param name="systemRoles"></param>
    /// <returns></returns>
    public async Task<List<SystemPermissionGroup>> GetPermissionGroupsAsync(
        List<SystemRole> systemRoles
    )
    {
        IEnumerable<Guid> ids = systemRoles.Select(r => r.Id);
        return await _dbContext
            .SystemPermissionGroups.Where(m => m.Roles.Any(r => ids.Contains(r.Id)))
            .ToListAsync();
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemRole> UpdateAsync(Guid id, SystemRoleUpdateDto dto)
    {
        var current = await FindAsync(id) ?? throw new BusinessException(Localizer.RoleNotFound);

        // 权限验证可以在这里进行，利用 _userContext
        if (!CanUserModifyRole(current))
        {
            throw new BusinessException(
                Localizer.InsufficientPermissions,
                StatusCodes.Status403Forbidden
            );
        }

        current.Merge(dto);
        await InsertAsync(current);
        return current;
    }

    /// <summary>
    /// 验证用户是否可以修改角色
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    private bool CanUserModifyRole(SystemRole role)
    {
        // 实现具体的权限逻辑
        return _userContext.IsRole(WebConst.SuperAdmin);
    }

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemRole> SetMenusAsync(SystemRoleSetMenusDto dto)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var current =
                await FindAsync(dto.Id) ?? throw new BusinessException(Localizer.RoleNotFound);

            if (!CanUserModifyRole(current))
            {
                throw new BusinessException(
                    Localizer.InsufficientPermissions,
                    StatusCodes.Status403Forbidden
                );
            }

            await _dbContext.Entry(current).Collection(r => r.Menus).LoadAsync();

            var menus = await _dbContext
                .SystemMenus.Where(m => dto.MenuIds.Contains(m.Id))
                .ToListAsync();

            current.Menus = menus;
            await InsertAsync(current);

            return current;
        });
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    public async Task<List<SystemRole>> ListAsync(Expression<Func<SystemRole, bool>>? whereExp = null)
    {
        return await _dbContext.SystemRoles.AsNoTracking().Where(whereExp ?? (e => true)).ToListAsync();
    }

    public async Task<SystemRoleDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<SystemRoleDetailDto>(d => d.Id == id);
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
