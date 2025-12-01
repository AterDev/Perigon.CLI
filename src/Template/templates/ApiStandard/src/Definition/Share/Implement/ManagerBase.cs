using EFCore.BulkExtensions;
using Entity;
using EntityFramework;
using EntityFramework.AppDbFactory;
using Mapster;
using System.Linq.Expressions;

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
    protected IQueryable<TEntity> Queryable { get; set; }
    protected readonly ILogger _logger;
    protected readonly TDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly IUserContext _userContext;

    public ManagerBase(
        TenantDbFactory dbContextFactory,
        IUserContext userContext,
        ILogger logger
    )
    {
        _logger      = logger;
        _dbContext   = (dbContextFactory.CreateDbContextAsync().Result as TDbContext)!;
        _userContext = userContext;
        _dbSet = _dbContext.Set<TEntity>();
        _userContext = userContext;
        Queryable = _dbSet.AsNoTracking().AsQueryable();
    }

    /// <summary>
    /// Finds and attaches the entity by id for tracking.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public async Task<TEntity?> FindAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Finds a DTO by condition without tracking. If TDto is TEntity, attaches the entity.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <returns>The DTO if found; otherwise, null.</returns>
    protected async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null)
        where TDto : class
    {
        var model = await _dbSet
            .AsNoTracking()
            .Where(e => e.TenantId == _userContext.TenantId)
            .Where(whereExp ?? (e => true))
            .ProjectToType<TDto>()
            .FirstOrDefaultAsync();
        return model;
    }

    /// <summary>
    /// Checks if an entity with the specified id exists.
    /// </summary>
    /// <param name="id">Entity id</param>
    /// <returns>True if exists; otherwise, false.</returns>
    public async Task<bool> ExistAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id);
    }

    /// <summary>
    /// Gets a list of DTOs matching the condition without tracking.
    /// </summary>
    /// <typeparam name="TDto">DTO type</typeparam>
    /// <param name="whereExp">Filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of DTOs.</returns>
    protected async Task<List<TDto>> ListAsync<TDto>(
        Expression<Func<TEntity, bool>>? whereExp = null,
        CancellationToken cancellationToken = default
    )
        where TDto : class
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.TenantId == _userContext.TenantId)
            .Where(whereExp ?? (e => true))
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a paged list of items based on the filter.
    /// </summary>
    /// <typeparam name="TFilter">Filter type</typeparam>
    /// <typeparam name="TItem">Item type</typeparam>
    /// <param name="filter">Paging and filter information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of items.</returns>
    public async Task<PageList<TItem>> PageListAsync<TFilter, TItem>(
        TFilter filter,
        CancellationToken cancellationToken = default
    )
        where TFilter : FilterBase
        where TItem : class
    {
        Queryable = Queryable.Where(e => e.TenantId == _userContext.TenantId);
        Queryable =
            filter.OrderBy != null && filter.OrderBy.Count > 0
                ? Queryable.OrderBy(filter.OrderBy)
                : Queryable.OrderByDescending(t => t.CreatedTime);

        var count = Queryable.Count();
        List<TItem> data = await Queryable
            .AsNoTracking()
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectToType<TItem>()
            .ToListAsync(cancellationToken);

        ResetQuery();
        return new PageList<TItem>
        {
            Count     = count,
            Data      = data,
            PageIndex = filter.PageIndex,
        };
    }

    /// <summary>
    /// Insert new entity to database
    /// </summary>
    /// <remarks></remarks>
    /// <param name="entity">The entity to insert or update. Cannot be null.</param>
    protected async Task InsertAsync(TEntity entity)
    {
        entity.TenantId = _userContext.TenantId;
        await _dbContext.BulkInsertAsync([entity]);
    }

    /// <summary>
    /// update entity data to database
    /// </summary>
    /// <typeparam name="TUpdateDto"></typeparam>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <param name="updateTime"></param>
    /// <returns></returns>
    protected async Task<int> UpdateAsync<TUpdateDto>(
        Guid id,
        TUpdateDto dto,
        bool updateTime = true
    )
        where TUpdateDto : class
    {
        return await _dbContext.PartialUpdateAsync<TEntity, TUpdateDto>(id, dto, updateTime);
    }

    protected async Task BulkInsertAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (TEntity entity in entities)
        {
            entity.TenantId = _userContext.TenantId;
            entity.UpdatedTime = DateTime.UtcNow;
        }
        await _dbContext.BulkInsertAsync(entities, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes a batch of entities by id, with optional soft delete.
    /// </summary>
    /// <param name="ids">List of entity ids</param>
    /// <param name="softDelete">If true, performs soft delete; otherwise, hard delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful; otherwise, false.</returns>
    protected async Task<int> DeleteOrUpdateAsync(
        IEnumerable<Guid> ids,
        bool softDelete = true,
        CancellationToken cancellationToken = default
    )
    {
        var idsList = ids.ToList();
        if (idsList.Count == 0)
        {
            return 0;
        }

        var res = softDelete
            ? await _dbSet
                .Where(d => idsList.Contains(d.Id))
                .ExecuteUpdateAsync(d => d.SetProperty(d => d.IsDeleted, true), cancellationToken)
            : await _dbSet.Where(d => idsList.Contains(d.Id)).ExecuteDeleteAsync(cancellationToken);
        return res;
    }

    /// <summary>
    /// Loads a reference navigation property for the entity.
    /// </summary>
    /// <typeparam name="TProperty">Navigation property type</typeparam>
    /// <param name="entity">Entity instance</param>
    /// <param name="propertyExpression">Navigation property expression</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    protected async Task LoadAsync<TProperty>(
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
    protected async Task LoadManyAsync<TProperty>(
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
    /// 执行事务操作
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    protected async Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _dbContext
            .Database
            .BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "执行事务操作时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 执行事务操作 (无返回值)
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    protected async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _dbContext
            .Database
            .BeginTransactionAsync(cancellationToken);
        try
        {
            await operation();
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "执行事务操作时发生错误");
            throw;
        }
    }

    public abstract Task<bool> HasPermissionAsync(Guid id);

    /// <summary>
    /// Resets the queryable to its default state, applying or ignoring global query filters.
    /// </summary>
    protected void ResetQuery()
    {
        Queryable = _dbSet.AsQueryable();
    }
}
