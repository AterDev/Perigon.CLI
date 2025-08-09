using Microsoft.EntityFrameworkCore.Design;

namespace EntityFramework.DBProvider;

public class DefaultDesignTimeContext : IDesignTimeDbContextFactory<DefaultDbContext>
{
    public DefaultDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder.UseSqlite();

        return new DefaultDbContext(optionsBuilder.Options);
    }
}
