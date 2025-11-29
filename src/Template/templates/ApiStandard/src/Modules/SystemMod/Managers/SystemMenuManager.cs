using EntityFramework.AppDbFactory;
using SystemMod.Models.SystemMenuDtos;

namespace SystemMod.Managers;

/// <summary>
/// 系统菜单
/// </summary>
public class SystemMenuManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemMenuManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemMenu>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SystemMenu> AddAsync(SystemMenuAddDto dto)
    {
        var entity = dto.MapTo<SystemMenu>();
        // other property set
        if (dto.ParentId != null)
        {
            entity.ParentId = dto.ParentId.Value;
        }
        await InsertAsync(entity);
        return entity;
    }

    public async Task<bool> EditAsync(Guid id, SystemMenuUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto) > 0;
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// 菜单同步
    /// </summary>
    /// <param name="menus"></param>
    /// <returns></returns>
    public async Task<bool> SyncSystemMenusAsync(List<SystemMenuSyncDto> menus)
    {
        // 查询当前菜单内容
        List<SystemMenu> currentMenus = await _dbSet.ToListAsync();
        List<SystemMenu> flatMenus    = FlatTree(menus);

        var accessCodes = flatMenus.Select(m => m.AccessCode).ToList();
        // 获取需要被删除的
        var needDeleteMenus = currentMenus.Where(m => !accessCodes.Contains(m.AccessCode)).ToList();
        if (needDeleteMenus.Count != 0)
        {
            _dbSet.RemoveRange(needDeleteMenus);
            currentMenus = currentMenus.Except(needDeleteMenus).ToList();
        }

        // 菜单新增与更新
        foreach (SystemMenu menu in flatMenus)
        {
            if (currentMenus.Any(c => c.AccessCode == menu.AccessCode))
            {
                var index = currentMenus.FindIndex(m => m
                    .AccessCode
                    .Equals(menu.AccessCode));
                currentMenus[index].Name = menu.Name;
                currentMenus[index].Sort = menu.Sort;
                currentMenus[index].Icon = menu.Icon;
                _dbSet.Update(currentMenus[index]);
            }
            else
            {
                if (menu.Parent != null)
                {
                    SystemMenu? parent = currentMenus
                        .Where(c => c.AccessCode == menu.Parent.AccessCode)
                        .FirstOrDefault();
                    if (parent != null)
                    {
                        menu.Parent = parent;
                    }
                }
                await _dbSet.AddAsync(menu);
            }
        }
        return await _dbContext.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// flat tree
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static List<SystemMenu> FlatTree(
        List<SystemMenuSyncDto> list,
        SystemMenu? parent = null
    )
    {
        var res = new List<SystemMenu>();
        foreach (SystemMenuSyncDto item in list)
        {
            if (item.Children.Count != 0)
            {
                var menu = new SystemMenu
                {
                    Name       = item.Name,
                    AccessCode = item.AccessCode,
                    MenuType   = (MenuType)item.MenuType,
                    Parent     = parent,
                    Sort       = item.Sort ?? 0,
                    Icon       = item.Icon,
                };
                res.Add(menu);
                List<SystemMenu> children = FlatTree(item.Children, menu);
                res.AddRange(children);
            }
            else
            {
                var menu = new SystemMenu
                {
                    Name       = item.Name,
                    AccessCode = item.AccessCode,
                    MenuType   = (MenuType)item.MenuType,
                    Parent     = parent,
                    Sort       = item.Sort ?? 0,
                    Icon       = item.Icon,
                };
                res.Add(menu);
            }
        }
        return res;
    }

    public async Task<PageList<SystemMenu>> FilterAsync(SystemMenuFilterDto filter)
    {
        List<SystemMenu>? menus;
        if (filter.RoleId != null)
        {
            menus = await Queryable
                .Where(q => q
                .Roles
                .Any(r => r.Id == filter.RoleId))
                .ToListAsync();
            menus.BuildTree();
        }
        else if (filter.ParentId != null)
        {
            menus = await Queryable
                .Where(q => q.Parent != null && q.ParentId == filter.ParentId)
                .ToListAsync();
        }
        else
        {
            menus = await Queryable
                .AsNoTracking()
                .OrderByDescending(t => t.Sort)
                .ThenByDescending(t => t.CreatedTime)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
            menus = menus.BuildTree();
        }

        return new PageList<SystemMenu>() { Data = menus, PageIndex = filter.PageIndex };
    }

    public async Task<bool> DeleteAsync(IEnumerable<Guid> ids, bool isDelete = false)
    {
        if (!ids.Any())
        {
            return false;
        }

        if (ids.Count() == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                return await DeleteOrUpdateAsync(ids, !isDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                return await DeleteOrUpdateAsync(ownedIds, !isDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    /// <summary>
    /// wheather user have edit/delete permission
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        // TODO: other conditions
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        var query = _dbSet
            .Where(q => ids.Contains(q.Id) && q.TenantId == _userContext.TenantId)
            // TODO: other conditions
            .Select(q => q.Id);
        return await query.ToListAsync();
    }
}
