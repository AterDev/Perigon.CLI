// 本文件由 ater.dry工具自动生成.
using SystemMod.Services;
using SystemMod.Worker;

namespace SystemMod;
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
    public static IServiceCollection AddSystemModServices(this IServiceCollection services)
    {
        services.AddSystemModManagers();
        services.AddSingleton<IEntityTaskQueue<SystemLogs>, EntityTaskQueue<SystemLogs>>();
        services.AddSingleton(typeof(SystemLogService));
        services.AddHostedService<SystemLogTaskHostedService>();
        return services;
    }

    /// <summary>
    /// 添加Manager服务
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddSystemModManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(SystemConfigManager));
        services.AddScoped(typeof(SystemLogsManager));
        services.AddScoped(typeof(SystemMenuManager));
        services.AddScoped(typeof(SystemPermissionGroupManager));
        services.AddScoped(typeof(SystemPermissionManager));
        services.AddScoped(typeof(SystemRoleManager));
        services.AddScoped(typeof(SystemUserManager));
        return services;
    }
}

