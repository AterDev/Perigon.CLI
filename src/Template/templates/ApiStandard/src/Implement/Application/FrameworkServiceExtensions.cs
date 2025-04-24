using EntityFramework.DBProvider;
using Framework.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SharedModule.Const;

namespace SharedModule;
/// <summary>
/// 应用扩展服务
/// </summary>
public static partial class FrameworkServiceExtensions
{
    /// <summary>
    /// 添加数据工厂
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDbFactory(this IServiceCollection services)
    {
        services.AddScoped(typeof(DbContextFactory<>));
        return services;
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(DataAccessContext<>));
        builder.Services.AddScoped(typeof(DataAccessContext));

        builder.AddSqlServerDbContext<QueryDbContext>(WebConst.QueryDb);
        builder.AddSqlServerDbContext<CommandDbContext>(WebConst.CommandDb);
        return builder;
    }

    /// <summary>
    /// add cache config
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddCache(this IHostApplicationBuilder builder)
    {
        // redis 客户端
        builder.AddRedisClient(connectionName: "cache");
        // 分布式缓存
        var cache = builder.Configuration.GetConnectionString(WebConst.Cache);
        if (cache.NotEmpty() && cache != "Memory")
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(WebConst.Cache);
                options.InstanceName = Constant.ProjectName;
            });
        }
        else
        {
            builder.Services.AddDistributedMemoryCache();
        }
        // 内存缓存
        builder.Services.AddMemoryCache();
        return builder;
    }
}
