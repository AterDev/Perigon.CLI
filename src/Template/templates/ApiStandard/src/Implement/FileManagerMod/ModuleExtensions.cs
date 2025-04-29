using Microsoft.Extensions.Hosting;

namespace FileManagerMod;
/// <summary>
/// 模块服务扩展
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// add module
    /// </summary>
    /// <param name="builder"></param>
    public static IHostApplicationBuilder AddFileManagerMod(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}

