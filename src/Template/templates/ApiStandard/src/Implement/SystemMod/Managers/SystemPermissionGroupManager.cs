using Framework.Common.Models;
using Framework.Common.Utils;
using SharedModule.Implement;
using SystemMod.Models.SystemPermissionGroupDtos;

namespace SystemMod.Managers;

public class SystemPermissionGroupManager(
    DataAccessContext<SystemPermissionGroup> dataContext,
    ILogger<SystemPermissionGroupManager> logger
        ) : ManagerBase<SystemPermissionGroup>(dataContext, logger)
{

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemPermissionGroupAddDto dto)
    {
        SystemPermissionGroup entity = dto.MapTo<SystemPermissionGroupAddDto, SystemPermissionGroup>();
        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(SystemPermissionGroup entity, SystemPermissionGroupUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<SystemPermissionGroupItemDto>> ToPageAsync(SystemPermissionGroupFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(filter.Name, q => q.Name.Contains(filter.Name!));
        return await ToPageAsync<SystemPermissionGroupFilterDto, SystemPermissionGroupItemDto>(filter);
    }

    public override async Task<SystemPermissionGroup?> FindAsync(Guid id)
    {
        return await Query.Include(g => g.Permissions)
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
        IQueryable<SystemPermissionGroup> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
