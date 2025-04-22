using Microsoft.Extensions.Configuration;

namespace EntityFramework.DBProvider;

/// <summary>
/// create DbContext from factory
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <param name="configuration"></param>
public class DbContextFactory<TContext>(IConfiguration configuration) where TContext : DbContext
{
    public TContext CreateDbContext(DbProvider dbProvider = DbProvider.SqlServer)
    {
        var contextName = typeof(TContext).Name;

        // TODO:自定义实现获取连接字符串逻辑
        var connectionStrings = configuration.GetConnectionString(contextName);

        if (string.IsNullOrEmpty(connectionStrings))
        {
            throw new Exception($"Connection string for {contextName} not found.");
        }
        var builder = new DbContextOptionsBuilder<TContext>();

        switch (dbProvider)
        {
            case DbProvider.SqlServer:
                builder.UseSqlServer(connectionStrings);
                break;
            case DbProvider.PostgreSql:
                builder.UseNpgsql(connectionStrings);
                break;
            default:
                throw new NotSupportedException($"Database provider {dbProvider} is not supported.");
        }
        try
        {
            var context = (TContext?)Activator.CreateInstance(typeof(TContext), builder.Options);
            return context ?? throw new InvalidOperationException($"Failed to create an instance of {contextName} using Activator.CreateInstance.");
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException($"Could not find a constructor on '{contextName}' that accepts 'DbContextOptions<{contextName}>'. Ensure the constructor exists.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while creating an instance of {contextName}.", ex);
        }
    }

    public enum DbProvider
    {
        SqlServer,
        PostgreSql,
    }
}
