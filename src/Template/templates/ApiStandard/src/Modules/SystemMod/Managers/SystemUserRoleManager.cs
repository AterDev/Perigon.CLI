using EntityFramework.AppDbFactory;

namespace SystemMod.Managers;

/// <summary>
/// 系统用户角色关联管理器
/// </summary>
public class SystemUserRoleManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemUserRoleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemUserRole>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 批量设置用户角色
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="roleIds">角色ID列表</param>
    /// <returns></returns>
    public async Task<bool> SetUserRolesAsync(Guid userId, List<Guid> roleIds)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            // 先删除现有的用户角色关联
            await _dbSet.Where(ur => ur.UserId == userId).ExecuteDeleteAsync();

            // 批量插入新的用户角色关联
            if (roleIds.Count > 0)
            {
                var userRoles = roleIds.Select(roleId => new SystemUserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                });

                await BulkInsertAsync(userRoles);
            }

            return true;
        });
    }

    /// <summary>
    /// 获取用户的角色ID列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    public async Task<List<Guid>> GetUserRoleIdsAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();
    }

    /// <summary>
    /// 获取拥有某个角色的用户ID列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns></returns>
    public async Task<List<Guid>> GetRoleUserIdsAsync(Guid roleId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();
    }

    /// <summary>
    /// 检查用户是否拥有指定角色
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="roleId">角色ID</param>
    /// <returns></returns>
    public async Task<bool> HasUserRoleAsync(Guid userId, Guid roleId)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id).Join(
            _dbContext.SystemUsers,
            ur => ur.UserId,
            u => u.Id,
            (ur, u) => u
        ).Where(u => u.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }
}
