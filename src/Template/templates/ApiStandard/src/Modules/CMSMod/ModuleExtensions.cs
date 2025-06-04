using Microsoft.Extensions.Hosting;

namespace CMSMod;
/// <summary>
/// 模块服务扩展
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// add module
    /// </summary>
    /// <param name="builder"></param>
    public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}

