using DataContext.DBProvider;
using StudioMod.Models.GenStepDtos;

namespace StudioMod.Managers;

/// <summary>
/// task step
/// </summary>
public class GenStepManager(
    DefaultDbContext dbContext,
    ILogger<GenStepManager> logger,
    IProjectContext projectContext
) : ManagerBase<DefaultDbContext, GenStep>(dbContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;

    protected override ICollection<GenStep> GetCollection() => _dbContext.GenSteps;

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int?> CreateNewEntityAsync(GenStepAddDto dto)
    {
        var entity = dto.MapTo<GenStep>();
        entity.ProjectId = (int)_projectContext.SolutionId!.Value.GetHashCode();

        var fileExt = Path.GetExtension(dto.OutputPath ?? "");
        entity.FileType = fileExt;

        return await AddAsync(entity) ? entity.Id : null;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(GenStep entity, GenStepUpdateDto dto)
    {
        entity.Merge(dto);
        var fileExt = Path.GetExtension(dto.OutputPath ?? "");
        entity.FileType = fileExt;
        return await base.UpdateAsync(entity);
    }

    public async Task<PageList<GenStepItemDto>> ToPageAsync(GenStepFilterDto filter)
    {
        var query = GetCollection().AsQueryable();
        
        if (!string.IsNullOrEmpty(filter.Name))
        {
            query = query.Where(q => q.Name.Contains(filter.Name));
        }
        
        if (!string.IsNullOrEmpty(filter.FileType))
        {
            query = query.Where(q => q.FileType == filter.FileType);
        }
        
        if (filter.ProjectId.HasValue)
        {
            query = query.Where(q => q.ProjectId == (int)filter.ProjectId.Value.GetHashCode());
        }

        Queryable = query;
        return await ToPageAsync<GenStepFilterDto, GenStepItemDto>(filter);
    }

    /// <summary>
    /// 获取实体详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenStepDetailDto?> GetDetailAsync(int id)
    {
        return await FindAsync<GenStepDetailDto>(e => e.Id == id);
    }

    /// <summary>
    /// TODO:唯一性判断
    /// </summary>
    /// <param name="unique">唯一标识</param>
    /// <param name="id">排除当前</param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(string unique, int? id = null)
    {
        // 自定义唯一性验证参数和逻辑
        var query = GetCollection()
            .Where(q => q.Id.ToString() == unique);
        
        if (id.HasValue)
        {
            query = query.Where(q => q.Id != id.Value);
        }
        
        return !query.Any();
    }
}
