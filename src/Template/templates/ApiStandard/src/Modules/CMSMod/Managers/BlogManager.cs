using CMSMod.Models.BlogDtos;
using EntityFramework.DBProvider;

namespace CMSMod.Managers;

/// <summary>
/// 博客
/// </summary>
public class BlogManager(DefaultDbContext dbContext, ILogger<BlogManager> logger)
    : ManagerBase<DefaultDbContext, Blog>(dbContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(BlogAddDto dto)
    {
        Blog entity = dto.MapTo<Blog>();
        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(Blog entity, BlogUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<BlogItemDto>> ToPageAsync(BlogFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Title, q => q.Title == filter.Title)
            .WhereNotNull(filter.LanguageType, q => q.LanguageType == filter.LanguageType)
            .WhereNotNull(filter.BlogType, q => q.BlogType == filter.BlogType)
            .WhereNotNull(filter.IsAudit, q => q.IsAudit == filter.IsAudit)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsOriginal, q => q.IsOriginal == filter.IsOriginal)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId)
            .WhereNotNull(filter.CatalogId, q => q.Catalog.Id == filter.CatalogId);
        return await ToPageAsync<BlogFilterDto, BlogItemDto>(filter);
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
    public async Task<Blog?> GetOwnedAsync(Guid id)
    {
        IQueryable<Blog> query = _dbSet.Where(q => q.Id == id);
        // 获取权限范围的实体
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }
}
