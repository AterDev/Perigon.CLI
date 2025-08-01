namespace Definition.EntityFramework.DBProvider;

public class CommandDbContext(DbContextOptions<CommandDbContext> options) : ContextBase(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
