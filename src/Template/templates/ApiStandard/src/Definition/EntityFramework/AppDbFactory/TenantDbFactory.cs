using Ater.AspNetCore.Constants;
using Entity.CommonMod;
using EntityFramework.AppDbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EntityFramework.AppDbFactory;

/// <summary>
/// factory for create TenantDbContext
/// </summary>
/// <param name="tenantContext"></param>
/// <param name="cache"></param>
/// <param name="configuration"></param>
public class TenantDbFactory(
    ITenantContext tenantContext,
    IConfiguration configuration,
    IOptions<ComponentOption> options
)
{
    public async Task<DefaultDbContext> CreateDbContextAsync()
    {
        var  builder  = new DbContextOptionsBuilder<DefaultDbContext>();
        Guid tenantId = tenantContext.TenantId;

        var connectionStrings = configuration.GetConnectionString(AppConst.Default);
        if (tenantContext.TenantType == TenantType
            .Independent
            .ToString())
        {
            connectionStrings = await tenantContext.GetDbConnectionStringAsync();
        }
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

    public async Task<AnalysisDbContext> CreateAnalysisDbContextAsync()
    {
        var  builder           = new DbContextOptionsBuilder<AnalysisDbContext>();
        Guid tenantId          = tenantContext.TenantId;
        var  connectionStrings = configuration.GetConnectionString(AppConst.Analysis);
        if (tenantContext.TenantType == TenantType
            .Independent
            .ToString())
        {
            connectionStrings = await tenantContext.GetDbConnectionStringAsync();
        }
        switch (options?.Value.Database)
        {
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(connectionStrings);
                break;
            case DatabaseType.SqlServer:
                builder.UseSqlServer(connectionStrings);
                break;
        }
        return new AnalysisDbContext(builder.Options);
    }
}
