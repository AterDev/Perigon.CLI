using Ater.Common.Models;
using Ater.Common.Utils;
using Share.Implement;
using SystemMod.Models.SystemMenuDtos;

namespace SystemMod.Managers;
/// <summary>
/// 系统菜单
/// </summary>
public class SystemMenuManager(
    DataAccessContext<SystemMenu> dataContext,
    ILogger<SystemMenuManager> logger) : ManagerBase<SystemMenu>(dataContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemMenuAddDto dto)
    {
        SystemMenu entity = dto.MapTo<SystemMenuAddDto, SystemMenu>();
        if (dto.ParentId != null)
        {
            entity.ParentId = dto.ParentId.Value;
        }

        return await AddAsync(entity) ? entity.Id : null;
    }

    /// <summary>
    /// 菜单同步
    /// </summary>
    /// <param name="menus"></param>
    /// <returns></returns>
    public async Task<bool> SyncSystemMenusAsync(List<SystemMenuSyncDto> menus)
    {
        // 查询当前菜单内容
        List<SystemMenu> currentMenus = await Command.ToListAsync();
        List<SystemMenu> flatMenus = FlatTree(menus);

        var accessCodes = flatMenus.Select(m => m.AccessCode).ToList();
        // 获取需要被删除的
        var needDeleteMenus = currentMenus.Where(m => !accessCodes.Contains(m.AccessCode)).ToList();
        if (needDeleteMenus.Count != 0)
        {
            Command.RemoveRange(needDeleteMenus);
            currentMenus = currentMenus.Except(needDeleteMenus).ToList();
        }

        // 菜单新增与更新
        foreach (SystemMenu menu in flatMenus)
        {

            if (currentMenus.Any(c => c.AccessCode == menu.AccessCode))
            {
                var index = currentMenus.FindIndex(m => m.AccessCode.Equals(menu.AccessCode));
                currentMenus[index].Name = menu.Name;
                currentMenus[index].Sort = menu.Sort;
                currentMenus[index].Icon = menu.Icon;
                Command.Update(currentMenus[index]);
            }
            else
            {
                if (menu.Parent != null)
                {
                    SystemMenu? parent = currentMenus.Where(c => c.AccessCode == menu.Parent.AccessCode).FirstOrDefault();
                    if (parent != null)
                    {
                        menu.Parent = parent;
                    }
                }
                await Command.AddAsync(menu);
            }
        }
        return await SaveChangesAsync() > 0;
    }

    /// <summary>
    /// flat tree 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private List<SystemMenu> FlatTree(List<SystemMenuSyncDto> list, SystemMenu? parent = null)
    {
        var res = new List<SystemMenu>();
        foreach (SystemMenuSyncDto item in list)
        {
            if (item.Children.Count != 0)
            {
                var menu = new SystemMenu
                {
                    Name = item.Name,
                    AccessCode = item.AccessCode,
                    MenuType = (MenuType)item.MenuType,
                    Parent = parent,
                    Sort = item.Sort ?? 0,
                    Icon = item.Icon
                };
                res.Add(menu);
                List<SystemMenu> children = FlatTree(item.Children, menu);
                res.AddRange(children);
            }
            else
            {
                var menu = new SystemMenu
                {
                    Name = item.Name,
                    AccessCode = item.AccessCode,
                    MenuType = (MenuType)item.MenuType,
                    Parent = parent,
                    Sort = item.Sort ?? 0,
                    Icon = item.Icon
                };
                res.Add(menu);
            }
        }
        return res;
    }

    public async Task<bool> UpdateAsync(SystemMenu entity, SystemMenuUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<SystemMenu>> FilterAsync(SystemMenuFilterDto filter)
    {
        List<SystemMenu>? menus;
        if (filter.RoleId != null)
        {
            menus = await Queryable.Where(q => q.Roles.Any(r => r.Id == filter.RoleId))
                .ToListAsync();
            menus.BuildTree();
        }
        else if (filter.ParentId != null)
        {
            menus = await Queryable.Where(q => q.Parent != null && q.ParentId == filter.ParentId)
                .ToListAsync();
        }
        else
        {
            menus = await Queryable.AsNoTracking()
            .OrderByDescending(t => t.Sort)
                .ThenByDescending(t => t.CreatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageIndex)
            .Take(filter.PageSize)
            .ToListAsync();
            menus = menus.BuildTree();
        }

        return new PageList<SystemMenu>()
        {
            Data = menus,
            PageIndex = filter.PageIndex
        };
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemMenu?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemMenu> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
