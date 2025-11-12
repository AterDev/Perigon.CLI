namespace Share.Implement;

/// <summary>
/// Base manager class without dbContext
/// </summary>
/// <param name="logger">Logger instance</param>
public abstract class ManagerBase(ILogger logger)
{
    protected ILogger _logger = logger;
}

/// <summary>
/// Generic manager base class for entity operations.
/// </summary>
/// <typeparam name="TDbContext">Database context type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <remarks>
/// Initializes a new instance of the ManagerBase class with DbContextFactory.
/// </remarks>
/// <param name="dbContextFactory">Database context factory</param>
/// <param name="logger">Logger instance</param>
public abstract class ManagerBase<TDbContext, TEntity>(
    IDbContextFactory<TDbContext> dbContextFactory,
    ILogger logger
)
    where TDbContext : DbContext
    where TEntity : class, IEntityBase
{
    #region Properties and Fields

    protected readonly IDbContextFactory<TDbContext> _dbContextFactory = dbContextFactory;

    /// <summary>
    /// Enable or disable global query filters.
    /// </summary>
    public bool EnableGlobalQuery { get; set; } = true;

    /// <summary>
    /// Error message for the last operation.
    /// </summary>
    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    /// Error status code for the last operation.
    /// </summary>
    public int ErrorStatus { get; set; }
    #endregion
    protected IQueryable<TEntity> Queryable { get; set; } =
        dbContextFactory.CreateDbContext().Set<TEntity>().AsNoTracking();
    protected readonly ILogger _logger = logger;

    /// <summary>
    /// Gets the current entity by id without tracking.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> GetCurrentAsync(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return await context.Set<TEntity>().FindAsync(id);
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
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<TEntity>();
        if (typeof(TDto) == typeof(TEntity))
        {
            var model = await dbSet.Where(whereExp ?? (e => true)).FirstOrDefaultAsync();
            return model as TDto;
        }
        else
        {
            return await dbSet
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
        using var context = _dbContextFactory.CreateDbContext();
        return await context.Set<TEntity>().FindAsync(id);
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
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<TEntity>();
        var model = await dbSet
            .AsNoTracking()
            .Where(whereExp ?? (e => true))
            .ProjectTo<TDto>()
            .FirstOrDefaultAsync();

        if (typeof(TDto) is TEntity && model != null)
        {
            dbSet.Attach((model as TEntity)!);
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
        using var context = _dbContextFactory.CreateDbContext();
        return await context.Set<TEntity>().AnyAsync(q => q.Id == id);
    }

    /// <summary>
    /// Checks if any entity matches the given condition.
    /// </summary>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>True if any entity matches; otherwise, false.</returns>
    public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExp)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return await context.Set<TEntity>().AnyAsync(whereExp);
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
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<TEntity>();
        return await dbSet
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
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<TEntity>();
        return await dbSet.AsNoTracking().Where(whereExp ?? (e => true)).ToListAsync();
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
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<TEntity>();
        var queryable = Queryable ?? dbSet.AsNoTracking().AsQueryable();

        queryable =
            filter.OrderBy != null
                ? queryable.OrderBy(filter.OrderBy)
                : queryable.OrderByDescending(t => t.CreatedTime);

        var count = queryable.Count();
        List<TItem> data = await queryable
            .AsNoTracking()
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<TItem>()
            .ToListAsync();

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
    public async Task<bool> AddAsync(TEntity entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        await context.Set<TEntity>().AddAsync(entity);
        return await context.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Updates an existing tracked entity.
    /// </summary>
    /// <param name="entity">Tracked entity to update</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(TEntity entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var _dbSet = context.Set<TEntity>();

        _dbSet!.Update(entity);
        return await context.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Deletes a batch of entities by id, with optional soft delete.
    /// </summary>
    /// <param name="ids">List of entity ids</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var _dbSet = context.Set<TEntity>();
        var res = softDelete
            ? await _dbSet!
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
        using var context = _dbContextFactory.CreateDbContext();
        var _dbSet = context.Set<TEntity>();

        if (softDelete)
        {
            entity.IsDeleted = true;
        }
        else
        {
            _dbSet!.Remove(entity);
        }
        return await context.SaveChangesAsync() > 0;
    }
}
