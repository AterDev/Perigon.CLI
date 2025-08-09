using System.Reflection;
using CodeGenerator.Helper;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Share.Helper;

/// <summary>
/// 加载和分析外部 DbContext
/// </summary>
public class ExternalDbContextAnalyzer
{
    private readonly Assembly _assembly;

    public ExternalDbContextAnalyzer(string assemblyPath)
    {
        var loadContext = new PluginLoadContext(assemblyPath);
        _assembly = loadContext.LoadFromAssemblyName(
            new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath))
        );
    }

    public Dictionary<string, IModel> GetDbContextModels(string baseContextTypeName)
    {
        var result = new Dictionary<string, IModel>();
        var contextTypes = GetDbContextTypes(baseContextTypeName);

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
        return result;
    }

    public void AnalyzeModel(IModel model)
    {
        Console.WriteLine("--- Analyzing DbContext Model ---");

        // 1. Get all entity types from the model
        var entityTypes = model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            Console.WriteLine($"\nEntity: {entityType.DisplayName()}");

            // 2. Get properties of the entity
            Console.WriteLine("  Properties:");
            foreach (var property in entityType.GetProperties())
            {
                var propertyType = property.ClrType.Name;
                var isNullable = property.IsNullable ? "?" : "";

                Console.WriteLine($"    - {property.Name}: {propertyType}{isNullable}");
            }

            // Get the primary key
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                var pkProperties = string.Join(", ", primaryKey.Properties.Select(p => p.Name));
                Console.WriteLine($"  Primary Key: ({pkProperties})");
            }

            // 3. Get relationships (foreign keys)
            Console.WriteLine("  Relationships (Foreign Keys):");
            var foreignKeys = entityType.GetForeignKeys();
            if (!foreignKeys.Any())
            {
                Console.WriteLine("    (None)");
            }
            else
            {
                foreach (var foreignKey in foreignKeys)
                {
                    var principalEntityType = foreignKey.PrincipalEntityType.DisplayName();
                    var foreignKeyProperties = string.Join(
                        ", ",
                        foreignKey.Properties.Select(p => p.Name)
                    );
                    Console.WriteLine(
                        $"    - FK to {principalEntityType} on ({foreignKeyProperties})"
                    );
                }
            }

            // Get navigation properties
            Console.WriteLine("  Navigation Properties:");
            var navigations = entityType.GetNavigations();
            if (!navigations.Any())
            {
                Console.WriteLine("    (None)");
            }
            else
            {
                foreach (var navigation in navigations)
                {
                    var targetEntity = navigation.TargetEntityType.DisplayName();
                    var relationshipType = navigation.IsCollection ? "Collection" : "Reference";
                    Console.WriteLine(
                        $"    - {navigation.Name}: {relationshipType} to {targetEntity}"
                    );
                }
            }
        }
        Console.WriteLine("\n--- Analysis Complete ---");
    }

    private IModel? GetModel(Type contextType)
    {
        DbContext? dbContextInstance = null;
        try
        {
            // 1. 创建泛型 DbContextOptionsBuilder<TContext>
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilder =
                Activator.CreateInstance(optionsBuilderType) as DbContextOptionsBuilder;

            if (optionsBuilder == null)
            {
                // 如果创建失败，则无法继续
                return null;
            }

            var sqliteAssembly = Assembly.Load("Microsoft.EntityFrameworkCore.Sqlite");
            var sqliteExtensionsType = sqliteAssembly.GetType(
                "Microsoft.EntityFrameworkCore.SqliteDbContextOptionsBuilderExtensions"
            );

            if (sqliteExtensionsType != null)
            {
                var useSqliteMethod = sqliteExtensionsType.GetMethod(
                    "UseSqlite",
                    [
                        optionsBuilderType,
                        typeof(string),
                        typeof(Action<object>), // 使用 object 以增加兼容性
                    ]
                );

                if (useSqliteMethod != null)
                {
                    // Invoke the extension method as a static method
                    useSqliteMethod.Invoke(null, [optionsBuilder, "DataSource=:memory:", null]);
                }
            }

            // 3. 直接从泛型 builder 获取泛型 options
            var options = optionsBuilder.Options;

            // 4. 使用配置好的 options 创建 DbContext 实例
            dbContextInstance = Activator.CreateInstance(contextType, options) as DbContext;
        }
        catch (MissingMethodException)
        {
            // This exception can be expected if a suitable constructor is not found.
            // We can ignore it and let the method return null if no other strategy works.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting model for {contextType.Name}: {ex}");
            // Decide whether to rethrow or handle
        }

        return dbContextInstance?.Model;
    }

    /// <summary>
    /// 从已加载的程序集中查找所有继承自指定基类的 DbContext 类型。
    /// </summary>
    /// <param name="baseContextTypeName">DbContext 基类的完全限定名。</param>
    /// <returns>符合条件的 DbContext 类型列表。</returns>
    private List<Type> GetDbContextTypes(string baseContextTypeName)
    {
        var contextTypes = new List<Type>();
        try
        {
            // Since we loaded the assembly, we can get the type from it.
            var baseType =
                _assembly.GetTypes().FirstOrDefault(t => t.FullName == baseContextTypeName)
                ?? Type.GetType(baseContextTypeName);

            if (baseType == null)
            {
                // Could not find the base type, return empty list
                return contextTypes;
            }

            foreach (var type in _assembly.GetTypes())
            {
                if (
                    type.IsClass
                    && !type.IsAbstract
                    && type.IsPublic
                    && baseType.IsAssignableFrom(type)
                )
                {
                    contextTypes.Add(type);
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine("Failed to load types from assembly: " + _assembly.FullName);
            foreach (var loaderException in ex.LoaderExceptions)
            {
                Console.WriteLine(loaderException?.Message);
            }
        }
        return contextTypes;
    }
}
