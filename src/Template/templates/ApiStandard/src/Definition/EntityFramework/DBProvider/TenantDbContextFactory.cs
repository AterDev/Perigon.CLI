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
    DefaultDbContext db,
    IOptions<ComponentOption> options
)
{
    public DefaultDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<DefaultDbContext>();
        Guid tenantId = tenantProvider.TenantId;

        // 从缓存中查询连接字符串
        var connectionStrings = cache
            .GetOrCreateAsync(
                $"{tenantId}_ConnectionString",
                async cancel => await GetTenantConnectionStringAsync(tenantId)
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
        return new DefaultDbContext(builder.Options);
    }

    /// <summary>
    /// 获取租户的连接字符串
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    private async ValueTask<string?> GetTenantConnectionStringAsync(Guid tenantId)
    {
        var tenant = await db.Tenants.Where(t => t.TenantId == tenantId).FirstOrDefaultAsync();
        if (tenant == null)
        {
            return null;
        }
        return tenant.DbConnectionString;
    }
}
