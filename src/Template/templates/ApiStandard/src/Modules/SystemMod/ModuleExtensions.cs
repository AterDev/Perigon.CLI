// 本文件由 ater.dry工具自动生成.
using Microsoft.Extensions.Hosting;
using SystemMod.Services;
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
        builder.Services.AddSingleton<IEntityTaskQueue<SystemLogs>, EntityTaskQueue<SystemLogs>>();
        builder.Services.AddSingleton(typeof(SystemLogService));
        builder.Services.AddHostedService<SystemLogTaskHostedService>();
        return builder;
    }
}

