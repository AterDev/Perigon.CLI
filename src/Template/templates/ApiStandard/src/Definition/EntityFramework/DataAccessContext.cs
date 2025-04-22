using EntityFramework.DBProvider;
using Framework.Common.Models;
using Framework.Web.Convention.EntityFramework;

namespace EntityFramework;
/// <summary>
/// 数据访问上下文
/// </summary>
public class DataAccessContext<TEntity>(CommandDbContext commandDbContext, QueryDbContext queryDbContext)
    : DataAccessContextBase<CommandDbContext, QueryDbContext, TEntity>(commandDbContext, queryDbContext)
    where TEntity : class, IEntityBase
{
}

/// <summary>
/// DataAccessContext without TEntity
/// </summary>
/// <param name="commandDbContext"></param>
/// <param name="queryDbContext"></param>
public class DataAccessContext(CommandDbContext commandDbContext, QueryDbContext queryDbContext)
    : DataAccessContextBase<CommandDbContext, QueryDbContext>(commandDbContext, queryDbContext)

{
}