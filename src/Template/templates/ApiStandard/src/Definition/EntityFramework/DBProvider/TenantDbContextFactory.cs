using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace EntityFramework.DBProvider;

public class TenantDbContextFactory(
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IConfiguration configuration
    )
{
    public CommandDbContext CreateCommandDbContext()
    {
        var builder = new DbContextOptionsBuilder<CommandDbContext>();
        Guid tenantId = tenantProvider.TenantId;

        // 从缓存或配置中查询连接字符串
        var connectionStrings = cache.GetString($"{tenantId}_CommandConnectionString");

        builder.UseSqlServer(connectionStrings);
        return new CommandDbContext(builder.Options);
    }


    public QueryDbContext CreateQueryDbContext()
    {
        var builder = new DbContextOptionsBuilder<QueryDbContext>();
        Guid tenantId = tenantProvider.TenantId;

        // 从缓存或配置中查询连接字符串
        var connectionStrings = cache.GetString($"{tenantId}_CommandConnectionString");

        builder.UseSqlServer(connectionStrings);
        return new QueryDbContext(builder.Options);
    }
}
