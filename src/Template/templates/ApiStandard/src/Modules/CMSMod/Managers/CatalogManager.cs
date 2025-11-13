using CMSMod.Models.CatalogDtos;
using EntityFramework.DBProvider;

namespace CMSMod.Managers;

/// <summary>
/// 目录管理
/// </summary>
public class CatalogManager(DefaultDbContext dbContext, ILogger<BlogManager> logger)
    : ManagerBase<DefaultDbContext, Catalog>(dbContext, logger)
{
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
        List<Guid?> parentIds = await Queryable.Select(s => s.ParentId).ToListAsync();

        List<Catalog> source = await Queryable
            .Where(c => !parentIds.Contains(c.Id))
            .Include(c => c.Parent)
            .ToListAsync();
        return source;
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Catalog?> GetOwnedAsync(Guid id, Guid userId)
    {
        IQueryable<Catalog> query = _dbSet.Where(q => q.Id == id);
        // 属于当前角色的对象
        query = query.Where(q => q.UserId == userId);
        return await query.FirstOrDefaultAsync();
    }
}
