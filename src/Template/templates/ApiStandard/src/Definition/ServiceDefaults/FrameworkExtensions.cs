using Framework.Web.Convention.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace ServiceDefaults;
/// <summary>
/// 应用扩展服务
/// </summary>
public static class FrameworkExtensions
{
    public static IHostApplicationBuilder AddFrameworkServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<UserContext>();
        builder.Services.AddScoped<TenantProvider>();

        var components = builder.Configuration.GetSection(ComponentOption.ConfigPath)
            .Get<ComponentOption>()
            ?? throw new Exception($"can't get {ComponentOption.ConfigPath} config");

        builder.AddOptions(components);
        builder.AddCache(components);
        builder.Services.AddDbFactory();
        builder.AddDbContext(components);

        return builder;
    }


    /// <summary>
    /// config options
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="components"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IHostApplicationBuilder AddOptions(this IHostApplicationBuilder builder, ComponentOption components)
    {
        var config = builder.Configuration;
        builder.Services.Configure<ComponentOption>(config.GetSection(ComponentOption.ConfigPath));

        builder.Services.Configure<LoginSecurityPolicyOption>(config.GetSection(LoginSecurityPolicyOption.ConfigPath));

        builder.Services.Configure<JwtOption>(config.GetSection(JwtOption.ConfigPath));
        builder.Services.Configure<CacheOption>(config.GetSection(CacheOption.ConfigPath));

        if (components.UseSmtp)
        {
            builder.Services.Configure<SmtpOption>(config.GetSection(SmtpOption.ConfigPath));
        }
        if (components.UseSMS)
        {
            builder.Services.Configure<SMSOption>(config.GetSection(SMSOption.ConfigPath));
        }
        if (components.UseAWSS3)
        {
            builder.Services.Configure<AWSS3Option>(config.GetSection(AWSS3Option.ConfigPath));
        }
        return builder;
    }

    /// <summary>
    /// 添加数据工厂
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDbFactory(this IServiceCollection services)
    {
        services.AddScoped(typeof(DbContextFactory));
        services.AddScoped(typeof(TenantDbContextFactory));
        return services;
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbContext(this IHostApplicationBuilder builder, ComponentOption components)
    {
        builder.Services.AddScoped(typeof(DataAccessContext<>));
        builder.Services.AddScoped(typeof(DataAccessContext));

        switch (components.Database)
        {
            case DatabaseType.SqlServer:
                builder.Services.AddDbContext<CommandDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString(WebConst.CommandDb)));
                builder.Services.AddDbContext<QueryDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString(WebConst.QueryDb)));
                break;
            case DatabaseType.PostgreSql:
                builder.Services.AddDbContext<CommandDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString(WebConst.CommandDb)));
                builder.Services.AddDbContext<QueryDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString(WebConst.QueryDb)));
                break;
            default:
                throw new NotSupportedException($"Database provider {components.Database} is not supported.");
        }
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="components"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddCache(this IHostApplicationBuilder builder, ComponentOption components)
    {
        // 默认支持内存缓存
        builder.Services.AddMemoryCache();

        // 分布式缓存
        if (components.Cache != CacheType.Memory)
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(WebConst.Cache);
                options.InstanceName = WebConst.ProjectName;
            });
        }
        // 混合缓存
        var cacheOption = builder.Configuration.GetSection(CacheOption.ConfigPath).Get<CacheOption>();
        builder.Services.AddHybridCache(options =>
        {
            HybridCacheEntryFlags flags = components.Cache switch
            {
                CacheType.Memory => HybridCacheEntryFlags.DisableDistributedCache,
                CacheType.Redis => HybridCacheEntryFlags.DisableLocalCache,
                _ => HybridCacheEntryFlags.None
            };

            options.MaximumPayloadBytes = cacheOption?.MaxPayloadBytes ?? 1024 * 1024;
            options.MaximumKeyLength = cacheOption?.MaxKeyLength ?? 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Flags = flags,
                Expiration = TimeSpan.FromMinutes(cacheOption?.Expiration ?? 20),
                LocalCacheExpiration = TimeSpan.FromMinutes(cacheOption?.LocalCacheExpiration ?? 10)
            };
        });

        builder.Services.AddSingleton<CacheService>();
        return builder;
    }
}
