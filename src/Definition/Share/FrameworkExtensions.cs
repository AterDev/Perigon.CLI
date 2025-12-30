using DataContext.DBProvider;
using Entity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share.Services;

namespace Share;

/// <summary>
/// 服务注册扩展
/// </summary>
public static partial class FrameworkExtensions
{
    /// <summary>
    /// 添加默认应用组件 for MiniDb
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddFrameworkServices(this IHostApplicationBuilder builder)
    {
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

        builder.AddDbContext();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<CacheService>();
        return builder;
    }

    /// <summary>
    /// 添加数据库上下文 - MiniDb
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbContext(this IHostApplicationBuilder builder)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Data");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        var dbPath = Path.Combine(dir, "app.db");
        builder.Services.AddSingleton(new DefaultDbContext(dbPath));
        builder.Services.AddScoped<IProjectContext, ProjectContext>();
        
        return builder;
    }
}
