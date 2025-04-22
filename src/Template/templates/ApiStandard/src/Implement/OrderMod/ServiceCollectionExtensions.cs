// 本文件由 ater.dry工具自动生成.
using OrderMod.Managers;

namespace OrderMod;
/// <summary>
/// 服务注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加OrderMod 注入服务
    /// </summary>
    /// <param name="services"></param>
    public static void AddOrderModManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(OrderManager));
        services.AddScoped(typeof(ProductManager));

    }
}

