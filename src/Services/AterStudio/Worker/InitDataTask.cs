using Microsoft.EntityFrameworkCore;

namespace AterStudio.Worker;

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
        try
        {
            var connectionString = context.Database.GetConnectionString();
            logger.LogInformation("ℹ️ Using db file: {connectionString}", connectionString);

            // 输出当前启动端口

            CancellationTokenSource source = new(10000);
            await context.Database.MigrateAsync(source.Token);
        }
        catch (Exception e)
        {
            logger.LogError("❌ Init db failed:{message}", e.Message);
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }
    }
}
