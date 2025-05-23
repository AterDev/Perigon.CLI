using Ater.Web.Convention.Abstraction;
using Microsoft.Extensions.Caching.Distributed;

namespace EntityFramework.DBProvider;
public class QueryDbContextFactory(ITenantProvider tenantProvider, IDistributedCache cache) : IDbContextFactory<QueryDbContext>
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IDistributedCache _cache = cache;

    public QueryDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<QueryDbContext>();
        Guid tenantId = _tenantProvider.TenantId;

        // 从缓存或配置中查询连接字符串
        var connectionStrings = _cache.GetString($"{tenantId}_QueryConnectionString");

        builder.UseSqlServer(connectionStrings);
        return new QueryDbContext(builder.Options);
    }
}
