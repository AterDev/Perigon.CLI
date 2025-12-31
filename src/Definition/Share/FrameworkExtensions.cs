using CodeGenerator.Helper;
using Entity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Perigon.MiniDb;
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
        var dir = AssemblyHelper.GetStudioPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var path = Path.Combine(dir, ConstVal.DbName);

        MiniDbConfiguration.AddDbContext<DefaultDbContext>(config =>
        {
            config.UseMiniDb(path);
        });
        builder.Services.AddSingleton<DefaultDbContext>();
        
        return builder;
    }
}
