using Microsoft.Extensions.Caching.Memory;

namespace Share.Services;

/// <summary>
/// 简单内存缓存服务
/// </summary>
public class CacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetSlidingExpiration(expiration.Value);
        }
        _cache.Set(key, value, options);
    }

    public T? Get<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cache.TryGetValue(key, out var obj) && obj is T val ? val : default;
    }

    public void Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _cache.Remove(key);
    }
}
