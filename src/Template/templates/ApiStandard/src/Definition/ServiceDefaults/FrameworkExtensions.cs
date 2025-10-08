using Ater.Common;
using Ater.Web.Convention.Abstraction;
using Ater.Web.Convention.Services;
using Ater.Web.Extension.Services;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace ServiceDefaults;

/// <summary>
/// 应用扩展服务
/// </summary>
public static class FrameworkExtensions
{
    public static IHostApplicationBuilder AddFrameworkServices(this IHostApplicationBuilder builder)
    {
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<ITenantProvider, TenantProvider>();

        var components =
            builder.Configuration.GetSection(ComponentOption.ConfigPath).Get<ComponentOption>()
            ?? throw new Exception($"can't get {ComponentOption.ConfigPath} config");

        builder.AddOptions(components);
        builder.AddCache(components);
        builder.AddDbFactory(components);
        builder.AddDbContext(components);

        builder.Services.AddScoped<JwtService>();
        builder.Services.AddScoped<SmtpService>();
        return builder;
    }

    /// <summary>
    /// config options
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="components"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IHostApplicationBuilder AddOptions(
        this IHostApplicationBuilder builder,
        ComponentOption components
    )
    {
        var config = builder.Configuration;
        builder.Services.Configure<ComponentOption>(config.GetSection(ComponentOption.ConfigPath));

        builder.Services.Configure<LoginSecurityPolicyOption>(
            config.GetSection(LoginSecurityPolicyOption.ConfigPath)
        );

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
    public static IHostApplicationBuilder AddDbFactory(
        this IHostApplicationBuilder builder,
        ComponentOption components
    )
    {
        builder.Services.AddSingleton<DbContextFactory>();
        //builder.Services.AddScoped<TenantDbContextFactory>();
        return builder;
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbContext(
        this IHostApplicationBuilder builder,
        ComponentOption components
    )
    {
        switch (components.Database)
        {
            case DatabaseType.SqlServer:
                builder.AddSqlServerDbContext<DefaultDbContext>(AppConst.Default);
                break;

            case DatabaseType.PostgreSql:
                builder.AddNpgsqlDbContext<DefaultDbContext>(AppConst.Default);
                break;
        }
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="components"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddCache(
        this IHostApplicationBuilder builder,
        ComponentOption components
    )
    {
        // 默认支持内存缓存
        builder.Services.AddMemoryCache();

        // 分布式缓存
        if (components.Cache != CacheType.Memory)
        {
            builder.AddRedisDistributedCache(AppConst.Cache);
        }
        // 混合缓存
        var cacheOption = builder
            .Configuration.GetSection(CacheOption.ConfigPath)
            .Get<CacheOption>();
        builder.Services.AddHybridCache(options =>
        {
            HybridCacheEntryFlags? flags = components.Cache switch
            {
                CacheType.Memory => HybridCacheEntryFlags.DisableDistributedCache,
                CacheType.Redis => HybridCacheEntryFlags.DisableLocalCache,
                _ => HybridCacheEntryFlags.None,
            };

            options.MaximumPayloadBytes = cacheOption?.MaxPayloadBytes ?? 1024 * 1024;
            options.MaximumKeyLength = cacheOption?.MaxKeyLength ?? 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Flags = flags,
                Expiration = TimeSpan.FromMinutes(cacheOption?.Expiration ?? 20),
                LocalCacheExpiration = TimeSpan.FromMinutes(
                    cacheOption?.LocalCacheExpiration ?? 10
                ),
            };
        });

        builder.Services.AddSingleton<CacheService>();
        return builder;
    }

    /// <summary>
    /// 仅在调试特殊异常时使用
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseDomainException(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<WebApplication>>();
        AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
        {
            if (eventArgs.Exception is OutOfMemoryException)
            {
                logger.LogError(
                    "=== OutOfMemory: {message}, {stackTrace}",
                    eventArgs.Exception.Message,
                    eventArgs.Exception.StackTrace
                );
            }
            else
            {
                logger.LogError(
                    "=== FirstChanceException: {message}, {stackTrace}",
                    eventArgs.Exception.Message,
                    eventArgs.Exception.StackTrace
                );
            }
        };
        return app;
    }
}
