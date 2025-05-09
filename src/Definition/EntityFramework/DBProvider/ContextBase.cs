using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EntityFramework.DBProvider;

public class ContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<ModelProperty> ModelProperties { get; set; }
    public DbSet<ApiDocInfo> ApiDocInfos { get; set; } = null!;
    public DbSet<GenAction> GenActions { get; set; } = null!;
    public DbSet<GenStep> GenSteps { get; set; } = null!;
    public DbSet<ConfigData> Configs { get; set; } = null!;
    public DbSet<GenActionGenStep> GenActionGenSteps { get; set; } = null!;
    public DbSet<GenActionTpl> GenActionTpls { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Project>(e =>
        {
            e.OwnsOne(p => p.Config).ToJson();
            e.HasMany(p => p.GenActions)
                .WithOne(a => a.Project)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.GenSteps)
                .WithOne(a => a.Project)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GenAction>(e =>
        {
            e.OwnsMany(a => a.Variables).ToJson();

            e.HasMany(e => e.GenSteps)
                .WithMany(e => e.GenActions)
                .UsingEntity<GenActionGenStep>();
        });

        OnSQLiteModelCreating(builder);
        OnModelExtendCreating(builder);
        base.OnModelCreating(builder);
    }


    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entityEntry in entries)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property = entityEntry.Metadata.FindProperty("CreatedTime");
            if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entityEntry.Property("CreatedTime").CurrentValue = DateTimeOffset.UtcNow;
            }
        }
        entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entityEntry in entries)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property = entityEntry.Metadata.FindProperty("UpdatedTime");
            if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entityEntry.Property("UpdatedTime").CurrentValue = DateTimeOffset.UtcNow;

            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private void OnModelExtendCreating(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes = modelBuilder.Model.GetEntityTypes();
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes)
        {
            if (typeof(IEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name).HasKey("Id");
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression<IEntityBase>(e => !e.IsDeleted, entityType.ClrType));
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression<TInterface>(Expression<Func<TInterface, bool>> filterExpression, Type entityType)
    {
        ParameterExpression newParam = Expression.Parameter(entityType);
        Expression newBody = ReplacingExpressionVisitor.Replace(filterExpression.Parameters.Single(), newParam, filterExpression.Body);

        return Expression.Lambda(newBody, newParam);
    }

    /// <summary>
    /// sqlite的兼容处理
    /// </summary>
    /// <param name="modelBuilder"></param>
    private void OnSQLiteModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                IEnumerable<System.Reflection.PropertyInfo> properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
                foreach (System.Reflection.PropertyInfo? property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToStringConverter());
                }
            }
        }
    }

}
