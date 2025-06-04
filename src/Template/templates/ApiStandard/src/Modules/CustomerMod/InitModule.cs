using Microsoft.Extensions.Configuration;
namespace CustomerMod;
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
        CommandDbContext context = provider.GetRequiredService<CommandDbContext>();
        ILogger<InitModule> logger = loggerFactory.CreateLogger<InitModule>();
        IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
        try
        {
           // TODO:初始化逻辑
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError("初始化CustomerMod失败！{message}", ex.Message);
        }
    }
}