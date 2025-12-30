using Entity;
using Mapster;
using Perigon.MiniDb;

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
/// Generic manager base class for entity operations with MiniDb.
/// </summary>
/// <typeparam name="TDbContext">Database context type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public abstract class ManagerBase<TDbContext, TEntity>
    where TDbContext : MicroDbContext
    where TEntity : EntityBase
{
    #region Properties and Fields

    /// <summary>
    /// Error message for the last operation.
    /// </summary>
    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    /// Error status code for the last operation.
    /// </summary>
    public int ErrorStatus { get; set; }
    #endregion

    protected readonly TDbContext _dbContext;
    protected IEnumerable<TEntity> Queryable { get; set; }
    protected readonly ILogger _logger;

    protected abstract IEnumerable<TEntity> GetCollection();

    public ManagerBase(TDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        Queryable = GetCollection();
    }

    /// <summary>
    /// Gets the current entity by id.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> GetCurrentAsync(int id)
    {
        var collection = GetCollection();
        return collection.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Gets the current entity or DTO by condition.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    public async Task<TDto?> GetCurrentAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null)
        where TDto : class
    {
        var collection = GetCollection();
        if (typeof(TDto) == typeof(TEntity))
        {

            var model = collection.Where(whereExp ?? (e => true)).FirstOrDefault();
            return model as TDto;
        }
        else
        {
            return collection
                .Where(whereExp ?? (e => true))
                .AsQueryable()
                .ProjectToType<TDto>()
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Finds the entity by id.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> FindAsync(int id)
    {
        var collection = GetCollection();
        return collection.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Finds a DTO by condition.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    public async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null)
        where TDto : class
    {
        var collection = GetCollection();
        var model = collection
            .Where(whereExp ?? (e => true))
            .AsQueryable()
            .ProjectToType<TDto>()
            .FirstOrDefault();
        return model;
    }

    /// <summary>
    /// Checks if an entity with the specified id exists.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>True if exists; otherwise, false.</returns>
    public virtual async Task<bool> ExistAsync(int id)
    {
        return GetCollection().Any(q => q.Id == id);
    }

    /// <summary>
    /// Checks if any entity matches the given condition.
    /// </summary>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>True if any entity matches; otherwise, false.</returns>
    public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExp)
    {
        return GetCollection().Any(whereExp);
    }

    /// <summary>
    /// Gets a list of DTOs matching the condition.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>List of DTOs.</returns>
    public async Task<List<TDto>> ToListAsync<TDto>(
        Expression<Func<TEntity, bool>>? whereExp = null
    )
        where TDto : class
    {
        var collection = GetCollection();
        return collection
            .Where(whereExp ?? (e => true))
            .AsQueryable()
            .ProjectToType<TDto>()
            .ToList();
    }

    /// <summary>
    /// Gets a list of entities matching the condition.
    /// </summary>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>List of entities.</returns>
    public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>>? whereExp = null)
    {
        var collection = GetCollection();
        return collection.Where(whereExp ?? (e => true)).ToList();
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
        var collection = GetCollection();
        var queryable = Queryable.AsQueryable() ?? collection.AsQueryable();

        queryable =
            filter.OrderBy != null
                ? queryable.OrderBy(filter.OrderBy)
                : queryable.OrderByDescending(t => t.CreatedTime);

        var count = queryable.Count();
        List<TItem> data = queryable
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectToType<TItem>()
            .ToList();

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
        var collection = GetCollection();
        collection.Add(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(TEntity entity)
    {
        var collection = GetCollection();
        var existing = collection.FirstOrDefault(e => e.Id == entity.Id);
        if (existing != null)
        {
            collection.Remove(existing);
            collection.Add(entity);
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a batch of entities by id, with optional soft delete.
    /// </summary>
    /// <param name="ids">List of entity ids</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(List<int> ids, bool softDelete = true)
    {
        var collection = GetCollection();
        var entities = collection.Where(d => ids.Contains(d.Id)).ToList();

        if (softDelete)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                collection.Remove(entity);
                collection.Add(entity);
            }
        }
        else
        {
            foreach (var entity in entities)
            {
                collection.Remove(entity);
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a single entity, with optional soft delete.
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(TEntity entity, bool softDelete = true)
    {
        var collection = GetCollection();

        if (softDelete)
        {
            entity.IsDeleted = true;
            collection.Remove(entity);
            collection.Add(entity);
        }
        else
        {
            collection.Remove(entity);
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
