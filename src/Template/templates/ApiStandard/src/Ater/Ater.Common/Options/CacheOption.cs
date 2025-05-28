namespace Ater.Common.Options;
/// <summary>
/// 缓存配置
/// </summary>
public class CacheOption
{
    public const string ConfigPath = "Cache";
    public int MaxPayloadBytes { get; set; } = 1024 * 1024;
    public int MaxKeyLength { get; set; } = 1024;
    /// <summary>
    /// minute
    /// </summary>
    public int Expiration { get; set; } = 20;
    /// <summary>
    /// minute
    /// </summary>
    public int LocalCacheExpiration { get; set; } = 10;
}
