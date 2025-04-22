using EntityFramework.DBProvider;
using Framework.Common.Utils;

namespace Http.API.Worker;
public class InitDataTask
{
    /// <summary>
    /// 初始化应用数据
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static async Task InitDataAsync(IServiceProvider provider)
    {
        CommandDbContext context = provider.GetRequiredService<CommandDbContext>();
        ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        ILogger<InitDataTask> logger = loggerFactory.CreateLogger<InitDataTask>();
        IConfiguration configuration = provider.GetRequiredService<IConfiguration>();

        var connectionString = context.Database.GetConnectionString();
#if DEBUG
        logger.LogDebug("connectString:{cs}", connectionString);
#endif
        try
        {
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogError("数据库无法连接:{message}", connectionString);
                return;
            }
            else
            {
                // 初始化用户
                var user = await context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    await InitUserAsync(context, configuration, logger);
                }
                //await SystemMod.InitModule.InitializeAsync(provider);
                // [InitModule]
            }
        }
        catch (Exception ex)
        {
            logger.LogError("数据库连接成功，但初始化数据失败:{msg}", ex.Message);
        }
    }

    /// <summary>
    /// 初始化角色
    /// </summary>
    public static async Task InitUserAsync(CommandDbContext context, IConfiguration configuration, ILogger<InitDataTask> logger)
    {
        var defaultPassword = configuration.GetValue<string>("Key:DefaultPassword");
        if (string.IsNullOrWhiteSpace(defaultPassword))
        {
            defaultPassword = "Hello.Net";
        }
        var salt = HashCrypto.BuildSalt();

        User user = new()
        {
            UserName = "TestUser",
            Email = "TestEmail@domain",
            PasswordSalt = salt,
            PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
        };

        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            logger.LogInformation("初始化用户数据成功:{username}/{password}", user.UserName, defaultPassword);

        }
        catch (Exception ex)
        {
            logger.LogError("初始化角色用户时出错,请确认您的数据库没有数据！{message}", ex.Message);
        }
    }

}
