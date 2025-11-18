using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFramework.AppDbContext;

/// <summary>
/// 只读数据库上下文
/// </summary>
public abstract class ReadonlyDbContext : ContextBase
{
    public ReadonlyDbContext(DbContextOptions options)
        : base(options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("read-only context can't save data");
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException("read-only context can't save data");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 全局过滤
        base.OnModelCreating(builder);
    }
}
