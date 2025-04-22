using EntityFramework.DBProvider;

using Microsoft.Extensions.Configuration;

namespace OrderMod;
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

        logger.LogInformation("⛏️ 订单模块初始化");
        try
        {
            await InitProductAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError("❌ 订单模块初始化失败！{message}", ex.Message);
        }
    }

    /// <summary>
    /// 初始化产品
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static async Task InitProductAsync(CommandDbContext context, ILogger logger)
    {
        // 初始化产品信息
        if (!context.Products.Any())
        {
            var product = new Product
            {
                Name = "试用",
                Price = 9.9m,
                Description = "试用产品",
                OriginPrice = 9.9m,
                ProductType = ProductType.Trial,
            };
            context.Products.Add(product);

            await context.SaveChangesAsync();
            logger.LogInformation("✅ 初始化产品成功");
        }
    }
}
