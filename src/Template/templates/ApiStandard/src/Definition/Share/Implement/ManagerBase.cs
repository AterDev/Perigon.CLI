using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Entity;
using Share.Exceptions;

namespace Share.Implement;

/// <summary>
/// Base manager class without dbContext
/// </summary>
/// <param name="logger">Logger instance</param>
public abstract class ManagerBase(ILogger logger)
{
    protected ILogger _logger = logger;
}

public abstract class ManagerBase<TDbContext>(TDbContext dbContext, ILogger logger)
    : ManagerBase(logger)
    where TDbContext : DbContext
{
    protected readonly TDbContext _dbContext = dbContext;
}

/// <summary>
/// Generic manager base class for entity operations.
/// </summary>
/// <typeparam name="TDbContext">Database context type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public abstract class ManagerBase<TDbContext, TEntity>
    where TDbContext : DbContext
    where TEntity : EntityBase
{
    #region Properties and Fields

    /// <summary>
    /// Enable or disable global query filters.
    /// </summary>
    public bool EnableGlobalQuery { get; set; } = true;
    #endregion
    protected IQueryable<TEntity> Queryable { get; set; }
    protected readonly ILogger _logger;
    protected readonly TDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the ManagerBase class.
    /// </summary>
    /// <param name="dbContext">Database context</param>
    /// <param name="logger">Logger instance</param>
    public ManagerBase(TDbContext dbContext, ILogger logger)
    {
        _logger = logger;
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
        Queryable = _dbSet.AsNoTracking().AsQueryable();
        if (!EnableGlobalQuery)
        {
            Queryable = Queryable.IgnoreQueryFilters();
        }
    }

    /// <summary>
    /// Finds and attaches the entity by id for tracking.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> FindAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Finds a DTO by condition without tracking. If TDto is TEntity, attaches the entity.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    public async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null)
        where TDto : class
    {
        var model = await _dbSet
            .AsNoTracking()
            .Where(whereExp ?? (e => true))
            .ProjectTo<TDto>()
            .FirstOrDefaultAsync();
        return model;
    }

    /// <summary>
    /// Checks if an entity with the specified id exists.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>True if exists; otherwise, false.</returns>
    public virtual async Task<bool> ExistAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id);
    }

    /// <summary>
    /// Checks if any entity matches the given condition.
    /// </summary>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>True if any entity matches; otherwise, false.</returns>
    public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExp)
    {
        return await _dbSet.AnyAsync(whereExp);
    }

    /// <summary>
    /// Gets a list of DTOs matching the condition without tracking.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>List of DTOs.</returns>
    public async Task<List<TDto>> ToListAsync<TDto>(
        Expression<Func<TEntity, bool>>? whereExp = null
    )
        where TDto : class
    {
        return await _dbSet
            .AsNoTracking()
            .Where(whereExp ?? (e => true))
            .ProjectTo<TDto>()
            .ToListAsync();
    }

    /// <summary>
    /// Gets a list of entities matching the condition without tracking.
    /// </summary>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>List of entities.</returns>
    public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>>? whereExp = null)
    {
        return await _dbSet.AsNoTracking().Where(whereExp ?? (e => true)).ToListAsync();
    }

    /// <summary>
    /// Gets a paged list of items based on the filter.
    /// </summary>
    /// <typeparam name="TFilter">Filter type</typeparam>
    /// <typeparam name="TItem">Item type</typeparam>
    /// <param name="filter">Paging and filter information</param>
    /// <returns>Paged list of items.</returns>
    public async Task<PageList<TItem>> ToPageAsync<TFilter, TItem>(TFilter filter)
        where TFilter : FilterBase
        where TItem : class
    {
        Queryable =
            filter.OrderBy != null && filter.OrderBy.Count > 0
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
            PageIndex = filter.PageIndex,
        };
    }

    /// <summary>
    /// Upsert by primary key and immediately save.
    /// </summary>
    /// <remarks></remarks>
    /// <param name="entity">The entity to insert or update. Cannot be null.</param>
    public async Task UpsertAsync(TEntity entity)
    {
        await _dbContext.BulkInsertOrUpdateAsync([entity]);
    }

    public async Task BulkUpsertAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.BulkInsertOrUpdateAsync(entities);
    }

    /// <summary>
    /// Deletes a batch of entities by id, with optional soft delete.
    /// </summary>
    /// <param name="ids">List of entity ids</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public virtual async Task<bool> DeleteAsync(IEnumerable<Guid> ids, bool softDelete = true)
    {
        var idsList = ids.ToList();
        if (idsList.Count == 0)
        {
            return false;
        }

        // 检查实体是否存在
        var existingCount = await _dbSet.CountAsync(d => idsList.Contains(d.Id));
        if (existingCount == 0)
        {
            throw new BusinessException(Localizer.EntityNotFound, StatusCodes.Status404NotFound);
        }

        var res = softDelete
            ? await _dbSet
                .Where(d => idsList.Contains(d.Id))
                .ExecuteUpdateAsync(d => d.SetProperty(d => d.IsDeleted, true))
            : await _dbSet.Where(d => idsList.Contains(d.Id)).ExecuteDeleteAsync();
        return res > 0;
    }

    /// <summary>
    /// Loads a reference navigation property for the entity.
    /// </summary>
    /// <typeparam name="TProperty">Navigation property type</typeparam>
    /// <param name="entity">Entity instance</param>
    /// <param name="propertyExpression">Navigation property expression</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public async Task LoadAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertyExpression
    )
        where TProperty : class
    {
        var entry = _dbContext.Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            await _dbContext.Entry(entity).Reference(propertyExpression).LoadAsync();
        }
        else
        {
            await _dbContext
                .Entry(entity)
                .Reference(propertyExpression)
                .Query()
                .AsNoTracking()
                .LoadAsync();
        }
    }

    /// <summary>
    /// Loads a collection navigation property for the entity.
    /// </summary>
    /// <typeparam name="TProperty">Collection property type</typeparam>
    /// <param name="entity">Entity instance</param>
    /// <param name="propertyExpression">Collection property expression</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public async Task LoadManyAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression
    )
        where TProperty : class
    {
        var entry = _dbContext.Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            await _dbContext.Entry(entity).Collection(propertyExpression).LoadAsync();
        }
        else
        {
            await _dbContext
                .Entry(entity)
                .Collection(propertyExpression)
                .Query()
                .AsNoTracking()
                .LoadAsync();
        }
    }

    /// <summary>
    /// Saves changes to the database asynchronously.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    protected async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 执行事务操作
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <returns></returns>
    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "执行事务操作时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 执行事务操作 (无返回值)
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <returns></returns>
    protected async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "执行事务操作时发生错误");
            throw;
        }
    }

    /// <summary>
    /// Resets the queryable to its default state, applying or ignoring global query filters.
    /// </summary>
    protected void ResetQuery()
    {
        Queryable = EnableGlobalQuery
            ? _dbSet.AsQueryable()
            : Queryable.IgnoreQueryFilters().AsQueryable();
    }
}
