// 本文件由 ater.dry工具自动生成.
using CustomerMod.Managers;

namespace CustomerMod;
/// <summary>
/// 服务注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加模块服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomerModServices(this IServiceCollection services)
    {
        services.AddCustomerModManagers();
        // add other services
        return services;
    }

    /// <summary>
    /// 添加CustomerMod 注入服务
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddCustomerModManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(CustomerInfoManager));
        return services;
    }
}

