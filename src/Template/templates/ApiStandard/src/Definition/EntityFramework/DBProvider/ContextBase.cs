using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EntityFramework.DBProvider;

public abstract partial class ContextBase(DbContextOptions options) : DbContext(options)
{
    protected string CreatedFieldName { get; set; } = "CreatedTime";
    protected string UpdatedFieldName { get; set; } = "UpdatedTime";
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        OnModelExtendCreating(builder);
        OnSQLiteModelCreating(builder);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        // 创建和更新时间处理
        var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entityEntry in entries)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property =
                entityEntry.Metadata.FindProperty(CreatedFieldName);
            if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entityEntry.Property(CreatedFieldName).CurrentValue = DateTimeOffset.UtcNow;
            }
        }
        entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entityEntry in entries)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property =
                entityEntry.Metadata.FindProperty(UpdatedFieldName);
            if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entityEntry.Property(UpdatedFieldName).CurrentValue = DateTimeOffset.UtcNow;
            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 设置主键Id和软删除过滤
    /// </summary>
    /// <param name="modelBuilder"></param>
    private static void OnModelExtendCreating(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes =
            modelBuilder.Model.GetEntityTypes();
        foreach (
            Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes
        )
        {
            if (typeof(IEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name).HasKey("Id");
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(
                        ConvertFilterExpression<IEntityBase>(e => !e.IsDeleted, entityType.ClrType)
                    );
            }
        }
    }

    /// <summary>
    /// sqlite的兼容处理
    /// </summary>
    /// <param name="modelBuilder"></param>
    private void OnSQLiteModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType
                    .ClrType.GetProperties()
                    .Where(p =>
                        p.PropertyType == typeof(DateTimeOffset)
                        || p.PropertyType == typeof(DateTimeOffset?)
                    );
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToStringConverter());
                }
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression<TInterface>(
        Expression<Func<TInterface, bool>> filterExpression,
        Type entityType
    )
    {
        ParameterExpression newParam = Expression.Parameter(entityType);
        Expression newBody = ReplacingExpressionVisitor.Replace(
            filterExpression.Parameters.Single(),
            newParam,
            filterExpression.Body
        );

        return Expression.Lambda(newBody, newParam);
    }
}
