using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using CodeGenerator.Helper;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Share.Helper;

/// <summary>
/// 加载和分析外部 DbContext
/// </summary>
public class DbContextAnalyzer : IDisposable
{
    private readonly DbContextAnalysisHelper _helper;
    private readonly string _entityFrameworkPath;
    private PluginLoadContext? _loadContext;
    private bool _disposed = false;

    public DbContextAnalyzer(string entityFrameworkPath)
    {
        _entityFrameworkPath = entityFrameworkPath;
        _helper = new DbContextAnalysisHelper(entityFrameworkPath);
    }

    public FrozenDictionary<string, IModel> GetDbContextModels()
    {
        var dict = new Dictionary<string, IModel>(StringComparer.Ordinal);
        
        try
        {
            OutputHelper.Info($"🔍 Starting to analyze DbContext models from: {_helper.DllPath}");
            
            var dbContextNames = _helper.DbContextNamedTypeSymbols.Select(s => s.ToDisplayString()).ToArray();
            OutputHelper.Info($"📋 Found {dbContextNames.Length} DbContext types: {string.Join(", ", dbContextNames)}");

            _loadContext = new PluginLoadContext(_helper.DllPath);
            OutputHelper.Info("🔧 PluginLoadContext created");
            
            Assembly assembly;
            
            try
            {
                assembly = _loadContext.LoadFromAssemblyName(
                    new AssemblyName(Path.GetFileNameWithoutExtension(_helper.DllPath))
                );
                OutputHelper.Info($"📦 Assembly loaded: {assembly.FullName}");
            }
            catch (Exception ex)
            {
                OutputHelper.Error($"❌ Failed to load assembly: {ex.Message}");
                return dict.ToFrozenDictionary();
            }

            Type[]? contextTypes;
            try
            {
                contextTypes = assembly.GetTypes();
                OutputHelper.Info($"🔍 Found {contextTypes.Length} types in assembly");
            }
            catch (ReflectionTypeLoadException ex)
            {
                contextTypes = ex.Types.Where(t => t != null).ToArray()!;
                OutputHelper.Warning($"⚠️ ReflectionTypeLoadException: {ex.Message}, got {contextTypes.Length} valid types");
            }
            
            contextTypes = contextTypes?.Where(c => dbContextNames.Contains(c.FullName)).ToArray();
            OutputHelper.Info($"🎯 Filtered to {contextTypes?.Length ?? 0} DbContext types");
            
            if (contextTypes != null)
            {
                foreach (var contextType in contextTypes)
                {
                    try
                    {
                        OutputHelper.Info($"🔄 Processing DbContext: {contextType.Name}");
                        var model = GetModel(contextType);
                        if (model != null)
                        {
                            dict[contextType.Name] = model;
                            OutputHelper.Info($"✅ Successfully processed: {contextType.Name}");
                        }
                        else
                        {
                            OutputHelper.Warning($"⚠️ Failed to get model for: {contextType.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        OutputHelper.Error($"❌ Error processing {contextType.Name}: {ex.Message}");
                    }
                }
            }
            
            OutputHelper.Info($"✅ Completed analysis. Found {dict.Count} valid models");
            return dict.ToFrozenDictionary();
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"❌ Unexpected error in GetDbContextModels: {ex.Message}");
            return dict.ToFrozenDictionary();
        }
    }

    private IModel? GetModel(Type contextType)
    {
        DbContext? dbContextInstance = null;
        IModel? model = null;
        
        try
        {
            OutputHelper.Info($"🏗️ Creating DbContext instance for: {contextType.Name}");
            
            // 1. create DbContextOptionsBuilder<TContext>
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilder = Activator.CreateInstance(optionsBuilderType) as DbContextOptionsBuilder;

            if (optionsBuilder == null)
            {
                OutputHelper.Error($"❌ Failed to create DbContextOptionsBuilder for {contextType.Name}");
                return null;
            }
            
            OutputHelper.Info($"🔧 Created DbContextOptionsBuilder for: {contextType.Name}");
            
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
                OutputHelper.Info($"🗄️ Configured SQLite for: {contextType.Name}");
            }

            var options = optionsBuilder.Options;
            dbContextInstance = Activator.CreateInstance(contextType, options) as DbContext;
            
            if (dbContextInstance != null)
            {
                OutputHelper.Info($"✅ DbContext instance created for: {contextType.Name}");
                // 在释放实例之前获取 Model
                model = dbContextInstance.Model;
                OutputHelper.Info($"📊 Model extracted for: {contextType.Name}");
            }
            else
            {
                OutputHelper.Error($"❌ Failed to create DbContext instance for: {contextType.Name}");
            }
        }
        catch (MissingMethodException ex)
        {
            OutputHelper.Error($"❌ Missing constructor for {contextType.Name}: {ex.Message}");
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"❌ Error creating model for {contextType.Name}: {ex.Message}");
        }
        finally
        {
            // 确保 DbContext 实例被释放
            if (dbContextInstance != null)
            {
                try
                {
                    dbContextInstance.Dispose();
                    OutputHelper.Info($"♻️ DbContext instance disposed for: {contextType.Name}");
                }
                catch (Exception ex)
                {
                    OutputHelper.Warning($"⚠️ Error disposing DbContext for {contextType.Name}: {ex.Message}");
                }
            }
        }

        return model;
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

    /// <summary>
    /// 强制清理程序集引用
    /// </summary>
    public static void ForceCleanup()
    {
        try
        {
            OutputHelper.Info("🧹 Starting force cleanup...");
            
            // 正常垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            OutputHelper.Info("✅ Force cleanup completed");
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"❌ Error during force cleanup: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                OutputHelper.Info("🧹 DbContextAnalyzer disposing...");
                
                if (_loadContext != null)
                {
                    OutputHelper.Info("🔄 Unloading PluginLoadContext...");
                    _loadContext.Unload();
                    _loadContext = null;
                    OutputHelper.Info("✅ PluginLoadContext unloaded");
                }
                
                // 正常垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
                
                OutputHelper.Info("✅ DbContextAnalyzer disposed successfully");
            }
            catch (Exception ex)
            {
                OutputHelper.Error($"❌ Error during DbContextAnalyzer disposal: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    ~DbContextAnalyzer()
    {
        Dispose(false);
    }
}
