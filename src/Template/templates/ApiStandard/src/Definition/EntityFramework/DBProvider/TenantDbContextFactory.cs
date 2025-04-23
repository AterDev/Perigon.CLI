using Microsoft.Extensions.Caching.Distributed;

namespace EntityFramework.DBProvider;

public class TenantDbContextFactory(
    ITenantProvider tenantProvider, 
    IDistributedCache cache) : IDbContextFactory<CommandDbContext>
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IDistributedCache _cache = cache;

    public CommandDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<CommandDbContext>();
        Guid tenantId = _tenantProvider.TenantId;

        // 从缓存或配置中查询连接字符串
        var connectionStrings = _cache.GetString($"{tenantId}_CommandConnectionString");

        builder.UseSqlServer(connectionStrings);
        return new CommandDbContext(builder.Options);
    }
}
