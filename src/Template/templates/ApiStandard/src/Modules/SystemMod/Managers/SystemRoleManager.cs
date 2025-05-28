using Ater.Common.Models;
using Ater.Common.Utils;
using Share.Implement;
using SystemMod.Models.SystemRoleDtos;

namespace SystemMod.Managers;

public class SystemRoleManager(
    DataAccessContext<SystemRole> dataContext,
    ILogger<SystemRoleManager> logger) : ManagerBase<SystemRole>(dataContext, logger)
{

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemRoleAddDto dto)
    {
        var entity = dto.MapTo<SystemRoleAddDto, SystemRole>();
        return await AddAsync(entity) ? entity.Id : null;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(SystemRole entity, SystemRoleUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<SystemRoleItemDto>> ToPageAsync(SystemRoleFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.NameValue, q => q.NameValue == filter.NameValue);
        return await ToPageAsync<SystemRoleFilterDto, SystemRoleItemDto>(filter);
    }

    /// <summary>
    /// 获取菜单
    /// </summary>
    /// <param name="systemRoles"></param>
    /// <returns></returns>
    public async Task<List<SystemMenu>> GetSystemMenusAsync(List<SystemRole> systemRoles)
    {
        IEnumerable<Guid> ids = systemRoles.Select(r => r.Id);
        return await CommandContext.SystemMenus
            .Where(m => m.Roles.Any(r => ids.Contains(r.Id)))
        .ToListAsync();
    }

    /// <summary>
    /// Set PermissionGroups
    /// </summary>
    /// <param name="current"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemRole?> SetPermissionGroupsAsync(SystemRole current, SystemRoleSetPermissionGroupsDto dto)
    {
        try
        {
            await CommandContext.Entry(current)
                .Collection(r => r.PermissionGroups)
                .LoadAsync();

            List<SystemPermissionGroup> groups = await CommandContext.SystemPermissionGroups
                .Where(m => dto.PermissionGroupIds.Contains(m.Id))
                .ToListAsync();
            current.PermissionGroups = groups;
            await SaveChangesAsync();
            return current;
        }
        catch (Exception e)
        {
            _logger.LogError("set role permission groups failed:{message}", e.Message);
            return null;
        }
    }

    /// <summary>
    /// 获取权限组
    /// </summary>
    /// <param name="systemRoles"></param>
    /// <returns></returns>
    public async Task<List<SystemPermissionGroup>> GetPermissionGroupsAsync(List<SystemRole> systemRoles)
    {
        IEnumerable<Guid> ids = systemRoles.Select(r => r.Id);
        return await CommandContext.SystemPermissionGroups
            .Where(m => m.Roles.Any(r => ids.Contains(r.Id)))
            .ToListAsync();
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemRole?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemRole> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="current"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<SystemRole?> SetMenusAsync(SystemRole current, SystemRoleSetMenusDto dto)
    {
        // 更新角色菜单
        try
        {
            await CommandContext.Entry(current)
                .Collection(r => r.Menus)
                .LoadAsync();

            current.Menus = [];

            List<SystemMenu> menus = await CommandContext.SystemMenus
                .Where(m => dto.MenuIds.Contains(m.Id))
                .ToListAsync();
            current.Menus = menus;
            await SaveChangesAsync();
            return current;
        }
        catch (Exception e)
        {
            _logger.LogError("update role menus failed:{message}", e.Message);
            return default;
        }

    }
}
