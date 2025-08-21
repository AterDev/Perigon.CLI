using Microsoft.Extensions.Hosting;

namespace UserMod;

/// <summary>
/// 模块服务扩展
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// add module
    /// </summary>
    /// <param name="builder"></param>
    public static IHostApplicationBuilder AddUserMod(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}
