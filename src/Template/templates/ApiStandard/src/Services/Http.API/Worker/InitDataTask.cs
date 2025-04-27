using Entity.UserMod;
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
                // 初始化逻辑
            }
        }
        catch (Exception ex)
        {
            logger.LogError("数据库连接成功，但初始化数据失败:{msg}", ex.Message);
        }
    }
}
