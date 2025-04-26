using Entity.UserMod;
using EntityFramework.DBProvider;
using Framework.Common.Options;

namespace SharedModule.Managers;

/// <summary>
/// test manager
/// </summary>
/// <param name="factory"></param>
/// <param name="dbFactory"></param>
/// <param name="logger"></param>
public class TestManager(
    TenantDbContextFactory factory,
    DbContextFactory dbFactory,
    ILogger<TestManager> logger)
    : ManagerBase<User>(factory, logger)
{

    public async Task MultiDatabase()
    {
        var mssqlDb = dbFactory.CreateDbContext<CommandDbContext>();
        mssqlDb.Database.SetCommandTimeout(30);
        var tenant = await mssqlDb.Tenants.FirstOrDefaultAsync();


        var pgsqlDb = dbFactory.CreateDbContext<QueryDbContext>(DatabaseType.PostgreSql);
        pgsqlDb.Database.SetCommandTimeout(30);
        var user = await pgsqlDb.Tenants.FirstOrDefaultAsync(u => u.TenantId == tenant.TenantId);
    }
}
