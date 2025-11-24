using Ater.AspNetCore.Abstraction;
using CMSMod.Models.ArticleDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;

namespace CMSMod.Managers;

/// <summary>
/// 博客
/// </summary>
public class ArticleManager(
    TenantDbFactory dbContextFactory,
    ILogger<ArticleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Article>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ArticleItemDto>> ToPageAsync(ArticleFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Title, q => q.Title == filter.Title)
            .WhereNotNull(filter.LanguageType, q => q.LanguageType == filter.LanguageType)
            .WhereNotNull(filter.BlogType, q => q.ContentType == filter.BlogType)
            .WhereNotNull(filter.IsAudit, q => q.IsAudit == filter.IsAudit)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsOriginal, q => q.IsOriginal == filter.IsOriginal)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId)
            .WhereNotNull(filter.CatalogId, q => q.Catalog.Id == filter.CatalogId);


        return await PageListAsync<ArticleFilterDto, ArticleItemDto>(filter);
    }

    public async Task<bool> IsOwnedAsync(Guid id, Guid userId)
    {
        return await Queryable.AnyAsync(q => q.Id == id && q.UserId == userId);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Article?> GetOwnedAsync(Guid id)
    {
        IQueryable<Article> query = _dbSet.Where(q => q.Id == id);
        // 获取权限范围的实体
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.UserId == _userContext.UserId);
        return await query.AnyAsync();
    }
}
