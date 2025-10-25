using EfCoreContext.DBProvider;

namespace EntityFramework.DBProvider;
public class DefaultDbContext(DbContextOptions<DefaultDbContext> options) : ContextBase(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
