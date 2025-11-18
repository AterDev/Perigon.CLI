using Entity.CommonMod;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EntityFramework.AppDbContext;

public abstract partial class ContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Tenant>().Ignore(t => t.TenantId);

        base.OnModelCreating(builder);
        OnModelExtendCreating(builder);
        ConfigureMultiTenantUniqueIndexes(builder);
        OnSQLiteModelCreating(builder);
    }

    /// <summary>
    /// 设置主键Id和软删除过滤器
    /// </summary>
    /// <param name="modelBuilder"></param>
    private void OnModelExtendCreating(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes =
            modelBuilder.Model.GetEntityTypes();
        foreach (
            Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes
        )
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name).HasKey(nameof(EntityBase.Id));
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(
                        ConvertFilterExpression<EntityBase>(e => !e.IsDeleted, entityType.ClrType)
                    );
            }
        }
    }

    /// <summary>
    /// 为所有唯一索引添加 TenantId 和软删除过滤器
    /// </summary>
    private void ConfigureMultiTenantUniqueIndexes(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder
            .Model.GetEntityTypes()
            .Where(e => typeof(EntityBase).IsAssignableFrom(e.ClrType))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // 检查该实体是否有 TenantId 属性（未被忽略）
            var tenantIdProperty = entityType.FindProperty(nameof(EntityBase.TenantId));
            if (tenantIdProperty == null)
            {
                continue;
            }

            foreach (var index in entityType.GetIndexes())
            {
                var propertyNames = new List<string> { nameof(EntityBase.TenantId) };
                // 添加原索引的属性，确保 TenantId 在第一位
                foreach (var property in index.Properties)
                {
                    if (property.Name != nameof(EntityBase.TenantId))
                    {
                        propertyNames.Add(property.Name);
                    }
                }

                if (index.IsUnique)
                {
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .HasIndex([.. propertyNames])
                        .IsUnique()
                        .HasFilter($"\"{nameof(EntityBase.IsDeleted)}\" = 0");
                }
                else
                {
                    modelBuilder.Entity(entityType.ClrType).HasIndex([.. propertyNames]);
                }
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
