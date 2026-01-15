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
    where TDbContext : MiniDbContext
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
    protected IQueryable<TEntity> Queryable { get; set; }

    protected DbSet<TEntity> _dbSet;
    protected readonly ILogger _logger;


    public ManagerBase(TDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _dbSet = _dbContext.Set<TEntity>();
        Queryable = _dbSet.AsQueryable();
    }

    /// <summary>
    /// Gets the current entity by id.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> GetCurrentAsync(int id)
    {
        return _dbSet.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Gets the current entity or DTO by condition.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    public async Task<TDto?> GetCurrentAsync<TDto>(Func<TEntity, bool>? whereExp = null)
        where TDto : class
    {
        var data = _dbSet.Where(whereExp ?? (e => true))
            .FirstOrDefault();
        return data?.MapTo<TDto>();


    }

    /// <summary>
    /// Finds the entity by id.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<TEntity?> FindAsync(int id)
    {
        return _dbSet.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Finds a DTO by condition.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    public async Task<TDto?> FindAsync<TDto>(Func<TEntity, bool>? whereExp = null)
        where TDto : class
    {
        var data = _dbSet.Where(whereExp ?? (e => true))
            .FirstOrDefault();
        return data?.MapTo<TDto>();
    }

    /// <summary>
    /// Checks if an entity with the specified id exists.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>True if exists; otherwise, false.</returns>
    public virtual async Task<bool> ExistAsync(int id)
    {
        return _dbSet.Any(q => q.Id == id);
    }

    /// <summary>
    /// Checks if any entity matches the given condition.
    /// </summary>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>True if any entity matches; otherwise, false.</returns>
    public async Task<bool> ExistAsync(Func<TEntity, bool> whereExp)
    {
        return _dbSet.Any(whereExp);
    }

    /// <summary>
    /// Gets a list of DTOs matching the condition.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>List of DTOs.</returns>
    public async Task<List<TDto>> ToListAsync<TDto>(
        Func<TEntity, bool>? whereExp = null
    ) where TDto : class
    {

        return _dbSet
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
    public async Task<List<TEntity>> ToListAsync(Func<TEntity, bool>? whereExp = null)
    {
        return _dbSet.Where(whereExp ?? (e => true)).ToList();
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
        var queryable = Queryable.AsQueryable() ?? _dbSet.AsQueryable();

        queryable =
            filter.OrderBy != null
                ? queryable.OrderBy(filter.OrderBy)
                : queryable;

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

        _dbSet.Add(entity);
        entity.CreatedTime = DateTime.Now;
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
        _dbSet.Update(entity);
        entity.UpdatedTime = DateTime.Now;
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
        var entities = _dbSet.Where(d => ids.Contains(d.Id)).ToList();
        foreach (var entity in entities)
        {
            _dbSet.Remove(entity);
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }

}
