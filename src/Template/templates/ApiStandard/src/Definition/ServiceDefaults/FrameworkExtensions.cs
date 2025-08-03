using Aspire.Microsoft.EntityFrameworkCore.SqlServer;
using Aspire.Npgsql.EntityFrameworkCore.PostgreSQL;
using Ater.Web.Convention.Abstraction;
using Ater.Web.Convention.Services;
using Ater.Web.Extension.Services;
using Microsoft.EntityFrameworkCore;
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
        builder.Services.AddPooledDbContextFactory<CommandDbContext>(options =>
        {
            ConfigureDbContextOptions(
                options,
                components.Database,
                builder.Configuration,
                WebConst.CommandDb
            );
        });
        builder.Services.AddPooledDbContextFactory<QueryDbContext>(options =>
        {
            ConfigureDbContextOptions(
                options,
                components.Database,
                builder.Configuration,
                WebConst.QueryDb
            );
        });

        builder.Services.AddScoped<DbContextFactory>();
        builder.Services.AddScoped<TenantDbContextFactory>();
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
        builder.Services.AddScoped(typeof(DataAccessContext<>));
        builder.Services.AddScoped<DataAccessContext>();

        builder.Services.AddDbContextPool<CommandDbContext>(options =>
        {
            ConfigureDbContextOptions(
                options,
                components.Database,
                builder.Configuration,
                WebConst.CommandDb
            );
        });

        builder.Services.AddDbContextPool<QueryDbContext>(options =>
        {
            ConfigureDbContextOptions(
                options,
                components.Database,
                builder.Configuration,
                WebConst.QueryDb
            );
        });

        EnrichDbContext(builder, components.Database);
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
            builder.AddRedisDistributedCache(WebConst.Cache);
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
    /// 配置 DbContextOptionsBuilder
    /// </summary>
    private static void ConfigureDbContextOptions(
        DbContextOptionsBuilder options,
        DatabaseType dbType,
        IConfiguration configuration,
        string connectionName
    )
    {
        var connectionString = configuration.GetConnectionString(connectionName);
        switch (dbType)
        {
            case DatabaseType.SqlServer:
                options.UseSqlServer(connectionString);
                break;
            case DatabaseType.PostgreSql:
                options.UseNpgsql(connectionString);
                break;
            default:
                throw new NotSupportedException($"Database provider {dbType} is not supported.");
        }
#if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
#endif
    }

    /// <summary>
    /// 配置 DbContext settings
    /// </summary>
    private static void EnrichDbContext(IHostApplicationBuilder builder, DatabaseType databaseType)
    {
        void ConfigureDbContextSettings(dynamic settings)
        {
            settings.CommandTimeout = 60;
#if DEBUG
            settings.DisableRetry = true;
#endif
            if (settings is MicrosoftEntityFrameworkCoreSqlServerSettings sqlServerSettings)
            {
                // other SQL Server specific settings can be configured here
            }
            else if (settings is NpgsqlEntityFrameworkCorePostgreSQLSettings npgsqlSettings)
            {
                // other PostgreSQL specific settings can be configured here
            }
        }
        switch (databaseType)
        {
            case DatabaseType.SqlServer:
                builder.EnrichSqlServerDbContext<CommandDbContext>(ConfigureDbContextSettings);
                builder.EnrichSqlServerDbContext<QueryDbContext>(ConfigureDbContextSettings);

                break;
            case DatabaseType.PostgreSql:
                builder.EnrichNpgsqlDbContext<CommandDbContext>(ConfigureDbContextSettings);
                builder.EnrichNpgsqlDbContext<QueryDbContext>(ConfigureDbContextSettings);
                break;
            default:
                throw new NotSupportedException(
                    $"Database provider {databaseType} is not supported."
                );
        }
    }
}
