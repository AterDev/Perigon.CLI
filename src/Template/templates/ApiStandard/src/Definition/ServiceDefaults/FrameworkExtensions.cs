using Ater.AspNetCore.Abstraction;
using Ater.AspNetCore.Services;
using Ater.AspNetCore.Toolkit.Services;
using EntityFramework.AppDbFactory;
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
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddFrameworkServices()
        {
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddScoped<ITenantContext, TenantContext>();

            var components =
                builder.Configuration.GetSection(ComponentOption.ConfigPath).Get<ComponentOption>()
                ?? throw new Exception($"can't get {ComponentOption.ConfigPath} config");

            builder.AddOptions();
            builder.AddCache(components);
            builder.AddDbFactory();
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
        public IHostApplicationBuilder AddOptions()
        {
            var config = builder.Configuration;
            builder.Services.Configure<ComponentOption>(config.GetSection(ComponentOption.ConfigPath));

            builder.Services.Configure<LoginSecurityPolicyOption>(
                config.GetSection(LoginSecurityPolicyOption.ConfigPath)
            );

            builder.Services.Configure<JwtOption>(config.GetSection(JwtOption.ConfigPath));
            builder.Services.Configure<CacheOption>(config.GetSection(CacheOption.ConfigPath));

            return builder;
        }

        /// <summary>
        /// 添加数据工厂
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IHostApplicationBuilder AddDbFactory()
        {
            builder.Services.AddSingleton<UniversalDbFactory>();
            builder.Services.AddScoped<TenantDbFactory>();
            return builder;
        }

        /// <summary>
        /// 添加数据库上下文
        /// </summary>
        /// <returns></returns>
        public IHostApplicationBuilder AddDbContext(
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
        public IHostApplicationBuilder AddCache(
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

                options.MaximumPayloadBytes = cacheOption?.MaxPayloadBytes ?? (1024 * 1024);
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
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// 仅在调试特殊异常时使用
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public WebApplication UseDomainException()
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
}
