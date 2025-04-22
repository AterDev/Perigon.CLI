// 本文件由 ater.dry工具自动生成.
using CMSMod.Managers;

using Microsoft.Extensions.DependencyInjection;

namespace CMSMod;
/// <summary>
/// 服务注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加CMSMod 注入服务
    /// </summary>
    /// <param name="services"></param>
    public static void AddCMSModManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(BlogManager));

    }
}

