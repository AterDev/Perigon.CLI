using System.Text.Json;
using Ater.Common.Options;
using EntityFramework.DBProvider;

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
        ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        DefaultDbContext context = provider.GetRequiredService<DefaultDbContext>();
        ILogger<InitModule> logger = loggerFactory.CreateLogger<InitModule>();
        IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
        CacheService cache = provider.GetRequiredService<CacheService>();

        try
        {
            var isInitString = context
                .SystemConfigs.Where(c => c.Key.Equals(WebConst.IsInit))
                .Where(c => c.GroupName.Equals(WebConst.SystemGroup))
                .Select(c => c.Value)
                .FirstOrDefault();

            // 未初始化时
            if (isInitString.IsEmpty() || isInitString.Equals("false"))
            {
                logger.LogInformation("⛏️ 开始初始化[System]模块");
                await InitRoleAndUserAsync(context, logger, configuration);
                // 初始化配置
                await InitConfigAsync(context, configuration, logger);
            }

            logger.LogInformation("✅ Database check!");
            await InitCacheAsync(context, cache, logger);
            logger.LogInformation("✅ Cache check!");
        }
        catch (Exception ex)
        {
            var conn = context.Database.GetConnectionString();
            logger.LogError("初始化系统配置失败！{message}. ", ex.Message);
        }
    }

    private static async Task InitRoleAndUserAsync(
        DefaultDbContext context,
        ILogger<InitModule> logger,
        IConfiguration configuration
    )
    {
        SystemRole? role = await context.SystemRoles.SingleOrDefaultAsync(r =>
            r.NameValue == WebConst.SuperAdmin
        );
        // 初始化管理员账号和角色
        if (role == null)
        {
            var defaultPassword = configuration.GetValue<string>("Key:DefaultPassword");
            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                defaultPassword = "Hello.Net";
            }
            SystemRole superRole = new()
            {
                Name = WebConst.SuperAdmin,
                NameValue = WebConst.SuperAdmin,
            };
            SystemRole adminRole = new()
            {
                Name = WebConst.AdminUser,
                NameValue = WebConst.AdminUser,
            };
            var salt = HashCrypto.BuildSalt();
            SystemUser adminUser = new()
            {
                UserName = "admin",
                PasswordSalt = salt,
                PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
                SystemRoles = [superRole, adminRole],
            };

            SystemUser manager = new()
            {
                UserName = "manager",
                PasswordSalt = salt,
                PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
                SystemRoles = [superRole],
            };

            context.SystemUsers.Add(adminUser);
            context.SystemUsers.Add(manager);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "初始化管理员账号:{username}/{password}",
                adminUser.UserName,
                defaultPassword
            );
        }
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

        context.SystemConfigs.Add(loginSecurityPolicyConfig);
        context.SystemConfigs.Add(initConfig);

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
            .SystemConfigs.Where(c => c.Key.Equals(WebConst.LoginSecurityPolicy))
            .Where(c => c.GroupName.Equals(WebConst.SystemGroup))
            .Select(c => c.Value)
            .FirstOrDefault();

        if (securityPolicy != null)
        {
            await cache.SetValueAsync(WebConst.LoginSecurityPolicy, securityPolicy, null);
        }
    }
}
