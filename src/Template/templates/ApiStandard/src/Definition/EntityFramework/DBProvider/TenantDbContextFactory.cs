using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace EntityFramework.DBProvider;

/// <summary>
/// factory for create DbContext for tenant
/// </summary>
/// <param name="tenantProvider"></param>
/// <param name="cache"></param>
/// <param name="configuration"></param>
public class TenantDbContextFactory(
    ITenantProvider tenantProvider,
    HybridCache cache,
    CommandDbContext db,
    IOptions<ComponentOption> options
)
{
    public CommandDbContext CreateCommandDbContext()
    {
        var builder = new DbContextOptionsBuilder<CommandDbContext>();
        Guid tenantId = tenantProvider.TenantId;

        // 从缓存中查询连接字符串
        var connectionStrings = cache
            .GetOrCreateAsync(
                $"{tenantId}_CommandConnectionString",
                async cancel => await GetTenantConnectionStringAsync(tenantId, "command")
            )
            .AsTask()
            .Result;

        switch (options?.Value.Database)
        {
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(connectionStrings);
                break;
            case DatabaseType.SqlServer:
                builder.UseSqlServer(connectionStrings);
                break;
        }
        return new CommandDbContext(builder.Options);
    }

    public QueryDbContext CreateQueryDbContext()
    {
        var builder = new DbContextOptionsBuilder<QueryDbContext>();
        Guid tenantId = tenantProvider.TenantId;

        // 从缓存中查询连接字符串
        var connectionStrings = cache
            .GetOrCreateAsync(
                $"{tenantId}_QueryConnectionString",
                async cancel => await GetTenantConnectionStringAsync(tenantId, "query")
            )
            .AsTask()
            .Result;

        builder.UseSqlServer(connectionStrings);
        return new QueryDbContext(builder.Options);
    }

    /// <summary>
    /// 获取租户的连接字符串
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="database"></param>
    /// <returns></returns>
    private async ValueTask<string?> GetTenantConnectionStringAsync(Guid tenantId, string database)
    {
        var tenant = await db.Tenants.Where(t => t.TenantId == tenantId).FirstOrDefaultAsync();
        if (tenant == null)
        {
            return null;
        }
        return database.Equals("query", StringComparison.InvariantCultureIgnoreCase)
            ? tenant.QueryDbString
            : tenant.CommandDbString;
    }
}
