using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Ater.Web.Convention.Services;

/// <summary>
/// 简单封装对象的存储和获取
/// </summary>
public class CacheService(
    HybridCache cache,
    IOptions<CacheOption> options,
    IOptions<ComponentOption> component
)
{
    //public HybridCache Cache { get; init; } = cache;

    /// <summary>
    /// 缓存存储
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration">seconds</param>
    /// <returns></returns>
    public async Task SetValueAsync<T>(string key, T data)
    {
        await cache.SetAsync(key, data);
    }

    /// <summary>
    /// 保存到缓存
    /// </summary>
    /// <param name="data">值</param>
    /// <param name="key">键</param>
    /// <param name="expiration">绝对过期时间</param>
    /// <returns></returns>
    public async Task SetValueAsync<T>(
        string key,
        T data,
        int? expiration = null,
        int? localExpiration = null,
        HybridCacheEntryFlags? flags = null
    )
    {
        var cacheOption = options.Value;
        var entryOption = new HybridCacheEntryOptions()
        {
            Expiration = expiration.HasValue
                ? TimeSpan.FromSeconds(expiration.Value)
                : TimeSpan.FromMinutes(cacheOption.Expiration),
            Flags = flags ?? GetGlobalCacheFlags(cacheOption),
            LocalCacheExpiration = localExpiration.HasValue
                ? TimeSpan.FromMinutes(localExpiration.Value)
                : TimeSpan.FromMinutes(cacheOption.LocalCacheExpiration),
        };
        await cache.SetAsync(key, data, entryOption);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellation = default)
    {
        var cachedValue = await cache.GetOrCreateAsync<T?>(
            key,
            factory => default,
            cancellationToken: cancellation
        );
        return cachedValue;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        CancellationToken cancellation = default
    )
    {
        var cachedValue = await cache.GetOrCreateAsync(
            key,
            factory,
            cancellationToken: cancellation
        );
        return cachedValue;
    }

    private HybridCacheEntryFlags GetGlobalCacheFlags(CacheOption cacheOption)
    {
        var components = component.Value;
        return components.Cache switch
        {
            CacheType.Memory => HybridCacheEntryFlags.DisableDistributedCache,
            CacheType.Redis => HybridCacheEntryFlags.DisableLocalCache,
            _ => HybridCacheEntryFlags.None,
        };
    }
}
