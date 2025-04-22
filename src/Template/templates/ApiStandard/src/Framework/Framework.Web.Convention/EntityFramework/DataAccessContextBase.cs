using Microsoft.EntityFrameworkCore;

namespace Framework.Web.Convention.EntityFramework;
/// <summary>
/// 数据访问层抽象
/// </summary>
public class DataAccessContextBase<TCommandContext, TQueryContext, TEntity>(TCommandContext commandDbContext, TQueryContext queryDbContext) : DataAccessContextBase<TCommandContext, TQueryContext>(commandDbContext, queryDbContext)
    where TCommandContext : DbContext
    where TQueryContext : DbContext
    where TEntity : class, IEntityBase
{
}

/// <summary>
/// DataAccessContextBase without TEntity
/// </summary>
/// <typeparam name="TCommandContext"></typeparam>
/// <typeparam name="TQueryContext"></typeparam>
/// <param name="commandDbContext"></param>
/// <param name="queryDbContext"></param>
public class DataAccessContextBase<TCommandContext, TQueryContext>(TCommandContext commandDbContext, TQueryContext queryDbContext)
    where TCommandContext : DbContext
    where TQueryContext : DbContext
{
    public TQueryContext QueryContext { get; init; } = queryDbContext;
    public TCommandContext CommandContext { get; init; } = commandDbContext;
}
