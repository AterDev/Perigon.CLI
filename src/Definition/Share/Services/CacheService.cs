using Microsoft.Extensions.Caching.Memory;

namespace Share.Services;

/// <summary>
/// 内存缓存服务
/// </summary>
public class CacheService(IMemoryCache cache)
{
    private readonly IMemoryCache _cache = cache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 设置缓存（默认过期时间5分钟）
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (value is null)
        {
            _cache.Remove(key);
            return;
        }

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
        };
        _cache.Set(key, value, options);
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    public T? Get<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cache.TryGetValue(key, out var obj) && obj is T val ? val : default;
    }

    /// <summary>
    /// 删除缓存
    /// </summary>
    public void Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _cache.Remove(key);
    }
}
