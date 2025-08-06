using Ater.Common.Models;
using Ater.Common.Utils;
using CMSMod.Models.CatalogDtos;
using EntityFramework;
using Share.Implement;

namespace CMSMod.Managers;

/// <summary>
/// 目录管理
/// </summary>
public class CatalogManager(
    DataAccessContext<Catalog> dataContext,
    UserContext userContext,
    ILogger<BlogManager> logger
) : ManagerBase<Catalog>(dataContext, logger)
{
    private readonly UserContext _userContext = userContext;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(CatalogAddDto dto)
    {
        Catalog entity = dto.MapTo<CatalogAddDto, Catalog>();
        entity.UserId = _userContext.UserId;
        if (dto.ParentId != null)
        {
            Catalog? parent = await GetCurrentAsync(dto.ParentId.Value);
            if (parent != null)
            {
                entity.ParentId = parent.Id;
                entity.Parent = parent;
                entity.Level = (short)(parent.Level + 1);
            }
            else
            {
                entity.Level = 0;
            }
        }
        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(Catalog entity, CatalogUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<CatalogItemDto>> ToPageAsync(CatalogFilterDto filter)
    {
        // TODO:根据实际业务构建筛选条件
        // if ... Queryable = ...
        return await ToPageAsync<CatalogFilterDto, CatalogItemDto>(filter);
    }

    /// <summary>
    /// 获取树型目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<Catalog>> GetTreeAsync()
    {
        List<Catalog> data = await ToListAsync(null);
        List<Catalog> tree = data.BuildTree();
        return tree;
    }

    /// <summary>
    /// 获取叶结点目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<Catalog>> GetLeafCatalogsAsync()
    {
        List<Guid?> parentIds = await Query.Select(s => s.ParentId).ToListAsync();

        List<Catalog> source = await Query
            .Where(c => !parentIds.Contains(c.Id))
            .Include(c => c.Parent)
            .ToListAsync();
        return source;
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Catalog?> GetOwnedAsync(Guid id)
    {
        IQueryable<Catalog> query = DbSet.Where(q => q.Id == id);
        // 属于当前角色的对象
        query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }
}
