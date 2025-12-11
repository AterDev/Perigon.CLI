using System.Collections.Frozen;
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
    private WeakReference? _alcWeakRef;
    private string? _shadowDir;
    private bool _disposed;

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
            var dbContextNames = _helper.DbContextNamedTypeSymbols.Select(s => s.ToDisplayString()).ToArray();
            OutputHelper.Info($"📋 Found {dbContextNames.Length} DbContext types: {string.Join(", ", dbContextNames)}");

            // Shadow copy dlls to avoid locking original build output
            var originalDll = _helper.DllPath;
            var originalDir = Path.GetDirectoryName(originalDll)!;
            _shadowDir = Path.Combine(Path.GetTempPath(), "AterStudio_Shadow", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_shadowDir);
            foreach (var f in Directory.EnumerateFiles(originalDir, "*.dll", SearchOption.TopDirectoryOnly))
            {
                var target = Path.Combine(_shadowDir, Path.GetFileName(f));
                File.Copy(f, target, true);
            }
            var shadowDllPath = Path.Combine(_shadowDir, Path.GetFileName(originalDll));
            OutputHelper.Info($"📁 Shadow copy created: {_shadowDir}");

            _loadContext = new PluginLoadContext(shadowDllPath);
            _alcWeakRef = new WeakReference(_loadContext, trackResurrection: false);
            Assembly assembly;

            try
            {
                assembly = _loadContext.LoadFromAssemblyName(
                    new AssemblyName(Path.GetFileNameWithoutExtension(shadowDllPath))
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
                OutputHelper.Warning($"ReflectionTypeLoadException: {ex.Message}, got {contextTypes.Length} valid types");
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
            }

            var options = optionsBuilder.Options;
            dbContextInstance = Activator.CreateInstance(contextType, options) as DbContext;

            if (dbContextInstance != null)
            {
                OutputHelper.Info($"✅ DbContext instance created for: {contextType.Name}");
                // 在释放实例之前获取 Model
                model = dbContextInstance.Model;
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
                }
                catch (Exception ex)
                {
                    OutputHelper.Warning($"Error disposing DbContext for {contextType.Name}: {ex.Message}");
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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"❌ Error during force cleanup: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查文件是否被占用
    /// </summary>
    public static bool IsFileLocked(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch
        {
            return true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管资源
                // Unload 包含对 _loadContext (托管对象) 的操作，必须在 disposing=true 时调用
                Unload();
            }

            // 释放非托管资源
            // 目前没有纯非托管资源需要释放

            _disposed = true;
        }
    }

    private void Unload()
    {
        try
        {
            _loadContext?.Unload();
            _loadContext = null;

            // 尝试多轮 GC 以卸载 ALC
            if (_alcWeakRef != null)
            {
                for (int i = 0; i < 10 && _alcWeakRef.IsAlive; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(50);
                }
            }

            // 删除 shadow 目录
            if (_shadowDir != null && Directory.Exists(_shadowDir))
            {
                try
                {
                    Directory.Delete(_shadowDir, true);
                }
                catch (Exception ex)
                {
                    OutputHelper.Warning($"Failed to delete shadow directory {_shadowDir}: {ex.Message}");
                }
                _shadowDir = null;
            }

        }
        catch (Exception ex)
        {
            OutputHelper.Error($"❌ Error during DbContextAnalyzer unload: {ex.Message}");
        }
    }
    ~DbContextAnalyzer()
    {
        Dispose(false);
    }
}
