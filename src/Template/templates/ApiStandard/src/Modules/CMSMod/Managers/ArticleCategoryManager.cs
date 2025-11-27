using Ater.AspNetCore.Abstraction;
using CMSMod.Models.ArticleCategoryDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Share;
using Share.Exceptions;

namespace CMSMod.Managers;

/// <summary>
/// 目录管理
/// </summary>
public class ArticleCategoryManager(
    TenantDbFactory dbContextFactory,
    ILogger<ArticleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ArticleCategory>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// add catalog
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ArticleCategory> AddAsync(ArticleCategoryAddDto dto)
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
    public async Task<int> EditAsync(Guid id, ArticleCategoryUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public async Task<ArticleCategoryDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await FindAsync<ArticleCategoryDetailDto>(q => q.Id == id);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public async Task<PageList<ArticleCategoryItemDto>> FilterAsync(ArticleCategoryFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(filter.Name, q => q
            .Name
            .Contains(filter.Name!));
        return await PageListAsync<ArticleCategoryFilterDto, ArticleCategoryItemDto>(filter);
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

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        query = query.Where(q => q.UserId == _userContext.UserId);
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

    /// <summary>
    /// 获取树型目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<ArticleCategory>> GetTreeAsync()
    {
        List<ArticleCategory> data = await ListAsync<ArticleCategory>(null);
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
