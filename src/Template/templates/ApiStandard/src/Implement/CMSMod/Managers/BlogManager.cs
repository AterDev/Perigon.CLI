using CMSMod.Models.BlogDtos;

using EntityFramework;
using Share.Implement;

namespace CMSMod.Managers;
/// <summary>
/// 博客
/// </summary>
public class BlogManager(
    DataAccessContext<Blog> dataContext,
    ILogger<BlogManager> logger,
    UserContext userContext) : ManagerBase<Blog>(dataContext, logger)
{
    private readonly UserContext _userContext = userContext;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(BlogAddDto dto)
    {
        Blog entity = dto.MapTo<BlogAddDto, Blog>();
        entity.UserId = _userContext.UserId;
        entity.CatalogId = dto.CatalogId;
        // other required props
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
            .WhereNotNull(filter.UserId, q => q.User.Id == filter.UserId)
            .WhereNotNull(filter.CatalogId, q => q.Catalog.Id == filter.CatalogId);
        return await ToPageAsync<BlogFilterDto, BlogItemDto>(filter);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Blog?> GetOwnedAsync(Guid id)
    {
        IQueryable<Blog> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
