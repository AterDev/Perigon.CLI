using Microsoft.Extensions.Configuration;

namespace Share;

/// <summary>
/// 配置上下文
/// </summary>
public class SettingContext(IConfiguration configuration)
{
    public T? GetValue<T>(string key)
    {
        return configuration.GetValue<T>(key);
    }
}
