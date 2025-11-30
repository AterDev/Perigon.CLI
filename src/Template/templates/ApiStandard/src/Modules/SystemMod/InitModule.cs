using Entity.CommonMod;
using System.Text.Json;

namespace SystemMod;

public class InitModule
{
    /// <summary>
    /// 模块初始化方法
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var context = provider.GetRequiredService<DefaultDbContext>();
        var logger = loggerFactory.CreateLogger<InitModule>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var cache = provider.GetRequiredService<CacheService>();

        try
        {
            var hasTenant = await context
                .Tenants
                .AnyAsync();
            if (!hasTenant)
            {
                logger.LogInformation("⛏️ Start init [System] Module");
                await InitTenantAdminAccountAsync(context);
                await InitConfigAsync(context, configuration, logger);
            }

            logger.LogInformation("✅ Database check!");
            await InitCacheAsync(context, cache, logger);
            logger.LogInformation("✅ Cache check!");
        }
        catch (Exception ex)
        {
            var conn = context
                .Database
                .GetConnectionString();
            logger.LogError("Failed to initialize system configuration! {message}. ", ex.Message);
        }
    }

    private static async Task InitTenantAdminAccountAsync(DefaultDbContext context)
    {
        var domain = "default.com";
        var defaultPassword = "Ater.Default.2026";
        var tenant = new Tenant()
        {
            Domain = domain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
        };
        var superRole = new SystemRole()
        {
            Name = WebConst.SuperAdmin,
            NameValue = WebConst.SuperAdmin,
            TenantId = tenant.Id,
        };

        var adminRole = new SystemRole()
        {
            Name = WebConst.AdminUser,
            NameValue = WebConst.AdminUser,
            TenantId = tenant.Id,
        };
        var salt = HashCrypto.BuildSalt();
        var adminUser = new SystemUser()
        {
            UserName = "admin",
            Email = $"admin@{domain}",
            PasswordSalt = salt,
            PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
            SystemRoles = [superRole, adminRole],
            TenantId = tenant.Id,
        };

        context.Add(tenant);
        context.Add(adminUser);
        await context.SaveChangesAsync();

        Console.WriteLine($"✨ Created admin for {domain} : {adminUser.Email}/{defaultPassword}");
    }

    /// <summary>
    /// 初始化配置
    /// </summary>
    /// <param name="context"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static async Task InitConfigAsync(
        DefaultDbContext context,
        IConfiguration configuration,
        ILogger logger
    )
    {
        // 初始化配置信息
        var initConfig = SystemConfig.NewSystemConfig(
            WebConst.SystemGroup,
            WebConst.IsInit,
            "true"
        );

        var loginSecurityPolicy =
            configuration.GetSection(WebConst.LoginSecurityPolicy).Get<LoginSecurityPolicyOption>()
            ?? new LoginSecurityPolicyOption();

        var loginSecurityPolicyConfig = SystemConfig.NewSystemConfig(
            WebConst.SystemGroup,
            WebConst.LoginSecurityPolicy,
            JsonSerializer.Serialize(loginSecurityPolicy)
        );

        context
            .SystemConfigs
            .Add(loginSecurityPolicyConfig);
        context
            .SystemConfigs
            .Add(initConfig);

        await context.SaveChangesAsync();
        logger.LogInformation("写入登录安全策略成功");
    }

    /// <summary>
    /// 加载配置缓存
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cache"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static async Task InitCacheAsync(
        DefaultDbContext context,
        CacheService cache,
        ILogger logger
    )
    {
        logger.LogInformation("加载配置缓存");
        var securityPolicy = context
            .SystemConfigs.Where(c => c
            .Key
            .Equals(WebConst.LoginSecurityPolicy))
            .Where(c => c
            .GroupName
            .Equals(WebConst.SystemGroup))
            .Select(c => c.Value)
            .FirstOrDefault();

        if (securityPolicy != null)
        {
            await cache.SetValueAsync(WebConst.LoginSecurityPolicy, securityPolicy, null);
        }
    }
}
