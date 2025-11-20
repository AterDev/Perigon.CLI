using Ater.AspNetCore.Abstraction;
using CMSMod.Models.CatalogDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Share;
using Share.Exceptions;

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
    /// <summary>
    /// add catalog
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ArticleCategory> AddAsync(CatalogAddDto dto)
    {
        var entity = dto.MapTo<ArticleCategory>();
        entity.UserId = _userContext.UserId;
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// edit catalog
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="BusinessException"></exception>
    public async Task<int> EditAsync(Guid id, CatalogUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public async Task<CatalogDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<CatalogDetailDto>(q => q.Id == id);
    }

    public async Task<PageList<CatalogItemDto>> FilterAsync(CatalogFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(filter.Name, q => q.Name.Contains(filter.Name!));
        return await PageListAsync<CatalogFilterDto, CatalogItemDto>(filter);
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await DeleteAsync(id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q =>
            q.Id == id && q.UserId == _userContext.UserId && q.TenantId == _userContext.TenantId
        );
        return await query.AnyAsync();
    }

    /// <summary>
    /// 获取树型目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<ArticleCategory>> GetTreeAsync()
    {
        List<ArticleCategory> data = await ListAsync(null);
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
}
