using EntityFramework.DBProvider;

namespace Http.API.Worker;
public class Initialize
{
    /// <summary>
    /// 检查数据
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static async Task InitAsync(IServiceProvider provider)
    {
        CommandDbContext context = provider.GetRequiredService<CommandDbContext>();
        ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        ILogger<Initialize> logger = loggerFactory.CreateLogger<Initialize>();
        IConfiguration configuration = provider.GetRequiredService<IConfiguration>();


        try
        {
            CacheService cache = provider.GetRequiredService<CacheService>();
        }
        catch (Exception ex)
        {

            logger.LogError("初始化系统配置失败！{message}. ", ex.Message);
            throw;
        }



        var connectionString = context.Database.GetConnectionString();
#if DEBUG
        logger.LogDebug("connectString:{cs}", connectionString);
#endif
        await SystemMod.InitModule.InitializeAsync(provider);
    }
}
