using EntityFramework.DBProvider;
using FileManagerMod.Models.FolderDtos;

namespace FileManagerMod.Managers;

/// <summary>
/// 文件夹
/// </summary>
public class FolderManager(DefaultDbContext dbContext, ILogger<FolderManager> logger)
    : ManagerBase<DefaultDbContext, Folder>(dbContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(FolderAddDto dto)
    {
        Folder entity = dto.MapTo<Folder>();
        if (dto.ParentId != null)
        {
            Folder? parent = await FindAsync(dto.ParentId.Value);
            entity.Parent = parent;
            entity.Path = $"{parent!.Path}.{entity.Name}";
        }
        else
        {
            entity.Path = entity.Name;
        }
        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(Folder entity, FolderUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<FolderItemDto>> ToPageAsync(FolderFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.ParentId, q => q.ParentId == filter.ParentId);

        return await ToPageAsync<FolderFilterDto, FolderItemDto>(filter);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Folder?> GetOwnedAsync(Guid id)
    {
        IQueryable<Folder> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }
}
