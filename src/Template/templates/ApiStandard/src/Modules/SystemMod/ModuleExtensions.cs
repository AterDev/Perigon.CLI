using Microsoft.Extensions.Hosting;
using SystemMod.Managers;
using SystemMod.Worker;

namespace SystemMod;

/// <summary>
/// 服务注入扩展
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// 添加模块服务
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddSystemMod(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<SystemUserRoleManager>();
        builder.Services.AddSingleton<IEntityTaskQueue<SystemLogs>, EntityTaskQueue<SystemLogs>>();
        builder.Services.AddSingleton<SystemLogService>();
        builder.Services.AddHostedService<SystemLogTaskHostedService>();
        return builder;
    }
}
