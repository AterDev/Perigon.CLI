

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Share.Implement;

/// <summary>
/// without any implement
/// </summary>
/// <param name="logger"></param>
public class ManagerBase(ILogger logger)
{
    protected ILogger _logger = logger;
}

/// <summary>
/// ManagerBase for QueryDbContext and CommandDbContext
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public class ManagerBase<TEntity> : ManagerBase<QueryDbContext, CommandDbContext, TEntity>
    where TEntity : class, IEntityBase
{
    /// <summary>
    /// use DataAccessContext
    /// </summary>
    /// <param name="dataAccess"></param>
    /// <param name="logger"></param>
    public ManagerBase(DataAccessContext<TEntity> dataAccess, ILogger logger) : base(dataAccess.QueryContext, dataAccess.CommandContext, logger)
    {
    }
}

/// <summary>
/// specific DbContext
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <param name="context"></param>
/// <param name="logger"></param>
public class ManagerBase<TContext, TEntity>(TContext context, ILogger logger) : ManagerBase<TContext, TContext, TEntity>(context, context, logger)
    where TContext : DbContext
    where TEntity : class, IEntityBase
{

}

/// <summary>
/// 实现类
/// </summary>
/// <typeparam name="TQueryContext"></typeparam>
/// <typeparam name="TCommandContext"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public class ManagerBase<TQueryContext, TCommandContext, TEntity>
    where TQueryContext : DbContext
    where TCommandContext : DbContext
    where TEntity : class, IEntityBase
{
    #region Properties and Fields
    /// <summary>
    /// 自动日志类型
    /// </summary>
    protected LogActionType AutoLogType { get; private set; } = LogActionType.None;

    /// <summary>
    /// 全局筛选
    /// </summary>
    public bool EnableGlobalQuery { get; set; } = true;

    /// <summary>
    /// 是否自动保存(调用SaveChanges)
    /// </summary>
    protected bool AutoSave { get; set; } = true;
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    ///错误状态码
    /// </summary>
    public int ErrorStatus { get; set; }
    /// <summary>
    /// 当前实体
    /// </summary>
    public TEntity? CurrentEntity { get; set; }
    #endregion

    protected DatabaseFacade Database { get; init; }
    /// <summary>
    /// 实体的只读仓储实现
    /// </summary>
    protected DbSet<TEntity> Query { get; init; }
    protected DbSet<TEntity> Command { get; init; }
    protected IQueryable<TEntity> Queryable { get; set; }

    protected readonly ILogger _logger;
    protected TCommandContext CommandContext { get; init; }
    protected TQueryContext QueryContext { get; init; }

    public ManagerBase(TQueryContext queryContext, TCommandContext commandContext, ILogger logger)
    {
        _logger = logger;
        CommandContext = commandContext;
        QueryContext = queryContext;
        Database = CommandContext.Database;
        Query = QueryContext.Set<TEntity>();
        Command = CommandContext.Set<TEntity>();
        Queryable = Query.AsNoTracking().AsQueryable();
        if (!EnableGlobalQuery)
        {
            Queryable = Queryable.IgnoreQueryFilters();
        }
    }

    /// <summary>
    /// 在修改前查询对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<TEntity?> GetCurrentAsync(Guid id)
    {
        return await Command.FindAsync(id);
    }

    /// <summary>
    /// Command Entity
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public async Task<TDto?> GetCurrentAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null) where TDto : class
    {
        if (typeof(TDto) == typeof(TEntity))
        {
            var model = await Command.Where(whereExp ?? (e => true))
                .FirstOrDefaultAsync();
            return model as TDto;
        }
        else
        {
            return await Command.Where(whereExp ?? (e => true))
                .ProjectTo<TDto>()
                .FirstOrDefaultAsync();
        }
    }

    /// <summary>
    /// 获取实体
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<TEntity?> FindAsync(Guid id)
    {
        var entity = await Query.FindAsync(id);
        if (entity != null)
        {
            Command.Attach(entity);
        }
        return entity;
    }

    /// <summary>
    /// 实体查询
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null) where TDto : class
    {
        var model = await Query.AsNoTracking()
            .Where(whereExp ?? (e => true))
            .ProjectTo<TDto>()
            .FirstOrDefaultAsync();

        if (typeof(TDto) is TEntity && model != null)
        {
            Command.Attach((model as TEntity)!);
        }
        return model;
    }

    /// <summary>
    /// id是否存在
    /// </summary>
    /// <param name="id">主键id</param>
    /// <returns></returns>
    public virtual async Task<bool> ExistAsync(Guid id)
    {
        return await Query.AnyAsync(q => q.Id == id);
    }

    /// <summary>
    /// 存在判断
    /// </summary>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExp)
    {
        return await Query.AnyAsync(whereExp);
    }

    /// <summary>
    /// 条件查询列表
    /// </summary>
    /// <typeparam name="TDto">返回类型</typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public async Task<List<TDto>> ToListAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null) where TDto : class
    {
        return await Query.AsNoTracking()
            .Where(whereExp ?? (e => true))
            .ProjectTo<TDto>()
            .ToListAsync();
    }

    public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>>? whereExp = null)
    {
        return await Query.AsNoTracking()
            .Where(whereExp ?? (e => true))
            .ToListAsync();
    }

    /// <summary>
    /// 分页筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<PageList<TItem>> ToPageAsync<TFilter, TItem>(TFilter filter) where TFilter : FilterBase where TItem : class
    {
        Queryable = filter.OrderBy != null
            ? Queryable.OrderBy(filter.OrderBy)
            : Queryable.OrderByDescending(t => t.CreatedTime);

        var count = Queryable.Count();
        List<TItem> data = await Queryable
            .AsNoTracking()
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<TItem>()
            .ToListAsync();

        ResetQuery();
        return new PageList<TItem>
        {
            Count = count,
            Data = data,
            PageIndex = filter.PageIndex
        };
    }

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(TEntity entity)
    {
        await Command.AddAsync(entity);
        if (AutoSave)
        {
            return await SaveChangesAsync() > 0;
        }
        return true;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">已跟踪的实体</param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(TEntity entity)
    {
        Command.Update(entity);
        if (AutoSave)
        {
            return await SaveChangesAsync() > 0;
        }
        return true;
    }

    /// <summary>
    /// 更新关联数据
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="entity">当前实体</param>
    /// <param name="propertyExpression">导航属性</param>
    /// <param name="data">新数据</param>
    public void UpdateRelation<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, List<TProperty> data) where TProperty : class
    {
        var currentValue = CommandContext.Entry(entity).Collection(propertyExpression).CurrentValue;
        if (currentValue != null && currentValue.Any())
        {
            CommandContext.RemoveRange(currentValue);
            CommandContext.Entry(entity).Collection(propertyExpression).CurrentValue = null;
        }
        CommandContext.AddRange(data);
    }

    /// <summary>
    /// 批量覆盖保存,id相同时更新，否则新增或删除
    /// </summary>
    /// <param name="entityList">新的全量数据</param>
    /// <returns></returns>
    public async Task<bool> SaveAsync(List<TEntity> entityList)
    {
        var Ids = await Command.Select(e => e.Id).ToListAsync();
        // new entity by id
        var newEntities = entityList.Where(d => !Ids.Contains(d.Id)).ToList();

        var updateEntities = entityList.Where(d => Ids.Contains(d.Id)).ToList();
        var removeEntities = Ids.Where(d => !entityList.Select(e => e.Id).Contains(d)).ToList();

        if (newEntities.Any())
        {
            await Command.AddRangeAsync(newEntities);
        }
        if (updateEntities.Any())
        {
            Command.UpdateRange(updateEntities);
        }
        try
        {
            if (removeEntities.Any())
            {
                await Command.Where(d => removeEntities.Contains(d.Id)).ExecuteDeleteAsync();
            }
            _ = await SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddOrUpdateAsync");
            return false;
        }
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ids">实体id</param>
    /// <param name="softDelete">是否软件删除</param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        var res = softDelete
            ? await Command.Where(d => ids.Contains(d.Id))
                .ExecuteUpdateAsync(d => d.SetProperty(d => d.IsDeleted, true))
            : await Command.Where(d => ids.Contains(d.Id)).ExecuteDeleteAsync();
        return res > 0;
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(TEntity entity, bool softDelete = true)
    {
        if (softDelete)
        {
            entity.IsDeleted = true;
        }
        else
        {
            Command.Remove(entity);
        }
        return await SaveChangesAsync() > 0;
    }

    /// <summary>
    /// 加载导航数据
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="entity"></param>
    /// <param name="propertyExpression"></param>
    /// <returns></returns>
    public async Task LoadAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression) where TProperty : class
    {
        var entry = CommandContext.Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            await CommandContext.Entry(entity).Reference(propertyExpression).LoadAsync();
        }
        else
        {
            await QueryContext.Entry(entity).Reference(propertyExpression)
                .Query().AsNoTracking()
                .LoadAsync();
        }
    }

    /// <summary>
    /// 加载关联数据
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="entity"></param>
    /// <param name="propertyExpression"></param>
    /// <returns></returns>
    public async Task LoadManyAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression) where TProperty : class
    {
        var entry = CommandContext.Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            await CommandContext.Entry(entity).Collection(propertyExpression).LoadAsync();
        }
        else
        {
            await QueryContext.Entry(entity).Collection(propertyExpression)
                .Query().AsNoTracking()
                .LoadAsync();
        }
    }

    protected async Task<int> SaveChangesAsync()
    {
        return await CommandContext.SaveChangesAsync();
    }

    /// <summary>
    /// reset queryable
    /// </summary>
    protected void ResetQuery()
    {
        Queryable = EnableGlobalQuery
            ? Query.AsQueryable()
            : Queryable.IgnoreQueryFilters().AsQueryable();
    }
}