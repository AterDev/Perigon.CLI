using System.Linq.Expressions;
using Entity;

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

    /// <summary>
    /// Indicates whether to automatically call SaveChanges after operations.
    /// </summary>
    protected bool AutoSave { get; set; } = true;

    /// <summary>
    /// Error message for the last operation.
    /// </summary>
    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    /// Error status code for the last operation.
    /// </summary>
    public int ErrorStatus { get; set; }
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
    /// Gets the current entity by id without tracking.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> GetCurrentAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets the current entity or DTO by condition without tracking.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    public async Task<TDto?> GetCurrentAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null)
        where TDto : class
    {
        if (typeof(TDto) == typeof(TEntity))
        {
            var model = await _dbSet.Where(whereExp ?? (e => true)).FirstOrDefaultAsync();
            return model as TDto;
        }
        else
        {
            return await _dbSet
                .Where(whereExp ?? (e => true))
                .ProjectTo<TDto>()
                .FirstOrDefaultAsync();
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

        if (typeof(TDto) is TEntity && model != null)
        {
            _dbSet.Attach((model as TEntity)!);
        }
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
            filter.OrderBy != null
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
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public virtual async Task<bool> AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        return !AutoSave || await SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Updates an existing tracked entity.
    /// </summary>
    /// <param name="entity">Tracked entity to update</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return !AutoSave || await SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Updates related collection data for an entity.
    /// </summary>
    /// <typeparam name="TProperty">Related entity type</typeparam>
    /// <param name="entity">Current entity</param>
    /// <param name="propertyExpression">Navigation property expression</param>
    /// <param name="data">New related data</param>
    public void UpdateRelation<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression,
        List<TProperty> data
    )
        where TProperty : class
    {
        var currentValue = _dbContext.Entry(entity).Collection(propertyExpression).CurrentValue;
        if (currentValue != null && currentValue.Any())
        {
            _dbContext.RemoveRange(currentValue);
            _dbContext.Entry(entity).Collection(propertyExpression).CurrentValue = null;
        }
        _dbContext.AddRange(data);
    }

    /// <summary>
    /// Saves a list of entities, updating, adding, or removing as needed by id.
    /// </summary>
    /// <param name="entityList">New full list of entities</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> SaveAsync(List<TEntity> entityList)
    {
        var Ids = await _dbSet.Select(e => e.Id).ToListAsync();
        // new entity by id
        var newEntities = entityList.Where(d => !Ids.Contains(d.Id)).ToList();

        var updateEntities = entityList.Where(d => Ids.Contains(d.Id)).ToList();
        var removeEntities = Ids.Where(d => !entityList.Select(e => e.Id).Contains(d)).ToList();

        if (newEntities.Count != 0)
        {
            await _dbSet.AddRangeAsync(newEntities);
        }
        if (updateEntities.Count != 0)
        {
            _dbSet.UpdateRange(updateEntities);
        }
        try
        {
            if (removeEntities.Count != 0)
            {
                await _dbSet.Where(d => removeEntities.Contains(d.Id)).ExecuteDeleteAsync();
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
    /// Deletes a batch of entities by id, with optional soft delete.
    /// </summary>
    /// <param name="ids">List of entity ids</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public virtual async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        var res = softDelete
            ? await _dbSet
                .Where(d => ids.Contains(d.Id))
                .ExecuteUpdateAsync(d => d.SetProperty(d => d.IsDeleted, true))
            : await _dbSet.Where(d => ids.Contains(d.Id)).ExecuteDeleteAsync();
        return res > 0;
    }

    /// <summary>
    /// Deletes a single entity, with optional soft delete.
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(TEntity entity, bool softDelete = true)
    {
        if (softDelete)
        {
            entity.IsDeleted = true;
        }
        else
        {
            _dbSet.Remove(entity);
        }
        return await SaveChangesAsync() > 0;
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
    /// Resets the queryable to its default state, applying or ignoring global query filters.
    /// </summary>
    protected void ResetQuery()
    {
        Queryable = EnableGlobalQuery
            ? _dbSet.AsQueryable()
            : Queryable.IgnoreQueryFilters().AsQueryable();
    }
}
