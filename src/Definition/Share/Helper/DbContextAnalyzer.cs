using System.Reflection;
using CodeGenerator.Helper;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Share.Helper;

/// <summary>
/// 加载和分析外部 DbContext
/// </summary>
public class ExternalDbContextAnalyzer
{
    private readonly DbContextAnalysisHelper _helper;

    public ExternalDbContextAnalyzer(string entityFrameworkPath)
    {
        _helper = new DbContextAnalysisHelper(entityFrameworkPath);
        //_assembly = loadContext.LoadFromAssemblyName(
        //    new AssemblyName(Path.GetFileNameWithoutExtension(entityFrameworkPath))
        //);
    }

    public Dictionary<string, IModel> GetDbContextModels()
    {
        var result = new Dictionary<string, IModel>();
        var dbContextNames = _helper.DbContextNames.Select(s => s.ToDisplayString()).ToArray();

        var loadContext = new PluginLoadContext(_helper.DllPath);
        var assembly = loadContext.LoadFromAssemblyName(
            new AssemblyName(Path.GetFileNameWithoutExtension(_helper.DllPath))
        );

        Type[]? contextTypes;
        try
        {
            contextTypes = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            contextTypes = ex.Types.Where(t => t != null).ToArray();
        }
        contextTypes = contextTypes?.Where(c => dbContextNames.Contains(c.FullName)).ToArray();
        if (contextTypes != null)
        {
            foreach (var contextType in contextTypes)
            {
                try
                {
                    var model = GetModel(contextType);
                    if (model != null)
                    {
                        result.Add(contextType.Name, model);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        loadContext.Unload(); // release assembly resources
        return result;
    }

    private IModel? GetModel(Type contextType)
    {
        DbContext? dbContextInstance = null;
        try
        {
            // 1. create DbContextOptionsBuilder<TContext>
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilder =
                Activator.CreateInstance(optionsBuilderType) as DbContextOptionsBuilder;

            if (optionsBuilder == null)
            {
                return null;
            }
            // use tool sqlite assembly
            var sqliteAssembly = Assembly.Load("Microsoft.EntityFrameworkCore.Sqlite");
            var sqliteExtensionsType = sqliteAssembly.GetType(
                "Microsoft.EntityFrameworkCore.SqliteDbContextOptionsBuilderExtensions"
            );

            if (sqliteExtensionsType != null)
            {
                var useSqliteMethod = sqliteExtensionsType.GetMethod(
                    "UseSqlite",
                    [optionsBuilderType, typeof(string), typeof(Action<object>)]
                );

                useSqliteMethod?.Invoke(null, [optionsBuilder, "DataSource=temp", null]);
            }

            var options = optionsBuilder.Options;
            dbContextInstance = Activator.CreateInstance(contextType, options) as DbContext;
        }
        catch (MissingMethodException)
        {
            OutputHelper.Error(
                $"Failed to create DbContext instance for {contextType.Name}. Ensure it has a constructor accepting DbContextOptions."
            );
            return null;
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"Error getting model for {contextType.Name}: {ex}");
            return null;
        }

        return dbContextInstance?.Model;
    }
}
