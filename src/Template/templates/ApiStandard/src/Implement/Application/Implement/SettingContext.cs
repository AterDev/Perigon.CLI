using Microsoft.Extensions.Configuration;

namespace Application.Implement;

/// <summary>
/// 配置上下文
/// </summary>
public class SettingContext : ISettingContext
{
    private readonly IConfiguration _configuration;
    public SettingContext(IConfiguration configuration)
    {
        _configuration = configuration;
        // TODO:获取需要全局使用的配置值
    }

    public T? GetValue<T>(string key)
    {
        return _configuration.GetValue<T>(key);
    }
}
