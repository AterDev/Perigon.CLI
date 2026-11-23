using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share.Services;

namespace Share;

/// <summary>
/// 服务注册扩展
/// </summary>
public static partial class FrameworkExtensions
{
    public static IHostApplicationBuilder AddFrameworkServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<CacheService>();
        return builder;
    }
}
