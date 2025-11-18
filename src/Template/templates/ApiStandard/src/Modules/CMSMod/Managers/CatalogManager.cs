using Ater.AspNetCore.Abstraction;
using CMSMod.Models.CatalogDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Share.Implement;

namespace CMSMod.Managers;

/// <summary>
/// 目录管理
/// </summary>
public class CatalogManager(
    TenantDbFactory dbContextFactory,
    ILogger<BlogManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ArticleCategory>(dbContextFactory, userContext, logger)
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
    public async Task<List<ArticleCategory>> GetTreeAsync()
    {
        List<ArticleCategory> data = await ToListAsync(null);
        List<ArticleCategory> tree = data.BuildTree();
        return tree;
    }

    /// <summary>
    /// 获取叶结点目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<ArticleCategory>> GetLeafCatalogsAsync()
    {
        List<Guid?> parentIds = await Queryable.Select(s => s.ParentId).ToListAsync();

        List<ArticleCategory> source = await Queryable
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
    public async Task<ArticleCategory?> GetOwnedAsync(Guid id, Guid userId)
    {
        IQueryable<ArticleCategory> query = _dbSet.Where(q => q.Id == id);
        // 属于当前角色的对象
        query = query.Where(q => q.UserId == userId);
        return await query.FirstOrDefaultAsync();
    }
}
