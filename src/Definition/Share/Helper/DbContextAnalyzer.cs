using System.Collections.Frozen;
using System.Reflection;
using CodeGenerator.Helper;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Share.Helper;

/// <summary>
/// 加载和分析外部 DbContext
/// </summary>
public class DbContextAnalyzer(string entityFrameworkPath)
{
    private readonly DbContextAnalysisHelper _helper = new(entityFrameworkPath);

    public FrozenDictionary<string, IModel> GetDbContextModels()
    {
        var dict = new Dictionary<string, IModel>(StringComparer.Ordinal);
        var dbContextNames = _helper
            .DbContextNamedTypeSymbols.Select(s => s.ToDisplayString())
            .ToArray();

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
            contextTypes = ex.Types.Where(t => t != null).ToArray()!;
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
                        // later entries with the same simple name overwrite earlier ones
                        dict[contextType.Name] = model;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        loadContext.Unload(); // release assembly resources
        return dict.ToFrozenDictionary();
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

    /// <summary>
    /// 获取包含某个实体类型的DbContext
    /// </summary>
    /// <param name="entityName">实体类型名称</param>
    /// <returns></returns>
    public INamedTypeSymbol? GetDbContextType(string entityName)
    {
        return _helper.GetDbContextType(entityName);
    }
}
