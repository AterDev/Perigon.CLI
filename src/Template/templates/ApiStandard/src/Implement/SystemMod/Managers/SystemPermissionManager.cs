using Framework.Common.Models;
using Framework.Common.Utils;
using Share.Implement;
using SystemMod.Models.SystemPermissionDtos;

namespace SystemMod.Managers;
/// <summary>
/// 权限
/// </summary>
public class SystemPermissionManager(
    DataAccessContext<SystemPermission> dataContext,
    ILogger<SystemPermissionManager> logger
        ) : ManagerBase<SystemPermission>(dataContext, logger)
{

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemPermissionAddDto dto)
    {
        SystemPermission entity = dto.MapTo<SystemPermissionAddDto, SystemPermission>();
        entity.GroupId = dto.SystemPermissionGroupId;
        return await AddAsync(entity) ? entity.Id : null;
    }

    public override Task<SystemPermission?> GetCurrentAsync(Guid id)
    {
        return Command.Where(p => p.Id == id)
            .Include(p => p.Group)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(SystemPermission entity, SystemPermissionUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<SystemPermissionItemDto>> ToPageAsync(SystemPermissionFilterDto filter)
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
        IQueryable<SystemPermission> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
