using Microsoft.Extensions.Configuration;

namespace EntityFramework.AppDbFactory;

/// <summary>
/// create universal DbContext what you want
/// </summary>
/// <remarks>you can custom this factory</remarks>
/// <param name="configuration"></param>
public class UniversalDbFactory(IConfiguration configuration)
{
    public TContext CreateDbContext<TContext>(DatabaseType databaseType = DatabaseType.PostgreSql)
        where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        if (contextName.EndsWith("DbContext"))
        {
            contextName = contextName[..^"DbContext".Length];
        }
        var connectionStrings = configuration.GetConnectionString(contextName);
        if (string.IsNullOrEmpty(connectionStrings))
        {
            throw new Exception($"Connection string for {contextName} not found.");
        }
        var builder = new DbContextOptionsBuilder<TContext>();

        switch (databaseType)
        {
            case DatabaseType.SqlServer:
                builder.UseSqlServer(connectionStrings);
                break;
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(connectionStrings);
                break;
            default:
                throw new NotSupportedException(
                    $"Database provider {databaseType} is not supported."
                );
        }

        try
        {
            var context = (TContext?)Activator.CreateInstance(typeof(TContext), builder.Options);
            return context
                ?? throw new InvalidOperationException(
                    $"Failed to create an instance of {contextName} using Activator.CreateInstance."
                );
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException(
                $"Could not find a constructor on '{contextName}' that accepts 'DbContextOptions<{contextName}>'. Ensure the constructor exists.",
                ex
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred while creating an instance of {contextName}.",
                ex
            );
        }
    }
}
